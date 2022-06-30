using Checkout.TakeHomeChallenge.BankSimulator.Services;
using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Checkout.TakeHomeChallenge.BankSimulator.Controllers;

[ApiController]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionsService _transactionsService;

    public TransactionsController(ITransactionsService transactionsService)
    {
        _transactionsService = transactionsService;
    }

    [HttpPost(Routes.TransactionsResource.Base)]
    public async Task<IActionResult> MakeTransaction([FromBody] TransactionRequest request, 
        [FromHeader(Name = Constants.IdempotencyKeyHeader)] string? idempotencyKey) 
        => Ok(await _transactionsService.MaybeMakeTransactionAsync(request, idempotencyKey));

    [HttpGet(Routes.TransactionsResource.WithId)]
    public IActionResult GetTransaction(
        [FromRoute(Name = Routes.TransactionsResource.WithIdParamName)] Guid id) 
        => _transactionsService.TryGetTransactionStatus(id, out var transaction) 
            ? Ok(transaction) 
            : NotFound();
}