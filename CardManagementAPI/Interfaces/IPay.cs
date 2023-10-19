namespace CardManagementAPI.Interfaces
{
    public interface IPayment
    {
        Guid PaymentId { get; }
    }

    public interface IPaymentSingleton : IPayment
    {
    }

    public interface IPaymentSingletonInstance : IPayment
    {
    }
}


