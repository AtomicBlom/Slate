using System;
using System.Collections.Generic;
using System.Linq;
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


        public T? GetService<T>()
        {
            return _playerServices.OfType<T>().FirstOrDefault();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class CellPlayerService : IPlayerService
    {
        private readonly IRPCClient _rpcClient;

        public CellPlayerService(IRabbitClient rabbitClient, IRPCClient rpcClient)
        {
            _rpcClient = rpcClient;
        }

        public void StartService()
        {
            
        }

        public async Task MoveToCellAsync(string cellName)
        {
            var getCellResponse = await _rpcClient.CallAsync<GetCellServerRequest, GetCellServerResponse>(
                new GetCellServerRequest()
                {
                    CellName = cellName
                });
        }
    }

    public interface IPlayerService
    {
        void StartService(IRabbitClient rabbitClient, IRPCClient rpcClient);
    }
}
