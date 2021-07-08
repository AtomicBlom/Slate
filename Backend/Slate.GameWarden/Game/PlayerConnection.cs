using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Serilog;
using Slate.Events.InMemory;
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
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;
        private bool _disposed;
        private BufferBlock<ServerToClientMessage> MessagesToServer = new();

        public PlayerConnection(
            CharacterIdentifier characterIdentifier, 
            IPlayerService[] playerServices, 
            IEventAggregator eventAggregator,
            ILogger logger)
        {
            (_userId, _characterId) = characterIdentifier;
            _playerServices = playerServices;
            _eventAggregator = eventAggregator;
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
                    var message = clientEnumerator.Current;
                    _logger.Verbose("Received {MessageType} from client: ", message.GetType().Name);
                    var handlers = GetMessageHandlers(message.GetType());
                    foreach (var handleMessage in handlers)
                    {
                        handleMessage(message);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Error processing a message from the client");
                }
            }
        }

        delegate void HandleMessage(ClientToServerMessage clientToServerMessage);

        private List<HandleMessage> GetMessageHandlers(Type type)
        {
            MethodInfo? GetMethod(Type type1, Type serviceType)
            {
                return serviceType.GetMethod(nameof(IHandleClientMessage<ClientToServerMessage>.Handle),
                    0, new[] { type1 });
            }

            var candidates =
                from service in _playerServices
                let serviceType = service.GetType()
                from interf in service.GetType().GetInterfaces()
                where interf.IsGenericType
                where interf.GetGenericTypeDefinition() == typeof(IHandleClientMessage<>)
                where interf.GenericTypeArguments.Single() == type
                let handleMethod = GetMethod(type, serviceType)
                let action = (HandleMessage)(m => handleMethod.Invoke(service, new object?[] { m }))
                select action;

            return candidates.ToList();
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
