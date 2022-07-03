using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Responses;

namespace Checkout.TakeHomeChallenge.BankSimulator.Services;

internal sealed class TransactionsService : ITransactionsService
{
    private readonly SemaphoreSlim _semaphore = new(1);
    private readonly ConcurrentDictionary<Guid, TransactionResponse> _transactions = new();
    private readonly ConcurrentDictionary<string, Guid> _ids = new();

    public async Task<TransactionResponse> MaybeMakeTransactionAsync(TransactionRequest request, string? idempotencyKey)
    {
        // Given that this is a simulator, it does not really matter what happens here, we just want to return
        // some "result" of a "transaction". But let's place relatively realistic limitations anyway:
        // 1. Within lifetime of the service, TransactionResponse object created exactly twice per idempotencyKey:
        //      - first time with status Accepted
        //      - second time with randomly selected Status (it's created and not updated
        //        only due to TransactionResponse being immutable)
        //    This invariant should hold regardless of number of concurrent calls to this method 

        var id = GetTransactionId(idempotencyKey);
        
        // Happy non-locking synchronous path.
        if (_transactions.TryGetValue(id, out var transaction)) return transaction;

        (transaction, var created) = await TryMakeOrGetTransactionAsync(id);

        // If transaction was created in another call, then return - only the one who created it should update it
        if (!created) return transaction;

        await Task.Delay(TimeSpan.FromSeconds(1 + Random.Shared.NextDouble() * 3));

        return UpdateTransaction(id, transaction);
    }

    public bool TryGetTransactionStatus(Guid transactionId,
        [NotNullWhen(true)] out TransactionResponse? transaction) 
        => _transactions.TryGetValue(transactionId, out transaction);

    private TransactionResponse UpdateTransaction(Guid id, TransactionResponse currentValue)
    {
        // Exceptions here are for "future-proofing". They will help to realize if assumptions that are true at the
        // moment will at some point become false
        return _transactions.AddOrUpdate(id,
            _ => throw new ArgumentException("Transactions dictionary was deleted from"),
            (_, transaction) =>
            {
                if (!ReferenceEquals(currentValue, transaction))
                {
                    throw new ArgumentException("Transactions dictionary was updated in an unexpected way");
                }

                return transaction with
                {
                    Updated = DateTime.UtcNow,
                    Status = Random.Shared.NextSingle() switch
                    {
                        <= 0.2f => Status.Rejected,
                        _ => Status.Completed
                    }
                };
            });
    }

    /// <summary>
    /// Method will atomically try to retrieve or create a transaction response and return it.
    /// It additionally returns true if transaction was created and false if it was only retrieved
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<(TransactionResponse response, bool created)> TryMakeOrGetTransactionAsync(Guid id)
    {
        // This is a much stronger condition then necessary. We could have locked on specific id instead
        // of service wide lock. However, using available libraries it's not trivial to do. It's possible
        // that named Semaphore could work instead of SemaphoreSlim, but I do not know full implications of
        // using those and would rather not go down that research rabbit hole for a simulator
        await _semaphore.WaitAsync();
        try
        {
            if (_transactions.TryGetValue(id, out var transaction)) return (transaction, false);

            var datetime = DateTime.UtcNow;
            transaction = new TransactionResponse
            {
                Id = id,
                Status = Status.Accepted,
                Started = datetime,
                Updated = datetime
            };

            _transactions.AddOrUpdate(id, transaction, 
                (_, __) => throw new ArgumentException(
                    "Transactions dictionary was inserted into outside the locked context"));
            return (transaction, true);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private Guid GetTransactionId(string? idempotencyKey) 
        => string.IsNullOrWhiteSpace(idempotencyKey) 
            ? Guid.NewGuid()
            : _ids.GetOrAdd(idempotencyKey, _ => Guid.NewGuid());
}