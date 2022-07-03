namespace Checkout.TakeHomeChallenge.Contracts;

public static class Routes
{
    public static class TransactionsResource
    {
        public const string Base = "/transactions";

        public const string WithId = Base + "/{id:guid}";

        public const string WithIdParamName = "id";
    }
    
    public static class PaymentsResource
    {
        public const string Base = "/payments";
        
        public const string WithId = Base + "/{id}";

        public const string WithIdParamName = "id";
    }
}