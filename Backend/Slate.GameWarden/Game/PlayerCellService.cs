using System.Net.Http.Headers;
using System.Threading.Tasks;
using MessagePipe;
using Serilog;
using Slate.Backend.Shared;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.Internal.Protocol.Overseer;
using Slate.Networking.RabbitMQ;
using Slate.Networking.Shared.Protocol;

namespace Slate.GameWarden.Game
{
    public class PlayerCellService : IPlayerService
    {
        private readonly CharacterIdentifier _characterIdentifier;
        private readonly IRPCClient _rpcClient;
        private readonly ICellConnectionManager _cellConnectionManager;
        private readonly IBufferedAsyncPublisher<MessageToSnowglobe> _cellSendBus;
        private readonly ILogger _logger;

        public PlayerCellService(CharacterIdentifier characterIdentifier, IRPCClient rpcClient, ICellConnectionManager cellConnectionManager, ILogger logger, IBufferedAsyncPublisher<MessageToSnowglobe> cellSendBus)
        {
            _characterIdentifier = characterIdentifier;
            _rpcClient = rpcClient;
            _cellConnectionManager = cellConnectionManager;
            _cellSendBus = cellSendBus;
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

            await _cellSendBus.PublishAsync(
                new ConnectPlayerMessage(
                    response.Id, 
                    _characterIdentifier.CharacterId.ToUuid(),
                new Vector3()));

        }
    }
}