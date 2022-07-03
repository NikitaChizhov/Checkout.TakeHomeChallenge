using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Storage;

internal sealed class PaymentInitializedEventStorage : IPaymentInitializedEventStorage
{
    private readonly DatabaseContext _context;

    public PaymentInitializedEventStorage(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Result> SaveAsync(PaymentInitializedEvent @event)
    {
        try
        {
            _context.PaymentInitializedEvents.Add(@event);
            await _context.SaveChangesAsync();
            return Result.Successful();
        }
        catch (DbUpdateException e)
        {
            return Result.Fail(e.Message, FailureCode.PaymentAlreadyStartedBefore);
            // This exception is coming from SaveChangesAsync and is expected if
            // idempotency key is not new (i.e. this is a duplicate request)
            // in this case, we do not need to start the payment, we can simply return the id
        }
    }
}