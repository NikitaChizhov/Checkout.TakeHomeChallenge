using System.Runtime.Serialization;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Exceptions;

internal class PaymentGatewayException : Exception
{
    public PaymentGatewayException()
    {
    }

    public PaymentGatewayException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public PaymentGatewayException(string? message) : base(message)
    {
    }

    protected PaymentGatewayException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}