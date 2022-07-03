using Microsoft.Extensions.Options;

namespace Checkout.TakeHomeChallenge.BankSimulator.Services;

internal sealed class DummyApiKeyService : IApiKeyService
{
    private readonly string _authorizedKey;

    public DummyApiKeyService(IOptions<DummyApiKeyServiceConfig> config)
    {
        _authorizedKey = config.Value.ApiKey;
    }
    
    public Task<bool> IsKeyAuthorizedAsync(string key) =>
        Task.FromResult(string.Equals(key, _authorizedKey, StringComparison.OrdinalIgnoreCase));
}