using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using Checkout.TakeHomeChallenge.PaymentGateway.Exceptions;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;
using Checkout.TakeHomeChallenge.PaymentGateway.Services;
using Checkout.TakeHomeChallenge.PaymentGateway.Storage;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Controllers.Logic;

internal sealed class PaymentControllerLogic : IPaymentControllerLogic
{
    private readonly IPaymentInitializedEventStorage _storage;
    private readonly IMerchantStorage _merchants;
    private readonly IPaymentsService _paymentsService;
    private readonly ILogger<PaymentControllerLogic> _logger;

    public PaymentControllerLogic(IPaymentsService paymentsService,
        ILogger<PaymentControllerLogic> logger,
        IPaymentInitializedEventStorage storage,
        IMerchantStorage merchants)
    {
        _paymentsService = paymentsService;
        _logger = logger;
        _storage = storage;
        _merchants = merchants;
    }

    public async Task<Result<PaymentAcceptedResponse>> MakePaymentAsync(PaymentRequest request)
    {
        // try to find the merchant
        var merchant = await _merchants.GetAsync(request.MerchantId);
        switch (merchant.Success)
        {
            case false when merchant.Code is FailureCode.NotFound:
                _logger.LogInformation("Merchant with id {MerchantId} was not found.", request.MerchantId);
                return Result<PaymentAcceptedResponse>.Fail("Merchant not found", FailureCode.NotFound);
            case false:
                throw new PaymentGatewayException($"Could not retrieve merchant: {merchant.Reason}");
        }
        
        // generate payment id in a deterministic way based on idempotency key and merchant id
        var paymentId = new PaymentId(Deterministic.Create(merchant.Value!.Id, request.IdempotencyId.ToString()));
        _logger.LogInformation("Requested payment was given the id {PaymentId}", paymentId);

        var saveResult = await _storage.SaveAsync(new PaymentInitializedEvent
        {
            Merchant = merchant.Value,
            MerchantId = merchant.Value.Id,
            PaymentId = paymentId.Value,
            SenderCardLastFourDigits = request.CardNumber[^4..],
            Amount = request.Value.Amount,
            Currency = request.Value.Currency,
            PaymentReference = paymentId.ToPaymentReference()
        });

        if (saveResult.Success)
        {
            _paymentsService.StartPayment(paymentId, request, merchant.Value);
            _logger.LogInformation("Payment {PaymentId} has been started successfully", paymentId);
        }
        else
        {
            _logger.LogWarning("Request for payment {PaymentId} appears to be a duplicate. " +
                               "Skipping the payment start and responding as if it has already been started.", 
                paymentId);
        }
        
        return new PaymentAcceptedResponse
        {
            PaymentId = paymentId,
            Location = new Uri($"{Routes.PaymentsResource.Base}/{paymentId}")
        };
    }

    public async Task<Result<PaymentProcessedResponse>> GetPaymentAsync(PaymentId paymentId, 
        CancellationToken cancellationToken = default)
    {
        var response = await _paymentsService.GetPaymentResultAsync(paymentId, cancellationToken);
        return response ?? Result<PaymentProcessedResponse>.Fail(
            "Payment with that id is not found.", FailureCode.NotFound);
    }
}