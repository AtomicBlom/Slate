using System.Reactive.Linq;
using System.Threading.Tasks;
using Serilog;
using Slate.Backend.Shared;
using Slate.Events.InMemory;
using Slate.Networking.External.Protocol.ClientToServer;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.Internal.Protocol.Overseer;
using Slate.Networking.RabbitMQ;
using Slate.Networking.Shared.Protocol;

namespace Slate.GameWarden.Game
{
    public class PlayerCellService : IPlayerService, IHandleClientMessage<ClientRequestMove>
    {
        private readonly CharacterIdentifier _characterIdentifier;
        private readonly IRPCClient _rpcClient;
        private readonly ICellConnectionManager _cellConnectionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger _logger;

        private Uuid? ConnectedCellId { get; set; }

        public PlayerCellService(CharacterIdentifier characterIdentifier, IRPCClient rpcClient, ICellConnectionManager cellConnectionManager, IEventAggregator eventAggregator, ILogger logger)
        {
            _characterIdentifier = characterIdentifier;
            _rpcClient = rpcClient;
            _cellConnectionManager = cellConnectionManager;
            _eventAggregator = eventAggregator;
            _logger = logger.ForContext<ILogger>();
        }
        
        public async Task MoveToCellAsync(string cellName)
        {
            using var cellNameContext = CommonLogContexts.CellName(cellName);
            var response = await _rpcClient.CallAsync<GetCellServerRequest, GetCellServerResponse>(new GetCellServerRequest { CellName = cellName });
            using var cellIdContext = CommonLogContexts.ApplicationInstanceId(response.Id.ToGuid());

            _logger.Information("Cell is located at {@Endpoint}", response.Endpoint);
            var connectTask = await _cellConnectionManager.GetOrConnectAsync(response.Id.ToGuid(), response.Endpoint);
            await connectTask;

            _eventAggregator.Publish(
                new ConnectPlayerMessage(
                    response.Id, 
                    _characterIdentifier.CharacterId.ToUuid(),
                new Vector3()));

            ConnectedCellId = response.Id;
        }

        public Task Handle(ClientRequestMove message)
        {
            if (ConnectedCellId is null)
            {
                _logger.Warning("Client is not yet connected to a cell");
                return Task.CompletedTask;
            }

            _eventAggregator.Publish(new CellRequestMove(
                ConnectedCellId,
                _characterIdentifier.CharacterId.ToUuid(),
                message.Location,
                message.Velocity
                ));

            return Task.CompletedTask;
        }
    }
}