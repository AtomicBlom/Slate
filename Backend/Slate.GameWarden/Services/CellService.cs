using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.RabbitMQ;

namespace Slate.GameWarden.Services
{
    public class CellService : ICellService
    {
        private readonly IRPCClient _rpcClient;

        public CellService(IRPCClient rpcClient)
        {
            _rpcClient = rpcClient;
        }

        public async Task FindCell(string cellName) //Later, a method to determine affinities?
        {
            //Ask Overseer for any cells matching the cell name with capacity
            var getCellResponse = await _rpcClient.CallAsync<GetCellServerRequest, GetCellServerResponse>(
                new GetCellServerRequest
                {
                    CellName = cellName
                });

            //Identify a usable cell (player affinities?)
            //Check if we have an existing cell connection
            //If not, create one
        }
    }

    public interface ICellService
    {
    }
}
