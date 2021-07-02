using System;
using System.Threading.Tasks;

namespace Slate.Networking.RabbitMQ
{
    public interface IRPCClient : IDisposable
    {
        Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request, bool silent = false);
    }
}