namespace Checkout.TakeHomeChallenge.BankSimulator.Services;

internal sealed class DummyApiKeyService : IApiKeyService
{
    private readonly string _authorizedKey;

    public DummyApiKeyService(string authorizedKey = "zbky501yeyo2ezcaueaufiomnux4rqjy")
    {
        _authorizedKey = authorizedKey;
    }
    
    public Task<bool> IsKeyAuthorizedAsync(string key) =>
        Task.FromResult(string.Equals(key, _authorizedKey, StringComparison.OrdinalIgnoreCase));
}