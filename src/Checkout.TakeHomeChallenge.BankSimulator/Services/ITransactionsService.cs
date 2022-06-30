using System.Diagnostics.CodeAnalysis;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Responses;

namespace Checkout.TakeHomeChallenge.BankSimulator.Services;

public interface ITransactionsService
{
    Task<TransactionResponse> MaybeMakeTransactionAsync(TransactionRequest request, string? idempotencyKey);

    bool TryGetTransactionStatus(Guid transactionId, 
        [NotNullWhen(true)] out TransactionResponse? transaction);
}