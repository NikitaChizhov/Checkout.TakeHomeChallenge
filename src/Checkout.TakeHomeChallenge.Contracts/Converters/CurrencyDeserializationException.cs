using System.Runtime.Serialization;
using System.Text.Json;

namespace Checkout.TakeHomeChallenge.Contracts.Converters;

public class CurrencyDeserializationException : JsonException
{
    public CurrencyDeserializationException()
    {
    }

    protected CurrencyDeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public CurrencyDeserializationException(string? message) : base(message)
    {
    }

    public CurrencyDeserializationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}