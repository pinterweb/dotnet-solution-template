using System;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    public interface ITransactionFactory
    {
        Result<IUnitOfWork, Exception> Begin();
    }
}
