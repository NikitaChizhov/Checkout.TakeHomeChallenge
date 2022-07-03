using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;

internal sealed class Merchant
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public string BankAccountNumber { get; set; } = "";

    public string BankIdentifierCode { get; set; } = "";
}