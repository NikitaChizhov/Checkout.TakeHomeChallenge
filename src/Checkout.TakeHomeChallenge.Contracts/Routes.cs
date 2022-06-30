namespace Checkout.TakeHomeChallenge.Contracts;

public static class Routes
{
    public static class TransactionsResource
    {
        public const string Base = "/transactions";

        public const string WithId = Base + "/{id:guid}";

        public const string WithIdParamName = "id";
    }
}