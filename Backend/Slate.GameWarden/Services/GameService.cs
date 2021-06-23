using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Serilog;
using Slate.GameWarden.Game;
using Slate.Networking.External.Protocol;

namespace Slate.GameWarden.Services
{
    public class GameService : IGameService
    {
        private readonly IPlayerLocator _playerLocator;
        private readonly ILogger _logger;

        public GameService(IPlayerLocator playerLocator, ILogger logger)
        {
            _playerLocator = playerLocator;
            _logger = logger.ForContext<GameService>();
        }
        
        public async IAsyncEnumerable<GameServerUpdate> SubscribeAsync(IAsyncEnumerable<GameClientUpdate> clientUpdates, CallContext context = default)
        {
            PlayerConnection? character = null;

            var clientEnumerator = clientUpdates.GetAsyncEnumerator();
            try
            {
                //FIXME: Read this from the header
                var userId = Guid.NewGuid();

                if (!await clientEnumerator.MoveNextAsync()) throw new Exception("Missing first message");
                if (!clientEnumerator.Current.ShouldSerializeConnectToGameRequest())
                {
                    throw new Exception($"Expected first message to be a {nameof(ConnectToGameRequest)}");
                }

                var connectRequest = clientEnumerator.Current.ConnectToGameRequest;

                character = await _playerLocator.GetOrCreatePlayer(connectRequest.CharacterId.ToGuid());
                if (character is null)
                {
                    throw new Exception("Could not create or find character");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Could not connect player");
                throw;
            }

            Task.Run(async () => await character.HandleIncomingMessages(clientEnumerator));

            await foreach (var update in character.Updates)
            {
                yield return update;
            }
        }
    }
}
