using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;

namespace Checkout.TakeHomeChallenge.Contracts.Responses;

public sealed record PaymentProcessedResponse
{
    public PaymentId PaymentId { get; init; }
    
    public Status Status { get; init; }

    public string SenderCardLastFourDigits { get; init; } = "";

    public string PaymentReference { get; set; } = "";
}