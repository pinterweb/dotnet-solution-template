namespace BusinessApp.App
{
    using BusinessApp.Domain;

    public interface ITransactionFactory
    {
        IUnitOfWork Begin();
    }
}
