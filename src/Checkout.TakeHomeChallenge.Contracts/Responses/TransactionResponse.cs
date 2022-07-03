namespace Checkout.TakeHomeChallenge.Contracts.Responses;

public sealed record TransactionResponse
{
    /// <summary>
    /// Transaction Id.
    /// </summary>
    public Guid Id { get; init; }
    
    /// <summary>
    /// Transaction status.
    /// </summary>
    public Status Status { get; init; }
    
    /// <summary>
    /// Date and time of transaction start.
    /// </summary>
    public DateTime Started { get; init; }
    
    /// <summary>
    /// Date and time of transactions last update.
    /// </summary>
    public DateTime Updated { get; set; }
}