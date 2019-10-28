using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using DataAnalyzerServices.Interfaces;
using Microsoft.Extensions.Logging;

namespace DataAnalyzerServices
{
    public class JobQueue : IJobQueue
    {
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<Func<CancellationToken, Task>> _jobs;
        private SemaphoreSlim _signal;

        public JobQueue(ILogger<IJobQueue> logger)
        {
            _logger = logger;

            _jobs = new ConcurrentQueue<Func<CancellationToken, Task>>();
            _signal = new SemaphoreSlim(0);
        }

        public void QueueJob(Func<CancellationToken, Task> job)
        {
            if (_jobs == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            _jobs.Enqueue(job);
            _signal.Release();

            _logger.LogDebug("Job Enqueued");
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            if (!_jobs.TryDequeue(out var workItem))
                _logger.LogError("Error dequeueing Job");

            _logger.LogDebug("Job Dequeued");

            return workItem;
        }
    }
}
