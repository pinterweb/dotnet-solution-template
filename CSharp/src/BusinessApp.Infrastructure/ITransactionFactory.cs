using System;
using BusinessApp.Domain;

namespace BusinessApp.Infrastructure
{
    public interface ITransactionFactory
    {
        Result<IUnitOfWork, Exception> Begin();
    }
}
