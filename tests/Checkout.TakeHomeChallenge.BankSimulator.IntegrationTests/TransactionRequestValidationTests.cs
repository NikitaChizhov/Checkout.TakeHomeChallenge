using System.Net;
using System.Text;
using Checkout.TakeHomeChallenge.BankSimulator.Services;
using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Checkout.TakeHomeChallenge.BankSimulator.IntegrationTests;

public class TransactionRequestValidationTests : IAsyncLifetime
{
    private const string TestApiKey = "123";
    private readonly WebApplicationFactory<Program> _applicationFactory;
    private readonly HttpClient _client;

    public TransactionRequestValidationTests()
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
    
    [Fact]
    public async Task NoApiKeyReturns401()
    {
        var response = await _client.GetAsync("/");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task WrongApiKeyReturns401()
    {
        var message = new HttpRequestMessage(HttpMethod.Get, "/");
        message.Headers.Add(Constants.ApiKeyHeader, "wrong");
        
        var response = await _client.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
     
    
    [Fact]
    public async Task MakeTransaction_EmptyBodyReturns415()
    {
        var message = new HttpRequestMessage(HttpMethod.Post, Routes.TransactionsResource.Base);
        message.Headers.Add(Constants.ApiKeyHeader, TestApiKey);
        
        var response = await _client.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }
    
    [Fact]
    public async Task MakeTransaction_EmptyJsonReturns400()
    {
        var message = new HttpRequestMessage(HttpMethod.Post, Routes.TransactionsResource.Base)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        message.Headers.Add(Constants.ApiKeyHeader, TestApiKey);
        
        var response = await _client.SendAsync(message);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_EmptyRecipientBankAccountNumberReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { RecipientBankAccountNumber = "" };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_WrongRecipientBankAccountNumberReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with
        {
            RecipientBankAccountNumber = wellFormattedRequest.RecipientBankAccountNumber[1..^1]
        };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_EmptyRecipientBankIdentifierCodeReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { RecipientBankIdentifierCode = "" };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_EmptySenderCardNumberReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { SenderCardNumber = "" };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_WrongSenderCardNumberReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with
        {
            SenderCardNumber = wellFormattedRequest.SenderCardNumber.Replace('1', '2')
        };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_EmptySenderCardNameReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { SenderName = "" };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_EmptySenderCardExpiryDateReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { SenderCardExpiryDate = "" };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_EmptyCurrencyCodeReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { CurrencyCode = "" };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_TooLongCurrencyCodeReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { CurrencyCode = "Dollars" };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_TooShortCurrencyCodeReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { CurrencyCode = "US" };
        return AssertBadRequestAsync(request);
    }

    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public Task MakeTransaction_ZeroAmountReturns400(TransactionRequest wellFormattedRequest)
    {
        var request = wellFormattedRequest with { Amount = 0 };
        return AssertBadRequestAsync(request);
    }
    
    [Theory]
    [MemberData(nameof(TestData.WellFormattedRequests), MemberType = typeof(TestData))]
    public async Task MakeTransaction_WellFormattedRequestReturns200(TransactionRequest wellFormattedRequest)
    {
        var response = await SendTransactionRequestAsync(wellFormattedRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var transactionResponse = await response.Content.ReadFromJsonAsync<TransactionResponse>();
        transactionResponse.Should().NotBeNull();
        transactionResponse!.Id.Should().NotBeEmpty();
        transactionResponse.Started.Should()
            .BeWithin(TimeSpan.FromSeconds(5)).Before(DateTime.UtcNow, 
                "expected a response within 4 seconds");
        transactionResponse.Updated.Should()
            .BeWithin(TimeSpan.FromMilliseconds(50)).Before(DateTime.UtcNow, 
                "expected a transaction status to be updated right before returning");
        transactionResponse.StatusCode.Should().NotBe(Status.Accepted);
    }
    
    private async Task AssertBadRequestAsync(TransactionRequest request)
    {
        var response = await SendTransactionRequestAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<HttpResponseMessage> SendTransactionRequestAsync(TransactionRequest request,
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
        return await _client.SendAsync(message);
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _applicationFactory.DisposeAsync();
}