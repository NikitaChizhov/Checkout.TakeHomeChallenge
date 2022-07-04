using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.PaymentGateway;
using Checkout.TakeHomeChallenge.PaymentGateway.Controllers.Logic;
using Checkout.TakeHomeChallenge.PaymentGateway.Exceptions;
using Checkout.TakeHomeChallenge.PaymentGateway.Services;
using Checkout.TakeHomeChallenge.PaymentGateway.Storage;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddProblemDetails(options =>
{
    options.MapToStatusCode<AcquiringBankException>(StatusCodes.Status502BadGateway);
});
builder.Services.AddApiVersioning(options =>
{
    // https://gingter.org/2018/06/18/asp-net-core-api-versioning/
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
    options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, 
        "Checkout.TakeHomeChallenge.PaymentGateway.xml"));
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        "Checkout.TakeHomeChallenge.Contracts.xml"));
    
    // has to manually add mappings so that generated OpenAPI schema is correct.
    // Keep a look on https://github.com/andrewlock/StronglyTypedId/issues/58
    options.MapType<IdempotencyId>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
    options.MapType<MerchantId>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
    options.MapType<PaymentId>(() => new OpenApiSchema { Type = "string", Format = "uuid" });
    options.MapType<CurrencyAmount>(() => new OpenApiSchema
    {
        Type = "object",
        Required = new HashSet<string>{ "amount", "currency" },
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["amount"] = new() { Type = "integer", Format = "int64", Minimum = 1},
            ["currency"] = new() { Type = "string", Example = new OpenApiString("EUR") }
        }
    });
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
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapControllers();
app.Run();