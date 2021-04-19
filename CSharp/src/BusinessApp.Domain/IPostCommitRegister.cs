using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessApp.Domain
{
    /// <summary>
    /// Interface to register handlers that should be run after <see cref="IUnitOfWork.CommitAsync"/>
    /// </summary>
    public interface IPostCommitRegister
    {
        /// <summary>
        /// The tasks to run
        /// </summary>
        ICollection<Func<Task>> FinishHandlers { get; }
    }
}
