using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Storage;

internal interface IMerchantStorage
{
    Task<Result<Merchant>> GetAsync(MerchantId id, CancellationToken cancellationToken = default);
}