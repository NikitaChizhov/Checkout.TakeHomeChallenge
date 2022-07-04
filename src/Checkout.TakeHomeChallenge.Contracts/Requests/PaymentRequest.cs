using System.ComponentModel.DataAnnotations;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;

namespace Checkout.TakeHomeChallenge.Contracts.Requests;

public sealed record PaymentRequest
{
    /// <summary>
    /// Unique id of the payment, prevents payment to be done multiple times if request has to be retried or in case
    /// of request duplication due to network failure.
    /// </summary>
    [Required]
    public IdempotencyId IdempotencyId { get; init; }
    
    /// <summary>
    /// Id of the merchant.
    /// </summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    [Required]
    public MerchantId MerchantId { get; init; }
    
    /// <summary>
    /// Credit card number.
    /// </summary>
    /// <example>4974 5980 7129 2095</example>
    [Required]
    [CreditCard]
    public string CardNumber { get; init; } = "";

    /// <summary>
    /// Name written on the card.
    /// </summary>
    /// <example>Marie Curie</example>
    [Required]
    public string Name { get; init; } = "";

    /// <summary>
    /// Cards expiry date in the mm/yyyy pattern.
    /// </summary>
    /// <example>04/2028</example>
    [Required]
    [RegularExpression(Constants.ExpiryDatePattern)]
    public string CardExpiryDate { get; init; } = "";

    /// <summary>
    /// Cards CVV. 
    /// </summary>
    /// <example>123</example>
    [Required]
    [RegularExpression(Constants.CardVerificationNumberPattern)]
    public string CardVerificationValue { get; init; } = "";

    /// <summary>
    /// Amount of money to be transferred
    /// </summary>
    [Required]
    public CurrencyAmount Value { get; init; }
}