namespace BusinessApp.App
{
    using System;
    using BusinessApp.Domain;

    public interface ITransactionFactory
    {
        Result<IUnitOfWork, Exception> Begin();
    }
}
