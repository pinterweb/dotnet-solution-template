namespace BusinessApp.App
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements the Producer/Consume queue, using a background thread to process
    /// work
    /// </summary>
    public abstract class BackgroundWorker<T> : IDisposable
    {
        // it uses ConcurrentQueue<T> underneath
        private readonly BlockingCollection<T> queue = new BlockingCollection<T>();
        private readonly Task worker;
        private bool disposed;

        public BackgroundWorker()
        {
            // use over Task.Run so we can specify
            // LongRunning
            worker = Task.Factory.StartNew(
                Dequeue,
                CancellationToken.None,
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default
            );
        }

        public void Dispose()
        {
            if (!disposed)
            {
                queue.CompleteAdding();
                worker.Wait();
                queue.Dispose();
                disposed = true;
            }
        }

        protected void EnQueue(T t)
        {
            if (disposed || queue.IsAddingCompleted || !queue.TryAdd(t)) OnAddFailed(t);
        }

        protected abstract void OnAddFailed(T t);

        protected abstract void Consume(T t);

        protected abstract void EnqueueFailed(Exception ex);

        private void Dequeue()
        {
            try
            {
                foreach (var entry in queue.GetConsumingEnumerable())
                {
                    Consume(entry);
                }
            }
            catch (Exception ex)
            {
                EnqueueFailed(ex);
            }
        }
    }
}
