using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace discordbot.BackgroundServices
{
    public class BackgroundTaskQueue<T> : IBackgroundTaskQueue<T>
    {
        private ConcurrentQueue<Func<CancellationToken, Task<T>>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task<T>>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void QueueBackgroundWorkItem(
            Func<CancellationToken, Task<T>> workItem)
        {
            if (workItem == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }

            _workItems.Enqueue(workItem);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task<T>>> DequeueAsync(
            CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out Func<CancellationToken, Task<T>> workItem);

            return workItem;
        }
    }
}
