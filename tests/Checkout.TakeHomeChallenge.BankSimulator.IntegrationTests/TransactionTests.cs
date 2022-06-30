using System.Net;
using Checkout.TakeHomeChallenge.BankSimulator.Services;
using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Checkout.TakeHomeChallenge.BankSimulator.IntegrationTests;

public sealed class TransactionTests : IAsyncLifetime
{
    private const string TestApiKey = "123";
    private readonly WebApplicationFactory<Program> _applicationFactory;
    private readonly HttpClient _client;

    public TransactionTests()
    {
        _applicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(sc =>
                {
                    sc.AddSingleton<IApiKeyService>(new DummyApiKeyService(TestApiKey));
                });
            });
        _client = _applicationFactory.CreateClient();
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public async Task MakeTransaction_WellFormattedRequestWithIdempotencyKeyReturnsSame(TransactionRequest wellFormattedRequest)
    {
        const string idempotencyKey = "123";

        var tasks = new[]
        {
            SendTransactionRequestAsync(wellFormattedRequest, idempotencyKey),
            SendTransactionRequestAsync(wellFormattedRequest, idempotencyKey),
            SendTransactionRequestAsync(wellFormattedRequest, idempotencyKey),
            SendTransactionRequestAsync(wellFormattedRequest, idempotencyKey)
        };

        await Task.WhenAll(tasks);

        var ids = tasks
            .Select(task => task.Result.Id)
            .ToList();

        ids.Should().AllBeEquivalentTo(ids[0], "all transaction ids are expected to be the same");
    }

    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public async Task GetTransaction_ReturnsSameResponse(TransactionRequest wellFormattedRequest)
    {
        var response = await SendTransactionRequestAsync(wellFormattedRequest);

        var message = new HttpRequestMessage(HttpMethod.Get,
            Routes.TransactionsResource.WithId.Replace("{id:guid}", response.Id.ToString()));
        message.Headers.Add(Constants.ApiKeyHeader, TestApiKey);
        var responseFromGet = await _client.SendAsync(message);
        responseFromGet.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseFromGetObj = await responseFromGet.Content.ReadFromJsonAsync<TransactionResponse>();
        responseFromGetObj.Should().BeEquivalentTo(response);
    }
    
    private async Task<TransactionResponse> SendTransactionRequestAsync(TransactionRequest request,
        string? idempotencyKey = null)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, Routes.TransactionsResource.Base)
        {
            Content = JsonContent.Create(request)
        };
        message.Headers.Add(Constants.ApiKeyHeader, TestApiKey);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            message.Headers.Add(Constants.IdempotencyKeyHeader, idempotencyKey);
        }
        var response = await _client.SendAsync(message);
        response.EnsureSuccessStatusCode();
        
        return (await response.Content.ReadFromJsonAsync<TransactionResponse>())!;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _applicationFactory.DisposeAsync();
}