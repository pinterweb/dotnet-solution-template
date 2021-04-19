using System;
using BusinessApp.Domain;

namespace BusinessApp.App
{
    public interface ITransactionFactory
    {
        Result<IUnitOfWork, Exception> Begin();
    }
}
