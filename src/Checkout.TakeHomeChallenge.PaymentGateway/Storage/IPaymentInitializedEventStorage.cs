using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Storage;

internal interface IPaymentInitializedEventStorage
{
    /// <summary>
    /// Saves event in the database.
    /// </summary>
    /// <param name="event"></param>
    /// <returns>Successful result if event was saved.
    /// Failed result with Code <see cref="FailureCode.PaymentAlreadyStartedBefore"/>
    /// if this payment id is already stored.</returns>
    Task<Result> SaveAsync(PaymentInitializedEvent @event);
}