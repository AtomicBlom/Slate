using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Slate.Networking.RabbitMQ;
using StrongInject;

namespace Slate.Backend.Shared
{
    [Register(typeof(RabbitClient), Scope.SingleInstance, typeof(IRabbitClient))]
    public class RabbitMQModule
    {
        [Factory(Scope.SingleInstance)]
        public static IRabbitSettings CreateRabbitSettings(IConfiguration configuration)
        {
            return configuration
                .GetSection(RabbitSettings.SectionName)
                .Get<RabbitSettings>() ?? throw new StrongInjectException("RabbitSettings missing from application configuration");
        }

        [Factory(Scope.SingleInstance)]
        public static IConnection CreateRabbitMQConnection(IRabbitSettings rabbitSettings)
        {
            var factory = new ConnectionFactory
                {
                    HostName = rabbitSettings.Hostname,
                    Port = rabbitSettings.Port,
                    UserName = rabbitSettings.Username,
                    Password = rabbitSettings.Password,
                    VirtualHost = rabbitSettings.VirtualHost,
                    ClientProvidedName = rabbitSettings.ClientName,
                    DispatchConsumersAsync = true
                };
                
            return factory.CreateConnection();
        }

        [Factory]
        public static IModel CreateRabbitMQModel(IConnection connection) => connection.CreateModel();

        [Factory]
        public static IRPCServer CreateRPCServer(IRabbitClient client) => client.CreateRPCServer();

        [Factory]
        public static IRPCClient CreateRPCClient(IRabbitClient client) => client.CreateRPCClient();
    }
}