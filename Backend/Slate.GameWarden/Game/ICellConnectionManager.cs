using System;
using System.Threading.Tasks;
using Slate.Networking.Internal.Protocol.Model;

namespace Slate.GameWarden.Game
{
    public interface ICellConnectionManager
    {
        Task<Task> GetOrConnectAsync(Guid guid, Endpoint endpoint);
    }
}