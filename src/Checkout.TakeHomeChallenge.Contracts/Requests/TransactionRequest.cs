using System.ComponentModel.DataAnnotations;

namespace Checkout.TakeHomeChallenge.Contracts.Requests;

public sealed record TransactionRequest
{
    /// <summary>
    /// IBAN of the recipient.
    /// </summary>
    [Required]
    [RegularExpression(Constants.AccountNumberPattern)]
    public string RecipientBankAccountNumber { get; init; } = "";

    /// <summary>
    /// BIC of the recipient.
    /// </summary>
    [Required]
    [RegularExpression(Constants.BankIdentifierCodePattern)]
    public string RecipientBankIdentifierCode { get; init; } = "";

    /// <summary>
    /// Payment reference (maximum 18 characters). Optional.
    /// </summary>
    [StringLength(18)]
    public string? PaymentReference { get; init; }

    /// <summary>
    /// Credit card of the sender.
    /// </summary>
    [Required]
    [CreditCard]
    public string SenderCardNumber { get; init; } = "";

    /// <summary>
    /// Sender name written on the card.
    /// </summary>
    [Required]
    public string SenderName { get; init; } = "";

    /// <summary>
    /// Cards expiry date in the mm/yyyy pattern.
    /// </summary>
    [Required]
    [RegularExpression(Constants.ExpiryDatePattern)]
    public string SenderCardExpiryDate { get; init; } = "";

    /// <summary>
    /// Cards CVV. 
    /// </summary>
    [Required]
    [RegularExpression(Constants.CardVerificationNumberPattern)]
    public string SenderCardVerificationValue { get; init; } = "";

    /// <summary>
    /// Currency ISO 4217 letter code. 
    /// </summary>
    [Required]
    [RegularExpression(Constants.CurrencyCodePattern)]
    public string CurrencyCode { get; init; } = "";
    
    /// <summary>
    /// Amount of currency to transfer, in minor units.
    /// For example, 1 EUR will be 100, because EUR minor units are 1/100 of major unit.
    /// </summary>
    [Required]
    [Range(1, ulong.MaxValue)]
    public ulong Amount { get; set; }
}