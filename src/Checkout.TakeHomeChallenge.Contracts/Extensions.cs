using Be.Vlaanderen.Basisregisters.Generators.Guid;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;

namespace Checkout.TakeHomeChallenge.Contracts;

public static class Extensions
{
    /// <summary>
    /// Converts currency to an upper case string code according to ISO 4217
    /// </summary>
    /// <param name="currency"></param>
    /// <returns></returns>
    // ReSharper disable once InconsistentNaming
    public static string ToISOString(this Currency currency) => Enum.GetName(currency)!;

    /// <summary>
    /// Deterministically generates payment reference based on id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static string ToPaymentReference(this PaymentId id)
    {
        // This gives us very low collision chance,
        // and makes it impossible to get paymentId from payment ref.
        var refId = Deterministic.Create(id.Value, "payref");
        var base64Guid = Convert.ToBase64String(refId.ToByteArray());
        return base64Guid[..18];
    }
}