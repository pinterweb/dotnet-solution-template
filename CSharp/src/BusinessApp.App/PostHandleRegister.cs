namespace BusinessApp.App
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Runs all the tasks after handling completes
    /// </summary>
    public class PostHandleRegister : IPostHandleRegister
    {
        private readonly List<Func<Task>> finishHandlers
            = new List<Func<Task>>();

        public ICollection<Func<Task>> FinishHandlers
        {
            get => finishHandlers;
        }

        public async Task OnFinishedAsync()
        {
            var handlersToRun = finishHandlers.ToArray();
            finishHandlers.Clear();

            for (int i = 0; i < handlersToRun.Length; i++)
            {
                await handlersToRun[i]();
            }
        }
    }
}
