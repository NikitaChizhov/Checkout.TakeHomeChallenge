using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Services;

internal interface IPaymentsService
{
    /// <summary>
    /// Starts the payment. Method assumes that all idempotency and concurrency controls have
    /// already been passed and it is never called with the same payment id twice
    /// </summary>
    /// <param name="paymentId"></param>
    /// <param name="request"></param>
    /// <param name="merchant"></param>
    /// <returns></returns>
    void StartPayment(PaymentId paymentId, PaymentRequest request, Merchant merchant);

    /// <summary>
    /// Gets the status of a processed payment.
    /// </summary>
    /// <param name="paymentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PaymentProcessedResponse?> GetPaymentResultAsync(PaymentId paymentId,
        CancellationToken cancellationToken = default);
}