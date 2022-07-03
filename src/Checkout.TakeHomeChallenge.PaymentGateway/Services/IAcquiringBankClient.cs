using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Services;

internal interface IAcquiringBankClient
{
    Task<Result<TransactionResponse>> MakeTransactionRequestAsync(TransactionRequest request, string idempotencyKey);
    
    Task<TransactionResponse?> GetTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);
}