using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Slate.Client.UI.Framework
{
    public interface IThreadDispatcher
    {
        void FireOnUIAndForget(Func<Task> task);
        void FireInBackgroundAndForget(Func<Task> task);
    }

    public class Dispatcher : IThreadDispatcher
    {
        private delegate Task PendingTask();

        private readonly ILogger _logger;
        private List<PendingTask> _pendingTasks = new();
        private List<PendingTask> _nextList = new();
        private readonly ThreadLocal<bool> _isRenderingThread = new(() => false);
        public Dispatcher(ILogger logger)
        {
            _logger = logger;
            _isRenderingThread.Value = true;
        }

        public bool IsRenderingThread => _isRenderingThread.Value;

        public async void RunPendingTasks()
        {
            var tasks = _pendingTasks;
            _pendingTasks = _nextList;

            try
            {
                await Task.WhenAll(tasks.Select(t => t.Invoke()));
            }
            catch (AggregateException e)
            {
                foreach (var eInnerException in e.InnerExceptions)
                {
                    _logger.Error(eInnerException, "Error running UI Tasks");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error running UI Tasks");
            }

            tasks.Clear();
            _nextList = tasks;
        }
        
        public void FireOnUIAndForget(Func<Task> task)
        {
            _pendingTasks.Add(async () => await task());
        }

        public async void FireInBackgroundAndForget(Func<Task> task)
        {
            try
            {
                await Task.Run(async () => await task());
            }
            catch (Exception e)
            {
                _logger.Error(e, "Unhandled error in background task");
            }
        }
    }
}
