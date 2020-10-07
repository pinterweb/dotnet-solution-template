namespace BusinessApp.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Runs all the tasks after handling completes
    /// </summary>
    public class PostCommitRegister : IPostCommitRegister
    {
        public ICollection<Func<Task>> FinishHandlers { get; } = new List<Func<Task>>();

        public async Task OnFinishedAsync()
        {
            var handlersToRun = FinishHandlers.ToArray();
            FinishHandlers.Clear();

            for (int i = 0; i < handlersToRun.Length; i++)
            {
                await handlersToRun[i]();
            }
        }
    }
}
