using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Storage;

internal sealed class MerchantStorage : IMerchantStorage
{
    private readonly DatabaseContext _context;

    public MerchantStorage(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Result<Merchant>> GetAsync(MerchantId id, CancellationToken cancellationToken = default)
    {
        var merchant = await _context.Merchants.FindAsync(id.Value);
        if (merchant is null)
        {
            return Result<Merchant>.Fail("Merchant not found", FailureCode.NotFound);
        }

        return merchant;
    }
}