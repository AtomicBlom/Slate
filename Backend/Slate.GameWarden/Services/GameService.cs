using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf.Grpc;
using Serilog;
using Slate.GameWarden.Game;
using Slate.Networking.External.Protocol;
using Slate.Networking.External.Protocol.ClientToServer;
using Slate.Networking.External.Protocol.ServerToClient;
using Slate.Networking.External.Protocol.Services;

namespace Slate.GameWarden.Services
{
    public class GameService : IGameService
    {
        private readonly IPlayerLocator _characterLocator;
        private readonly ILogger _logger;

        public GameService(IPlayerLocator characterLocator, ILogger logger)
        {
            _characterLocator = characterLocator;
            _logger = logger.ForContext<GameService>();
        }
        
        public async IAsyncEnumerable<ServerToClientMessage> SubscribeAsync(IAsyncEnumerable<ClientToServerMessage> clientUpdates, CallContext context = default)
        {
            PlayerConnection? character = null;

            var clientEnumerator = clientUpdates.GetAsyncEnumerator();
            try
            {
                //FIXME: Read this from the header
                var userId = Guid.NewGuid();

                if (!await clientEnumerator.MoveNextAsync()) throw new Exception("Missing first message");
                if (clientEnumerator.Current is not ConnectToRequest connectRequest)
                {
                    throw new Exception($"Expected first message to be a {nameof(ConnectToRequest)}");
                }

                character = await _characterLocator.GetOrCreateCharacter(new CharacterIdentifier(userId, connectRequest.CharacterId.ToGuid()));
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

            var _ = Task.Run(async () => await character.HandleIncomingMessages(clientEnumerator));

            await foreach (var update in character.HandleOutgoingMessages())
            {
                yield return update;
            }
        }
    }
}
