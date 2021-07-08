using System;

namespace Slate.Events.InMemory
{
    public interface IEventAggregator : IDisposable
    {
        IObservable<TEvent> GetEvent<TEvent>();
        void Publish<TEvent>(TEvent sampleEvent);
    }
}