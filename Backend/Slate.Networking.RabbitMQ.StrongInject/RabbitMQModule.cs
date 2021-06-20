using Microsoft.Extensions.Configuration;
using StrongInject;

namespace Slate.Networking.RabbitMQ.StrongInject
{
    [Register(typeof(RabbitClient), Scope.SingleInstance, typeof(IRabbitClient))]
    public class RabbitMQModule
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