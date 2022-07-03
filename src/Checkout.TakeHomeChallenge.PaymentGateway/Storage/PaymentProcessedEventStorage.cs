using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Storage;

/// <summary>
/// This class is needed mostly to bridge EF's DbContext inability to work with singletons
/// </summary>
internal sealed class PaymentProcessedEventStorage : IPaymentProcessedEventStorage
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PaymentProcessedEventStorage(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task SaveAsync(PaymentProcessedEvent @event)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetService<DatabaseContext>()!;
        context.PaymentProcessedEvents.Add(@event);
        await context.SaveChangesAsync();
    }

    public async Task<PaymentProcessedEvent?> GetAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetService<DatabaseContext>()!;
        return await context.PaymentProcessedEvents
            .AsQueryable()
            .Where(e => e.PaymentId == paymentId.Value)
            .Include(e => e.Info)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}