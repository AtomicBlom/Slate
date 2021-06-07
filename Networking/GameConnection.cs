using System.Threading.Tasks;
using Game.CoreNetworking;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;

namespace Networking
{
    public class GameConnection
    {
        private readonly string _serverHost;
        private readonly int _serverPort;
        private GrpcChannel? _channel;

        public GameConnection(string serverHost, int serverPort)
        {
            _serverHost = serverHost;
            _serverPort = serverPort;
        }

        public async Task<(bool WasSuccessful, string? ErrorMessage)> Connect(string authToken)
        {
            _channel = GrpcChannel.ForAddress($"https://{_serverHost}:{_serverPort}");
            var auth = _channel.CreateGrpcService<IAuth>();
            var response = await auth.AuthorizeAsync(new AuthorizeRequest
            {
                AuthToken = authToken
            });

            return (response.WasSuccessful, response.ErrorMessage);
        }
    }
}
