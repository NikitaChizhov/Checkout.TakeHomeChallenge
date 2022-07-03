using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Controllers.Logic;

/// <summary>
/// This interface allows actual <see cref="PaymentControllerLogic"/> be internal and have internal dependencies.
/// </summary>
public interface IPaymentControllerLogic
{
    Task<Result<PaymentAcceptedResponse>> MakePaymentAsync(PaymentRequest request);

    Task<Result<PaymentProcessedResponse>> GetPaymentAsync(PaymentId paymentId,
        CancellationToken cancellationToken = default);
}