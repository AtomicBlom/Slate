using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Slate.Networking.External.Protocol;

namespace Slate.GameWarden.Game
{
    public class PlayerConnection : IDisposable
    {
        private readonly Guid _userId;
        private readonly Guid _characterId;
        private readonly IPlayerService[] _playerServices;
        private readonly ILogger _logger;
        private bool _disposed;

        public PlayerConnection(Guid userId, Guid characterId, IPlayerService[] playerServices, ILogger logger)
        {
            _userId = userId;
            _characterId = characterId;
            _playerServices = playerServices;
            _logger = logger.ForContext<PlayerConnection>()
                .ForContext("UserId", userId)
                .ForContext("CharacterId", characterId);
        }

        public IAsyncEnumerable<GameServerUpdate> Updates { get; }
        
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

        public async Task HandleIncomingMessages(IAsyncEnumerator<GameClientUpdate> clientEnumerator)
        {
            while (await clientEnumerator.MoveNextAsync())
            {
                try
                {
                    _logger.Verbose("Received {MessageType} from client: ", clientEnumerator.Current.MessageCase);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error processing a message from the client");
                }
            }
        }
    }
}
