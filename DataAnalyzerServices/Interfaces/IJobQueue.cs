using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataAnalyzerServices.Interfaces
{
    public interface IJobQueue
    {
        void QueueJob(Func<CancellationToken, Task> job);
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
