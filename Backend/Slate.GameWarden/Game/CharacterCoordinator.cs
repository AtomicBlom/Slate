using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Slate.Networking.External.Protocol;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;

namespace Slate.GameWarden.Game
{
    public class CharacterCoordinator : IDisposable
    {
        private readonly Guid _id;
        private readonly IEnumerable<IPlayerService> _playerServices;
        private IRabbitClient _rabbitClient;
        private IRPCClient _rpcClient;

        public CharacterCoordinator(Guid id, IRabbitClient rabbitClient, IEnumerable<IPlayerService> playerServices)
        {
            _id = id;
            _rabbitClient = rabbitClient;
            _playerServices = playerServices;
        }

        public IAsyncEnumerable<GameServerUpdate> Updates { get; }

        public void StartCoordinating()
        {
            _rpcClient = _rabbitClient.CreateRPCClient();
            foreach (var playerService in _playerServices)
            {
                playerService.StartService(_rabbitClient, _rpcClient);
            }
        }

        public async Task MoveToCellAsync(string cellName)
        {
            var getCellResponse = await _rpcClient.CallAsync<GetCellServerRequest, GetCellServerResponse>(
                new GetCellServerRequest()
                {
                    CellName = cellName
                });
            
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class CellPlayerService : IPlayerService
    {
        public void StartService(IRabbitClient rabbitClient, IRPCClient rpcClient)
        {
            throw new NotImplementedException();
        }
    }

    public interface IPlayerService
    {
        void StartService(IRabbitClient rabbitClient, IRPCClient rpcClient);
    }
}
