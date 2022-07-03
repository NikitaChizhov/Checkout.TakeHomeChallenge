using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Model.Database;

internal sealed class PaymentProcessedEvent
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid PaymentId { get; set; }
    
    public PaymentStatus Status { get; set; }
    
    [ForeignKey(nameof(PaymentId))]
    public PaymentInitializedEvent? Info { get; set; }
}