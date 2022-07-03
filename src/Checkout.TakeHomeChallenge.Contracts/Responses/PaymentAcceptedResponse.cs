using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;

namespace Checkout.TakeHomeChallenge.Contracts.Responses;

public sealed record PaymentAcceptedResponse
{
    public PaymentId PaymentId { get; init; }

    public Uri Location { get; init; } = new("about:blank");
}