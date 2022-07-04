using System.Net;
using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Services;

internal sealed class SimulatorBankClient : IAcquiringBankClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SimulatorBankClient> _logger;

    public SimulatorBankClient(HttpClient httpClient, ILogger<SimulatorBankClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<TransactionResponse>> MakeTransactionRequestAsync(TransactionRequest request, string idempotencyKey)
    {
        _logger.LogInformation("Sending the transaction request to simulator bank api.");
        var message = new HttpRequestMessage(HttpMethod.Post, Routes.TransactionsResource.Base)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add(Constants.IdempotencyKeyHeader, idempotencyKey);
        var response = await _httpClient.SendAsync(message);

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                var r = await response.Content.ReadFromJsonAsync<TransactionResponse>();
                _logger.LogInformation("Simulator bank responded to a transaction " +
                                       "with id {TransactionId} with status {Status}",
                    r!.Id, r.Status);
                return r;
            default:
                var responseString = await response.Content.ReadAsStringAsync();
                _logger.LogError("Simulator bank responded with Http Status Code {HttpStatusCode}: {Response}", 
                    response.StatusCode, responseString);
                return Result<TransactionResponse>.Fail(
                    $"Bank API returned {response.StatusCode}: {responseString}");
        }
    }

    public async Task<TransactionResponse?> GetTransactionAsync(Guid transactionId, 
        CancellationToken cancellationToken = default)
    {
        var route = $"{Routes.TransactionsResource.Base}/{transactionId}";
        var response = await _httpClient.GetAsync(route, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        return await response.Content.ReadFromJsonAsync<TransactionResponse>(cancellationToken: cancellationToken);
    }
}