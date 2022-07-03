using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;
using Checkout.TakeHomeChallenge.PaymentGateway.Storage;
using Microsoft.EntityFrameworkCore;

namespace Checkout.TakeHomeChallenge.PaymentGateway;

/// <summary>
/// This service hard-codes a single merchant into the database on startup.
/// </summary>
internal sealed class DatabaseInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseInitializer(IServiceScopeFactory factory)
    {
        _scopeFactory = factory;
    }
    
    // Normally it is not a good idea to run a potentially long running code in StartAsync
    // and usage of BackgroundService is recommended. But here we explicitly want the application
    // to be ready only after db is configured
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetService<DatabaseContext>()!;

        await context.Database.EnsureCreatedAsync(cancellationToken);
        
        var allMerchants = await context.Merchants.ToListAsync(cancellationToken);
        context.Merchants.RemoveRange(allMerchants);

        context.Merchants.Add(new Merchant
        {
            Id = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            BankAccountNumber = "DE75512108001245126199",
            BankIdentifierCode = "AARBDE5W100"
        });

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}