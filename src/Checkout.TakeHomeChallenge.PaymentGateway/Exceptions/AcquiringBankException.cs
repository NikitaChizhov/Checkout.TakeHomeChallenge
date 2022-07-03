using System.Runtime.Serialization;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Exceptions;

internal sealed class AcquiringBankException : PaymentGatewayException
{
    public AcquiringBankException()
    {
    }

    public AcquiringBankException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public AcquiringBankException(string? message) : base(message)
    {
    }

    public AcquiringBankException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}