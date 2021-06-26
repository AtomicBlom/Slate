using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Serilog;
using Slate.Networking.External.Protocol;
using Slate.Networking.External.Protocol.ClientToServer;
using Slate.Networking.External.Protocol.ServerToClient;

namespace Slate.GameWarden.Game
{
    public record CharacterIdentifier(Guid UserId, Guid CharacterId);

    public class PlayerConnection : IDisposable, IPlayerServiceHost
    {
        private readonly Guid _userId;
        private readonly Guid _characterId;
        private readonly IPlayerService[] _playerServices;
        private readonly ILogger _logger;
        private bool _disposed;
        private BufferBlock<ServerToClientMessage> MessagesToServer = new();

        public PlayerConnection(CharacterIdentifier characterIdentifier, IPlayerService[] playerServices, ILogger logger)
        {
            (_userId, _characterId) = characterIdentifier;
            _playerServices = playerServices;
            _logger = logger.ForContext<PlayerConnection>()
                .ForContext("UserId", _userId)
                .ForContext("CharacterId", _characterId);
        }

        public T? GetService<T>()
        {
            return _playerServices.OfType<T>().FirstOrDefault();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public async Task HandleIncomingMessages(IAsyncEnumerator<ClientToServerMessage> clientEnumerator)
        {
            while (await clientEnumerator.MoveNextAsync())
            {
                try
                {
                    _logger.Verbose("Received {MessageType} from client: ", clientEnumerator.Current.GetType().Name);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error processing a message from the client");
                }
            }
        }

        public async IAsyncEnumerable<ServerToClientMessage> HandleOutgoingMessages()
        {
            while (!_disposed)
            {
                ServerToClientMessage message;
                try
                {
                    message = await MessagesToServer.ReceiveAsync();
                }
                catch (InvalidOperationException e)
                {
                    break;
                }
                yield return message;
            }
        }

        public async Task QueueOutgoingMessage(ServerToClientMessage message)
        {
            await MessagesToServer.SendAsync(message);
        }
    }
}
