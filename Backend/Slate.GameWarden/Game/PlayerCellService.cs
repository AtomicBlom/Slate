using System;
using System.Threading.Tasks;
using Slate.Networking.Internal.Protocol;
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
            _cellConnectionManager.GetOrConnect(response.Id.ToGuid(), response.IPAddress, response.Port);

        }
    }

    public interface ICellConnectionManager
    {
        void GetOrConnect(Guid guid, string ipAddress, int port);
    }
}