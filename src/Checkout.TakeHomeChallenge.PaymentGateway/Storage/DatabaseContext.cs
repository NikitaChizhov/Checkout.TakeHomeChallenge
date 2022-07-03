using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;
using Microsoft.EntityFrameworkCore;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Storage;

internal sealed class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Merchant> Merchants => Set<Merchant>();

    public DbSet<PaymentInitializedEvent> PaymentInitializedEvents => Set<PaymentInitializedEvent>();
    
    public DbSet<PaymentProcessedEvent> PaymentProcessedEvents => Set<PaymentProcessedEvent>();
}