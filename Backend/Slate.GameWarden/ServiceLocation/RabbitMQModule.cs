using Microsoft.Extensions.Configuration;
using Slate.Networking.RabbitMQ;
using StrongInject;

namespace Slate.GameWarden.ServiceLocation
{
    [Register(typeof(RabbitClient), Scope.SingleInstance, typeof(IRabbitClient))]
    internal class RabbitMQModule
    {
        [Factory(Scope.SingleInstance)]
        public static IRabbitSettings CreateRabbitSettings(IConfiguration configuration)
        {
            return configuration
                .GetSection(RabbitSettings.SectionName)
                .Get<RabbitSettings>();
        } 

        [Factory]
        public static IRPCServer CreateRPCServer(IRabbitClient client) => client.CreateRPCServer();

        [Factory]
        public static IRPCClient CreateRPCClient(IRabbitClient client) => client.CreateRPCClient();
    }
}