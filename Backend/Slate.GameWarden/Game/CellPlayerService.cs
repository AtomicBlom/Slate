using System.Threading.Tasks;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;

namespace Slate.GameWarden.Game
{
    public class CellPlayerService : IPlayerService
    {
        private readonly IRPCClient _rpcClient;

        public CellPlayerService(IRPCClient rpcClient)
        {
            _rpcClient = rpcClient;
        }
        
        public async Task MoveToCellAsync(string cellName)
        {
            
        }
    }
}