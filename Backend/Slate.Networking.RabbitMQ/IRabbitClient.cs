using System;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Slate.Networking.RabbitMQ
{
    public interface IRabbitClient
    {
        IDisposable Subscribe<T>(Func<T, Task> action);
        void Send<T>(T message);
        IRPCClient CreateRPCClient();
        IRPCServer CreateRPCServer();
    }
}