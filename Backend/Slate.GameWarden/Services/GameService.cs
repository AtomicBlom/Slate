using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Slate.GameWarden.Game;
using Slate.Networking.External.Protocol;

namespace Slate.GameWarden.Services
{
    public class GameService : IGameService
    {
        private readonly IPlayerLocator _playerLocator; 

        public GameService(IPlayerLocator playerLocator)
        {
            _playerLocator = playerLocator;
        }
        
        public async IAsyncEnumerable<GameServerUpdate> SubscribeAsync(IAsyncEnumerable<GameClientUpdate> clientUpdates, CallContext context = default)
        {
            var awaitLogin = new TaskCompletionSource<CharacterCoordinator>();

            var _ = Task.Run(async () =>
            {
                var clientEnumerator = clientUpdates.GetAsyncEnumerator();
                try
                {
                    if (!await clientEnumerator.MoveNextAsync()) throw new Exception("Missing first message");
                    if (!clientEnumerator.Current.ShouldSerializeConnectToGameRequest())
                    {
                        throw new Exception($"Expected first message to be a {nameof(ConnectToGameRequest)}");
                    }

                    var connectRequest = clientEnumerator.Current.ConnectToGameRequest;

                    var character = await _playerLocator.GetOrCreatePlayer(connectRequest.CharacterId.ToGuid());
                    if (character is null)
                    {
                        throw new Exception("Could not create or find character");
                    }
                }
                catch (Exception e)
                {
                    awaitLogin.SetException(e);
                }

                while (await clientEnumerator.MoveNextAsync())
                {
                    //FIXME: Process client messages, look up processors via the discriminator
                    Console.WriteLine($"Received from client: {clientEnumerator.Current.ClientRequestMove}");
                }
            });

            var characterInstance = await awaitLogin.Task;
            
            await foreach (var update in characterInstance.Updates)
            {
                yield return update;
            }
        }
    }
}
