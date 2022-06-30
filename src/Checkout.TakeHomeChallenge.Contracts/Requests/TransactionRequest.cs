using System.ComponentModel.DataAnnotations;

namespace Checkout.TakeHomeChallenge.Contracts.Requests;

public sealed record TransactionRequest
{
    // Regex validation pattern is taken on faith from https://stackoverflow.com/a/44657292
    // language=regexp
    private const string AccountNumberRegexPattern =
        @"(?i)\A([A-Z]{2}[ \-]?[0-9]{2})(?=(?:[ \-]?[A-Z0-9]){9,30}$)((?:[ \-]?[A-Z0-9]{3,5}){2,7})([ \-]?[A-Z0-9]{1,3})?\z";

    // Regex validation pattern is taken on faith from https://stackoverflow.com/a/15920158
    // language=regexp
    private const string BankIdentifierCodePattern = @"(?i)\A[a-z]{6}[0-9a-z]{2}([0-9a-z]{3})?\z";

    // language=regexp
    private const string ExpiryDatePattern = @"\A(0[1-9]|1[0-2])\/([0-9]{4}|[0-9]{2})\z";

    // language=regexp
    private const string CardVerificationNumberPattern = @"\A[0-9]{3,4}\z";

    // language=regexp
    private const string CurrencyCodePattern = @"(?i)\A[A-Z]{3}\z";

    /// <summary>
    /// IBAN of the recipient.
    /// </summary>
    [Required]
    [RegularExpression(AccountNumberRegexPattern)]
    public string RecipientBankAccountNumber { get; init; } = "";

    /// <summary>
    /// BIC of the recipient.
    /// </summary>
    [Required]
    [RegularExpression(BankIdentifierCodePattern)]
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
    [RegularExpression(ExpiryDatePattern)]
    public string SenderCardExpiryDate { get; init; } = "";

    /// <summary>
    /// Cards CVV. 
    /// </summary>
    [Required]
    [RegularExpression(CardVerificationNumberPattern)]
    public string SenderCardVerificationValue { get; init; } = "";

    /// <summary>
    /// Currency ISO 4217 letter code. 
    /// </summary>
    [Required]
    [RegularExpression(CurrencyCodePattern)]
    public string CurrencyCode { get; init; } = "";
    
    /// <summary>
    /// Amount of currency to transfer, in minor units.
    /// For example, 1 EUR will be 100, because EUR minor units are 1/100 of major unit.
    /// </summary>
    [Required]
    [Range(1, ulong.MaxValue)]
    public ulong Amount { get; set; }
}