using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slate.Client.UI.Framework
{
    public interface IThreadDispatcher
    {
        void FireOnUIAndForget(Task task);
    }

    public class Dispatcher : IThreadDispatcher
    {
        private readonly ThreadLocal<bool> _isRenderingThread = new(() => false);
        public Dispatcher()
        {
            _isRenderingThread.Value = true;
        }

        public bool IsRenderingThread => _isRenderingThread.Value;

        public void RunPendingTasks()
        {
            
        }


        public void FireOnUIAndForget(Task task)
        {
            _pendingTasks.Add(async () => await task);
        }
    }
}
