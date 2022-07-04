using System.Collections.Concurrent;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;
using Checkout.TakeHomeChallenge.PaymentGateway.Storage;

namespace Checkout.TakeHomeChallenge.PaymentGateway.IntegrationTests;

internal sealed class TestPaymentStorage 
    : IPaymentInitializedEventStorage, IPaymentProcessedEventStorage
{
    private readonly ConcurrentDictionary<PaymentId, PaymentInitializedEvent> _initialized = new();
    private readonly ConcurrentDictionary<PaymentId, PaymentProcessedEvent> _processed = new();

    public Task<Result> SaveAsync(PaymentInitializedEvent @event)
    {
        var paymentId = new PaymentId(@event.PaymentId);
        var success = _initialized.TryAdd(paymentId, @event);
        return Task.FromResult(success
            ? Result.Successful()
            : Result.Fail("Payment id already exists", FailureCode.PaymentAlreadyStartedBefore));
    }

    public Task SaveAsync(PaymentProcessedEvent @event)
    {
        var paymentId = new PaymentId(@event.PaymentId);
        _processed.TryAdd(paymentId, @event);
        return Task.CompletedTask;
    }

    public Task<PaymentProcessedEvent?> GetAsync(PaymentId paymentId, CancellationToken cancellationToken = default)
    {
        if (!_processed.TryGetValue(paymentId, out var @event)) return Task.FromResult<PaymentProcessedEvent?>(null);

        if (_initialized.TryGetValue(paymentId, out var initialized)) @event.Info = initialized;

        return Task.FromResult(@event)!;
    }
}