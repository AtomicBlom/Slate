using System;

namespace Game.Networking.Internal.Client.RabbitMQ
{
    internal class ActionDisposable : IDisposable
    {
        private readonly Action _actionOnDispose;
        private bool _disposed = false;

        public ActionDisposable(Action actionOnDispose)
        {
            _actionOnDispose = actionOnDispose;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _actionOnDispose();
            _disposed = true;
        }
    }
}