using System.Text.Json.Serialization;

namespace Checkout.TakeHomeChallenge.Contracts.Responses;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    Accepted = 1,
    Completed = 2,
    Rejected = 3
}