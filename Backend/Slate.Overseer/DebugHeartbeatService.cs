using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Slate.Networking.Internal.Protocol.Shared;
using Slate.Networking.RabbitMQ;

namespace Slate.Overseer
{
    public class DebugHeartbeatService : IHostedService
    {
        private readonly IRPCServer _rpcServer;
        private IDisposable _serviceToken;

        public DebugHeartbeatService(IRPCServer rpcServer)
        {
            _rpcServer = rpcServer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _serviceToken = _rpcServer.Serve<HeartbeatRequest, HeartbeatResponse>(ProcessHeartbeat, silent: true);
            return Task.CompletedTask;
        }

        public Task<HeartbeatResponse> ProcessHeartbeat(HeartbeatRequest request)
        {
            return Task.FromResult(new HeartbeatResponse());

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _serviceToken.Dispose();
            return Task.CompletedTask;
        }
    }


}
