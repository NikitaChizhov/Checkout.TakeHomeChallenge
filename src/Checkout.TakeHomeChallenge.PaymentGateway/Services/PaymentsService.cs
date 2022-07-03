using System.Collections.Concurrent;
using System.Transactions;
using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using Checkout.TakeHomeChallenge.PaymentGateway.Exceptions;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;
using Checkout.TakeHomeChallenge.PaymentGateway.Storage;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Services;

internal sealed class PaymentsService : IPaymentsService
{
    private readonly ILogger<PaymentsService> _logger;
    private readonly IAcquiringBankClient _client;
    private readonly IPaymentProcessedEventStorage _storage;
    
    private readonly ConcurrentDictionary<PaymentId, Task<PaymentProcessedResponse>> _paymentTasks = new();

    public PaymentsService(IAcquiringBankClient client,
        IPaymentProcessedEventStorage storage,
        ILogger<PaymentsService> logger)
    {
        _client = client;
        _storage = storage;
        _logger = logger;
    }

    public void StartPayment(PaymentId paymentId, PaymentRequest request, Merchant merchant)
    {
        if (!_paymentTasks.TryAdd(paymentId, ProcessPaymentAsync(paymentId, request, merchant)))
        {
            throw new PaymentServiceConcurrencyException(
                "Unexpected situation - could not add payment task, even though payment id must have been new");
        }
    }

    public async Task<PaymentProcessedResponse?> GetPaymentResultAsync(PaymentId paymentId, 
        CancellationToken cancellationToken = default)
    {
        if (_paymentTasks.TryRemove(paymentId, out var task)) return await task;
        
        _logger.LogInformation("Requested payment response for {PaymentId} is not found in memory, " +
                               "trying to retrieve it from storage", paymentId);
        var r = await _storage.GetAsync(paymentId, cancellationToken);
        if (r is null)
        {
            _logger.LogWarning("Payment {PaymentId} is not found in memory or storage.", paymentId);
            return null;
        }

        return new PaymentProcessedResponse
        {
            PaymentId = paymentId,
            Status = r.Status switch
            {
                PaymentStatus.Completed => Status.Completed,
                PaymentStatus.Rejected => Status.Rejected,
                _ => throw new ArgumentOutOfRangeException()
            },
            SenderCardLastFourDigits = r.Info!.SenderCardLastFourDigits,
            PaymentReference = paymentId.ToPaymentReference()
        };
    }

    private async Task<PaymentProcessedResponse> ProcessPaymentAsync(PaymentId id,
        PaymentRequest request,
        Merchant merchant)
    {
        _logger.LogInformation("Starting to process payment {PaymentId}", id);
        var transactionRequest = new TransactionRequest
        {
            Amount = request.Value.Amount,
            CurrencyCode = request.Value.Currency.ToISOString(),
            PaymentReference = id.ToPaymentReference(),
            RecipientBankAccountNumber = merchant.BankAccountNumber,
            RecipientBankIdentifierCode = merchant.BankIdentifierCode,
            SenderName = request.Name,
            SenderCardNumber = request.CardNumber,
            SenderCardExpiryDate = request.CardExpiryDate,
            SenderCardVerificationValue = request.CardVerificationValue
        };
        
        var response = await _client.MakeTransactionRequestAsync(transactionRequest, 
            request.IdempotencyId.ToString());

        if (!response.Success)
        {
            throw new AcquiringBankException("Could not complete the transaction. Reason: " + response.Reason);
        }

        var transactionResponse = response.Value!;

        if (transactionResponse.Status == Status.Accepted)
        {
            _logger.LogWarning("Bank API responded to transaction with status Accepted, even though" +
                               "it should not have responded until transaction was completed. " +
                               "Starting repeating requests to get this transaction until status changes.");
            transactionResponse = await WaitForPrematurelyReturnedTransactionAsync(transactionResponse.Id);
        }

        _logger.LogInformation("Payment {PaymentId} was processed by acquiring bank. Saving the result.", id);
        var processedEvent = new PaymentProcessedEvent
        {
            PaymentId = id.Value,
            Status = transactionResponse.Status switch
            {
                Status.Completed => PaymentStatus.Completed,
                Status.Rejected => PaymentStatus.Rejected,
                Status.Accepted => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            }
        };
        await _storage.SaveAsync(processedEvent);
        _logger.LogInformation("Payment {PaymentId} result was saved.", id);
        // TODO: Card number is validated at this point, but last 4 digits can be extracted in a more solid manner
        return new PaymentProcessedResponse
        {
            PaymentId = id,
            Status = transactionResponse.Status,
            SenderCardLastFourDigits = request.CardNumber[^4..],
            PaymentReference = id.ToPaymentReference()
        };
    }

    // TODO: better handling of prematurely returned transaction responses
    private async Task<TransactionResponse> WaitForPrematurelyReturnedTransactionAsync(Guid transactionId)
    {
        var transactionResponse = await _client.GetTransactionAsync(transactionId);

        if (transactionResponse is null) throw new TransactionException(
            $"{nameof(WaitForPrematurelyReturnedTransactionAsync)} expected transaction to exist, " +
            "but acquiring bank client returned null");
        
        var counter = 0;
        while (transactionResponse!.Status == Status.Accepted && ++counter < 10)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            transactionResponse = await _client.GetTransactionAsync(transactionResponse.Id);
        }

        return transactionResponse;
    }
}