using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.PaymentGateway;
using Checkout.TakeHomeChallenge.PaymentGateway.Controllers.Logic;
using Checkout.TakeHomeChallenge.PaymentGateway.Exceptions;
using Checkout.TakeHomeChallenge.PaymentGateway.Services;
using Checkout.TakeHomeChallenge.PaymentGateway.Storage;
using Hellang.Middleware.ProblemDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddProblemDetails(options =>
{
    options.MapToStatusCode<AcquiringBankException>(StatusCodes.Status502BadGateway);
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // has to manually add mappings so that generated OpenAPI schema is correct.
    // Keep a look on https://github.com/andrewlock/StronglyTypedId/issues/58
    options.MapType<IdempotencyId>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
    options.MapType<MerchantId>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
    options.MapType<PaymentId>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
});


builder.Services
    .AddHttpClient<IAcquiringBankClient, SimulatorBankClient>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
        client.BaseAddress = builder.Configuration.GetValue<Uri>("Simulator:Url");
        
        var apiKey = builder.Configuration.GetValue<string>("Simulator:ApiKey");
        client.DefaultRequestHeaders.Add(Constants.ApiKeyHeader, apiKey);
    })
    .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
    {
        // this forces HttpClient to trust dev ssl certificates
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
    
builder.Services
    .AddHostedService<DatabaseInitializer>()
    .AddSingleton<IPaymentsService, PaymentsService>()
    .AddSingleton<IPaymentProcessedEventStorage, PaymentProcessedEventStorage>()
    .AddScoped<IPaymentControllerLogic, PaymentControllerLogic>()
    .AddScoped<IMerchantStorage, MerchantStorage>()
    .AddScoped<IPaymentInitializedEventStorage, PaymentInitializedEventStorage>();

builder.Services
    .AddDbContext<DatabaseContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetValue<string>("Postgres"),
                optionsBuilder =>
                {
                    optionsBuilder.EnableRetryOnFailure(3);
                });
        },
        ServiceLifetime.Scoped, ServiceLifetime.Singleton);

var app = builder.Build();

app.UseProblemDetails();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();