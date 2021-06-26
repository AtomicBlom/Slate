using System.Threading.Tasks;
using Slate.Networking.External.Protocol;
using Slate.Networking.External.Protocol.ServerToClient;

namespace Slate.GameWarden.Game
{
    public interface IPlayerServiceHost
    {
        Task QueueOutgoingMessage(ServerToClientMessage message);
    }
}