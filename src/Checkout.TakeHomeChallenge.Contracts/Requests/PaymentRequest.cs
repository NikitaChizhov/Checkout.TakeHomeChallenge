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
    [Required]
    public MerchantId MerchantId { get; init; }
    
    /// <summary>
    /// Credit card number.
    /// </summary>
    [Required]
    [CreditCard]
    public string CardNumber { get; init; } = "";

    /// <summary>
    /// Name written on the card.
    /// </summary>
    [Required]
    public string Name { get; init; } = "";

    /// <summary>
    /// Cards expiry date in the mm/yyyy pattern.
    /// </summary>
    [Required]
    [RegularExpression(Constants.ExpiryDatePattern)]
    public string CardExpiryDate { get; init; } = "";

    /// <summary>
    /// Cards CVV. 
    /// </summary>
    [Required]
    [RegularExpression(Constants.CardVerificationNumberPattern)]
    public string CardVerificationValue { get; init; } = "";

    /// <summary>
    /// Amount of money to be transferred
    /// </summary>
    [Required]
    public CurrencyAmount Value { get; init; }
}