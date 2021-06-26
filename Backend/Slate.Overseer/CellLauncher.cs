using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nito.AsyncEx;
using Serilog;
using Serilog.Context;
using Slate.Backend.Shared;
using Slate.Networking.Internal.Protocol.Cell;
using Slate.Networking.Internal.Protocol.Model;
using Slate.Networking.Internal.Protocol.Overseer;
using Slate.Networking.RabbitMQ;
using Slate.Networking.Shared.Protocol;

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
        private readonly ILogger _logger;
        private IDisposable? _lifecycleToken;
        private readonly Dictionary<string, List<CellInstance>> _knownCells = new();
        private readonly AsyncReaderWriterLock _knownCellLock = new();
        private readonly uint PlayerLimitPerCell = 64;

        public CellLauncher(IRPCServer server, IApplicationLauncher applicationLauncher, IRabbitClient rabbitClient, ILogger logger)
        {
            _server = server;
            _applicationLauncher = applicationLauncher;
            _rabbitClient = rabbitClient;
            _logger = logger.ForContext<CellLauncher>();
        }

        //FIXME: player affinity
        private async Task<GetCellServerResponse> ProcessCellRequests(GetCellServerRequest request)
        {
            using var logContext = LogContext.PushProperty("CellName", request.CellName);
            IDisposable? instanceIdContext = null;
            try
            {
                //Best case, we already have an instance of the cell, find the best one
                var (existed, cellInstance) = await TryGetExistingCellServer(request, false);
                if (!existed)
                {
                    //We're going to need to create a new instance.
                    using (await _knownCellLock.WriterLockAsync())
                    {
                        //First we need to guard against the idea that two requests might have made it through the reader lock
                        (existed, cellInstance) = await TryGetExistingCellServer(request, true);
                        if (!existed)
                        {
                            _logger.Information("GetCellServerRequest is spawning a new instance of Snowglobe");
                            var instanceId = Guid.NewGuid();
                            instanceIdContext = CommonLogContexts.ApplicationInstanceId(instanceId);

                            var launchRequest = new TaskCompletionSource();

                            cellInstance = new CellInstance(
                                instanceId,
                                request.CellName,
                                launchRequest.Task,
                                new CellMetrics
                                {
                                    InstanceId = instanceId.ToUuid(),
                                    PlayerCount = 0
                                }
                            );

                            IDisposable? awakeSubscription = null;
                            awakeSubscription = _rabbitClient.Subscribe<NotifyCellServerAwake>(message =>
                            {
                                if (awakeSubscription is null)
                                    throw new Exception(
                                        "Processed the awake message before the awake subscription was assigned!?");
                                if (instanceId.Equals(message.Id.ToGuid()))
                                {
                                    using var innerInstanceId = CommonLogContexts.ApplicationInstanceId(instanceId);

                                    _logger.Information("Received notification that the cell is now awake");
                                    cellInstance.Endpoint = message.Endpoint;
                                    launchRequest.SetResult();
                                    awakeSubscription?.Dispose();
                                }

                                return Task.CompletedTask;
                            });

                            //Ok good, we're the only one in here now, let's launch it in the background
                            var _ = _applicationLauncher.LaunchAsync("Snowglobe", new()
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
                    }
                }

                if (cellInstance is null)
                {
                    throw new Exception("The cell instance was null somehow???");
                }

                if (!cellInstance.LaunchRequest.IsCompleted)
                {
                    _logger.Information("Waiting for cell to come alive");
                }

                //Unlock from the queue and await for the cell to come up.
                await cellInstance.LaunchRequest;

                _logger.Information("Cell is alive");
                return new GetCellServerResponse
                {
                    Id = cellInstance.InstanceId.ToUuid(),
                    Endpoint = cellInstance.Endpoint
                };
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to locate a cell server on demand");
                throw;
            }
            finally
            {
                instanceIdContext?.Dispose();
            }
        }

        private async Task<(bool Existing, CellInstance? cellInstance)> TryGetExistingCellServer(GetCellServerRequest request, bool useExistingLock)
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
                    return (false, null);

                bestCell = cellInstances
                    .Where(ci => ci.Metrics.PlayerCount > PlayerLimitPerCell)
                    .OrderBy(ci => ci.Metrics.PlayerCount)
                    .FirstOrDefault();

                if (bestCell is null)
                    return (false, null);
            }
            finally
            {
                innerLock?.Dispose();
            }

            return (true, bestCell);

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
}
