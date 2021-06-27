using System.Net.Http.Headers;
using System.Threading.Tasks;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.Internal.Protocol.Overseer;
using Slate.Networking.RabbitMQ;

namespace Slate.GameWarden.Game
{
    public class PlayerCellService : IPlayerService
    {
        private readonly IRPCClient _rpcClient;
        private readonly ICellConnectionManager _cellConnectionManager;

        public PlayerCellService(IRPCClient rpcClient, ICellConnectionManager cellConnectionManager)
        {
            _rpcClient = rpcClient;
            _cellConnectionManager = cellConnectionManager;
        }
        
        public async Task MoveToCellAsync(string cellName)
        {
            var response = await _rpcClient.CallAsync<GetCellServerRequest, GetCellServerResponse>(new GetCellServerRequest { CellName = cellName });
            var connectTask = await _cellConnectionManager.GetOrConnectAsync(response.Id.ToGuid(), response.Endpoint);
            await connectTask;

        }
    }
}