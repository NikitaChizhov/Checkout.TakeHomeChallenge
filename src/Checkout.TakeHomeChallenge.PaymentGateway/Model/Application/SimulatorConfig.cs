namespace Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;

internal sealed class SimulatorConfig
{
    public Uri BaseUrl { get; set; } = new Uri("about:blank");
    public string ApiKey { get; init; } = "";
}