using System.Text.Json;
using System.Text.Json.Serialization;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;

namespace Checkout.TakeHomeChallenge.Contracts.Converters;

public sealed class CurrencyJsonConverter : JsonConverter<Currency>
{
    public override Currency Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                var number = reader.GetInt32();

                if (number is <= 0 or > 999)
                {
                    throw new CurrencyDeserializationException("Currency code is not 1-3 digits");
                }

                // Change if at some point enums will get IsDefined or similar without boxing an int
                if (!Enum.IsDefined(typeof(Currency), number))
                {
                    throw new CurrencyDeserializationException(
                        "Currency code is unrecognized (is it a valid ISO 4217?)");
                }
                return (Currency)number;
            case JsonTokenType.String:
                var stringValue = reader.GetString()!;

                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    throw new CurrencyDeserializationException("Currency field is empty");
                }

                if (!Enum.TryParse(stringValue, true, out Currency currency))
                {
                    throw new CurrencyDeserializationException("Currency string is unknown " +
                                                               "(it should be ISO 4217 compatible)");
                }
                return currency;
            default:
                throw new CurrencyDeserializationException("Unexpected json token type in " 
                                                           + nameof(CurrencyJsonConverter));
        }
    }

    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options) 
        => writer.WriteStringValue(value.ToISOString());
}