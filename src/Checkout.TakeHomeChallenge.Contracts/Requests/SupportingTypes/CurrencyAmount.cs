using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;

[DebuggerDisplay("{Currency} {Amount}")]
public readonly struct CurrencyAmount
{
    /// <summary>
    /// Amount in minor units of the currency 
    /// </summary>
    [Range(1, ulong.MaxValue)]
    [JsonInclude]
    public readonly ulong Amount;

    /// <summary>
    /// Currency code.
    /// </summary>
    [JsonInclude]
    public readonly Currency Currency;

    [JsonConstructor]
    public CurrencyAmount(ulong amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }
}