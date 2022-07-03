namespace Checkout.TakeHomeChallenge.Contracts;

public static class Constants
{
    public const string ApiKeyHeader = "X-Api-Key";

    public const string IdempotencyKeyHeader = "Idempotency-Key";

    // Regex validation pattern is taken on faith from https://stackoverflow.com/a/44657292
    // language=regexp
    public const string AccountNumberPattern =
        @"(?i)\A([A-Z]{2}[ \-]?[0-9]{2})(?=(?:[ \-]?[A-Z0-9]){9,30}$)((?:[ \-]?[A-Z0-9]{3,5}){2,7})([ \-]?[A-Z0-9]{1,3})?\z";

    // Regex validation pattern is taken on faith from https://stackoverflow.com/a/15920158
    // language=regexp
    public const string BankIdentifierCodePattern = @"(?i)\A[a-z]{6}[0-9a-z]{2}([0-9a-z]{3})?\z";
    
    // language=regexp
    public const string ExpiryDatePattern = @"\A(0[1-9]|1[0-2])\/([0-9]{4}|[0-9]{2})\z";
    
    // language=regexp
    public const string CardVerificationNumberPattern = @"\A[0-9]{3,4}\z";
    
    // language=regexp
    public const string CurrencyCodePattern = @"(?i)\A[A-Z]{3}\z";
}