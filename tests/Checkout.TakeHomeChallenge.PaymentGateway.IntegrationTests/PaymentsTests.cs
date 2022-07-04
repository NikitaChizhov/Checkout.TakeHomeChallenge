using System.Net;
using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;
using Checkout.TakeHomeChallenge.PaymentGateway.Services;
using Checkout.TakeHomeChallenge.PaymentGateway.Storage;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Checkout.TakeHomeChallenge.PaymentGateway.IntegrationTests;

public class PaymentsTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _applicationFactory;
    private readonly HttpClient _client;
    private readonly Mock<IAcquiringBankClient> _acquiringBankMock;

    // Save idempotencyId so that it is the same within one test, but not make it static so that it is different 
    // between different tests
    private readonly Guid _idempotencyId = Guid.NewGuid();
    private readonly Guid _merchantId = Guid.NewGuid();
    private JsonContent ValidPaymentRequest => JsonContent.Create(new
    {
        IdempotencyId = _idempotencyId,
        MerchantId = _merchantId,
        CardNumber = "4593 4460 1631 8149",
        Name = "Nikita Chizhov",
        CardExpiryDate = "04/2024",
        CardVerificationValue = "123",
        Value = new
        {
            Amount = 100,
            Currency = "EUR"
        }
    });
    
    public PaymentsTests()
    {
        _acquiringBankMock = new Mock<IAcquiringBankClient>();
        _acquiringBankMock
            .Setup(client => client.MakeTransactionRequestAsync(
                It.IsAny<TransactionRequest>(),
                It.IsAny<string>()))
            .ReturnsAsync(new TransactionResponse { Id = Guid.NewGuid(), Status = Status.Completed });

        var merchantsStorageMock = new Mock<IMerchantStorage>();
        merchantsStorageMock
            .Setup(storage => storage.GetAsync(
                It.Is<MerchantId>(id => id.Value == _merchantId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Merchant
            {
                Id = _merchantId,
                BankAccountNumber = "DE75512108001245126199",
                BankIdentifierCode = "AARBDE5W100"
            });

        _applicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(sc =>
                {
                    sc.RemoveAll(typeof(IHostedService));
                    sc.AddSingleton(_acquiringBankMock.Object);
                    sc.AddSingleton<TestPaymentStorage>();

                    sc.AddScoped(_ => merchantsStorageMock.Object)
                        .AddSingleton<IPaymentProcessedEventStorage>(
                            sp => sp.GetRequiredService<TestPaymentStorage>())
                        .AddScoped<IPaymentInitializedEventStorage>(
                            sp => sp.GetRequiredService<TestPaymentStorage>());
                });
            });
        _client = _applicationFactory.CreateClient();
    }
    
    [Fact]
    public async Task MakingPaymentWithValidFormatWorks()
    {
        var response = await MakePaymentAsync(ValidPaymentRequest);
        response.Should().NotBeNull();
        response!.PaymentId.Value.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task GettingPaymentWorks()
    {
        // prepare
        var response = await MakePaymentAsync(ValidPaymentRequest);
        response.Should().NotBeNull();
        response!.PaymentId.Value.Should().NotBeEmpty();
        
        // act
        var paymentResponse = await GetPaymentAsync(response.PaymentId);
        paymentResponse.Should().NotBeNull();

        paymentResponse!.PaymentId.Should().Be(response.PaymentId);
        paymentResponse.SenderCardLastFourDigits.Should().Be("8149");
        paymentResponse.Status.Should().BeOneOf(Status.Completed, Status.Rejected);
    }

    [Fact]
    public async Task MakingManyPaymentsWithSameIdempotencyIdCallsAcquiringOnce()
    {
        var paymentTasks = new Task<PaymentAcceptedResponse?>[10];
        Parallel.For(0, 10, i =>
        {
            paymentTasks[i] = MakePaymentAsync(ValidPaymentRequest);
        });

        await Task.WhenAll(paymentTasks);

        var paymentIds = paymentTasks
            .Select(t => t.Result!.PaymentId)
            .ToList();

        paymentIds.Should().AllBeEquivalentTo(paymentIds[0]);

        await GetPaymentAsync(paymentIds[0]);
        
        _acquiringBankMock.Verify(client => client.MakeTransactionRequestAsync(
            It.IsAny<TransactionRequest>(),
            It.IsAny<string>()), Times.Once);
    }

    private async Task<PaymentAcceptedResponse?> MakePaymentAsync(JsonContent content)
    {
        var responseMessage = await _client.PostAsync(Routes.PaymentsResource.Base, content);
        responseMessage.StatusCode.Should().Be(HttpStatusCode.Accepted);
        return await responseMessage.Content.ReadFromJsonAsync<PaymentAcceptedResponse>();
    }

    private async Task<PaymentProcessedResponse?> GetPaymentAsync(PaymentId paymentId)
    {
        var payment = await _client.GetAsync($"{Routes.PaymentsResource.Base}/{paymentId}");
        payment.StatusCode.Should().Be(HttpStatusCode.OK);
        return await payment.Content.ReadFromJsonAsync<PaymentProcessedResponse>();
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _applicationFactory.DisposeAsync();
}