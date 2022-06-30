using Checkout.TakeHomeChallenge.Contracts.Requests;

namespace Checkout.TakeHomeChallenge.BankSimulator.IntegrationTests;

public static class TestData
{
    public static IEnumerable<object[]> WellFormattedRequests => new[]
    {
        new[]
        {
            new TransactionRequest()
            {
                RecipientBankAccountNumber = "DE75512108001245126199",
                RecipientBankIdentifierCode = "AARBDE5W100",
                SenderName = "Nikita Chizhov",
                SenderCardNumber = "4593446016318149",
                SenderCardExpiryDate = "01/2022",
                SenderCardVerificationValue = "123",
                Amount = 13576,
                CurrencyCode = "EUR",
                PaymentReference = "Hello"
            }
        },
        new[]
        {
            new TransactionRequest()
            {
                RecipientBankAccountNumber = "BE71096123456769",
                RecipientBankIdentifierCode = "EBATBEBB",
                SenderName = "John Doe",
                SenderCardNumber = "4974598071292095",
                SenderCardExpiryDate = "06/2024",
                SenderCardVerificationValue = "321",
                Amount = 1,
                CurrencyCode = "GBP",
                PaymentReference = ""
            }
        },
        new[]
        {
            new TransactionRequest()
            {
                RecipientBankAccountNumber = "QA54QNBA000000000000693123456",
                RecipientBankIdentifierCode = "ABQQQAQADNB",
                SenderName = "Marie Curie",
                SenderCardNumber = "5232335224196310",
                SenderCardExpiryDate = "03/2023",
                SenderCardVerificationValue = "0801",
                Amount = 35643,
                CurrencyCode = "AUD",
                PaymentReference = "4239ab40df21"
            }
        }
    };
}