namespace Checkout.TakeHomeChallenge.BankSimulator.Services;

public interface IApiKeyService
{
    public Task<bool> IsKeyAuthorizedAsync(string key);
}