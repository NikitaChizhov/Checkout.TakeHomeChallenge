using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;

internal sealed class PaymentInitializedEvent
{
    // TODO: database layer can also utilize strongly typed ids, it just needs a couple of converters
    // They could be defined by a single additional flag in Contracts, but it would require 
    // Contracts to reference EntityFrameworkCore, which is not perfect.  

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid PaymentId { get; set; }
    
    public Guid MerchantId { get; set; }

    public string PaymentReference { get; set; } = "";
    
    public string SenderCardLastFourDigits { get; set; } = "";
    
    public ulong Amount { get; set; }
    public Currency Currency { get; set; }
    
    [ForeignKey(nameof(MerchantId))]
    public Merchant? Merchant { get; set; }
}