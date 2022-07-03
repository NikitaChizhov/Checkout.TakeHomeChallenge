namespace Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;

internal sealed class SimulatorConfig
{
    public Uri BaseUrl { get; set; }
    public string ApiKey { get; init; }
}