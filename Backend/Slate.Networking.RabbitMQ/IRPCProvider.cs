using RabbitMQ.Client;

namespace Slate.Networking.RabbitMQ
{
    internal interface IRPCProvider
    {
        IModel Model { get; }
        IRabbitSettings Settings { get; }
    }
}