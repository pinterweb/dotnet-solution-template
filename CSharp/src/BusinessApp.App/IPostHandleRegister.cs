namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface to hold onto handlers that should be run after the main
    /// handling completes
    /// </summary>
    public interface IPostHandleRegister
    {
        /// <summary>
        /// The tasks to run after the handling has finished
        /// </summary>
        ICollection<Func<Task>> FinishHandlers { get; }
    }
}
