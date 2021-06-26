using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Slate.Networking.External.Protocol.ClientToServer;
using Slate.Networking.External.Protocol.Model;
using Slate.Networking.External.Protocol.Services;
using Slate.Networking.Shared.Protocol;

namespace Slate.Client.Networking
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
            _channel = GrpcChannel.ForAddress($"http://{_serverHost}:{_serverPort}", new GrpcChannelOptions()
            {
                HttpClient = new HttpClient()
                {
                    DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", authToken)}
                }
            });
            
            var auth = _channel.CreateGrpcService<IAuthorizationService>();
            var response = await auth.AuthorizeAsync(new AuthorizeRequest());

            return (response.WasSuccessful, response.ErrorMessage);
        }

        public async Task<IReadOnlyList<Character>> GetCharacters()
        {
            if (_channel is null) throw new Exception("Channel not ready yet!");
            var account = _channel.CreateGrpcService<IAccountService>();
            var response = await account.GetCharactersAsync(new GetCharactersRequest());
            return response.Characters;
        }

        public async void PlayAsCharacter(Guid character)
        {
            if (_channel is null) throw new Exception("Channel not ready yet!");
            var gameService = _channel.CreateGrpcService<IGameService>();

            var serverMessages = gameService.SubscribeAsync(PublishClientMessages(character));

            await foreach (var message in serverMessages)
            {
                Console.WriteLine($"Received message type {message.GetType().Name}");
            }
        }

        public async IAsyncEnumerable<ClientToServerMessage> PublishClientMessages(Guid characterId)
        {
            yield return new ConnectToRequest()
            {
                CharacterId = characterId.ToUuid()
            };

            while (true)
            {
                await Task.Delay(1000);
                yield return new ClientRequestMove
                {
                    Location = new Vector3() { X = 0, Y = 0, Z = 0 },
                    Velocity = new Vector3() { X = 0, Y = 0, Z = 0 }
                };
                Console.WriteLine($"Sent message type ClientRequestMove");
            }
        }
    }
}
