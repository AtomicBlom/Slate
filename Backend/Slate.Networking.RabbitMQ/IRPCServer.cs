using System;
using System.Threading.Tasks;

namespace Slate.Networking.RabbitMQ
{
    public interface IRPCServer
    {
        IDisposable Serve<TRequest, TResponse>(Func<TRequest, Task<TResponse>> processor);
    }
}