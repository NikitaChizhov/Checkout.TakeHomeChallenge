using Checkout.TakeHomeChallenge.BankSimulator.Middlewares;
using Checkout.TakeHomeChallenge.BankSimulator.Services;
using Hellang.Middleware.ProblemDetails;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IApiKeyService, DummyApiKeyService>();
builder.Services.AddSingleton<ITransactionsService, TransactionsService>();

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseProblemDetails();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<AuthMiddleware>();
app.MapControllers();
app.Run();