using Checkout.TakeHomeChallenge.BankSimulator.Services;
using Checkout.TakeHomeChallenge.Contracts;

namespace Checkout.TakeHomeChallenge.BankSimulator.Middlewares;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyService _apiKeyService;

    public AuthMiddleware(RequestDelegate next, IApiKeyService apiKeyService)
    {
        _next = next;
        _apiKeyService = apiKeyService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(Constants.ApiKeyHeader, out var extractedApiKey)
            || !await _apiKeyService.IsKeyAuthorizedAsync(extractedApiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.CompleteAsync();
            return;
        }

        await _next(context);
    }
}