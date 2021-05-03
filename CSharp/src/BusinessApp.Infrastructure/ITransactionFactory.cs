using System;
using BusinessApp.Kernel;

namespace BusinessApp.Infrastructure
{
    /// <summary>
    /// Service to being a business transaction
    /// </summary>
    public interface ITransactionFactory
    {
        Result<IUnitOfWork, Exception> Begin();
    }
}
