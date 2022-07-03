using System.Runtime.Serialization;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Exceptions;

internal sealed class PaymentServiceConcurrencyException : PaymentGatewayException
{
    public PaymentServiceConcurrencyException()
    {
    }

    public PaymentServiceConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public PaymentServiceConcurrencyException(string? message) : base(message)
    {
    }

    public PaymentServiceConcurrencyException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}