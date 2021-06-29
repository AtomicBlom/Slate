using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Nito.AsyncEx;
using ProtoBuf.Grpc.Client;
using Slate.Networking.Internal.Protocol.Cell.Services;
using Slate.Networking.Internal.Protocol.Model;

namespace Slate.GameWarden.Game
{
    public class CellConnectionManager : ICellConnectionManager
    {
        private readonly Dictionary<Guid, Task> _knownCells = new();
        private readonly AsyncReaderWriterLock _knownCellLock = new();
        
        public async Task<Task> GetOrConnectAsync(Guid guid, Endpoint endpoint)
        {
            using (await _knownCellLock.ReaderLockAsync())
            {
                if (_knownCells.TryGetValue(guid, out var connectTask))
                {
                    return connectTask;
                }
            }

            TaskCompletionSource? tcs;

            using (await _knownCellLock.WriterLockAsync())
            {
                if (_knownCells.TryGetValue(guid, out var connectTask))
                {
                    return connectTask;
                }

                tcs = new TaskCompletionSource();
                _knownCells.Add(guid, tcs.Task);
            }

            var channel = GrpcChannel.ForAddress($"http://{endpoint.Hostname}:{endpoint.Port}", new GrpcChannelOptions()
            {
                HttpClient = new HttpClient()
                {
                    //DefaultRequestHeaders = { Authorization = new AuthenticationHeaderValue("Bearer", authToken) }
                }
            });

            var cellService = channel.CreateGrpcService<ICellService>();

            tcs.SetResult();
            return tcs.Task;
        }
    }
}