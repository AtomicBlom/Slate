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
            _cellConnectionManager.GetOrConnect(response.Id.ToGuid(), response.Endpoint);

        }
    }

    public interface ICellConnectionManager
    {
        void GetOrConnect(Guid guid, Endpoint endpoint);
    }

    public class CellConnectionManager : ICellConnectionManager
    {
        public void GetOrConnect(Guid guid, Endpoint endpoint)
        {
            throw new NotImplementedException();
        }
    }
}