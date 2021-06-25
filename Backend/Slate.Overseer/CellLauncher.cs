using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using Slate.Networking.Internal.Protocol;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.RabbitMQ;

namespace Slate.Overseer
{
    internal record CellInstance(
        Guid InstanceId,
        string CellName,
        Task LaunchRequest,
        CellMetrics Metrics)
    {
        public Endpoint? Endpoint { get; set; }
    }

    public class CellLauncher : IHostedService
    {
        private readonly IRPCServer _server;
        private readonly IApplicationLauncher _applicationLauncher;
        private readonly IRabbitClient _rabbitClient;
        private IDisposable? _lifecycleToken;
        private readonly Dictionary<string, List<CellInstance>> _knownCells = new();
        private readonly AsyncReaderWriterLock _knownCellLock = new();
        private readonly uint PlayerLimitPerCell = 64;

        public CellLauncher(IRPCServer server, IApplicationLauncher applicationLauncher, IRabbitClient rabbitClient)
        {
            _server = server;
            _applicationLauncher = applicationLauncher;
            _rabbitClient = rabbitClient;
        }

        public async Task<GetCellServerResponse> ProcessCellRequests(GetCellServerRequest request)
        {
            //Best case, we already have an instance of the cell, find the best one
            //FIXME: player affinity
            

            var (existed, id, endpoint) = await TryGetExistingCellServer(request, false);
            if (existed)
            {
                return new GetCellServerResponse
                {
                    Id = id!.Value.ToUuid(),
                    Endpoint = endpoint
                };
            }

            //We're going to need to create a new instance.
            CellInstance cellInstance;
            using (await _knownCellLock.WriterLockAsync())
            {
                //First we need to guard against the idea that two requests might have made it through the reader lock
                (existed, id, endpoint) = await TryGetExistingCellServer(request, true);
                if (existed)
                {
                    return new GetCellServerResponse
                    {
                        Id = id!.Value.ToUuid(),
                        Endpoint = endpoint
                    };
                }

                var instanceId = Guid.NewGuid();

                var launchRequest = new TaskCompletionSource();
                cellInstance = new CellInstance(
                    instanceId,
                    request.CellName,
                    launchRequest.Task,
                    new CellMetrics
                    {
                        InstanceId = new Uuid(),
                        PlayerCount = 0
                    }
                );

                
                using var awakeSubscription = _rabbitClient.Subscribe<NotifyCellServerAwake>(message =>
                {
                    cellInstance.Endpoint = message.Endpoint;
                    launchRequest.SetResult();
                    return Task.CompletedTask;
                });

                //Ok good, we're the only one in here now, let's launch it in the background
                var _ = _applicationLauncher.LaunchAsync("Snowglobe", new ()
                {
                    { "--Id", instanceId.ToString() },
                    { "--Cell", request.CellName }
                });

                if (!_knownCells.TryGetValue(request.CellName, out var cellInstances))
                {
                    cellInstances = new List<CellInstance>();
                    _knownCells.Add(request.CellName, cellInstances);
                }

                cellInstances.Add(cellInstance);
            }

            //Unlock from the queue and await for the cell to come up.
            await cellInstance.LaunchRequest;
            return new GetCellServerResponse
            {
                Id = cellInstance.InstanceId.ToUuid(),
                Endpoint = cellInstance.Endpoint
            };
        }

        private async Task<(bool Existing, Guid? Id, Endpoint? Endpoint)> TryGetExistingCellServer(GetCellServerRequest request, bool useExistingLock)
        {
            CellInstance? bestCell;
            IDisposable? innerLock = null;
            try
            {
                if (!useExistingLock)
                {
                    innerLock = await _knownCellLock.ReaderLockAsync();
                }

                if (!_knownCells.TryGetValue(request.CellName, out var cellInstances))
                    return (false, null, null);

                bestCell = cellInstances
                    .Where(ci => ci.Metrics.PlayerCount > PlayerLimitPerCell)
                    .OrderBy(ci => ci.Metrics.PlayerCount)
                    .FirstOrDefault();

                if (bestCell is null)
                    return (false, null, null);
            }
            finally
            {
                innerLock?.Dispose();
            }

            await bestCell.LaunchRequest;
            return (true, bestCell.InstanceId, bestCell.Endpoint);

        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _lifecycleToken = _server.Serve<GetCellServerRequest, GetCellServerResponse>(ProcessCellRequests);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _lifecycleToken?.Dispose();
            _lifecycleToken = null;
            return Task.CompletedTask;
        }
    }

    public interface IApplicationLauncher
    {
        Task LaunchAsync(string applicationDefinitionName, Dictionary<string, string?>? arguments = null);
        Task ExitAllApplicationsAsync();
    }
}
