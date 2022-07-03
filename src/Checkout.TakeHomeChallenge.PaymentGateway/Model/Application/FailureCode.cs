namespace Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;

public enum FailureCode
{
    Default = 0,
    NotFound = 1,
    BadGateway = 2,
    PaymentAlreadyStartedBefore = 3
}