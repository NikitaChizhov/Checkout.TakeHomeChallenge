using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Storage;

internal interface IPaymentProcessedEventStorage
{
    Task SaveAsync(PaymentProcessedEvent @event);

    Task<PaymentProcessedEvent?> GetAsync(PaymentId paymentId, CancellationToken cancellationToken = default);
}