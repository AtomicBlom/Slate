using System.Threading.Tasks;
using Slate.Networking.External.Protocol;

namespace Slate.GameWarden.Game
{
    public interface IPlayerServiceHost
    {
        Task QueueOutgoingMessage(GameServerUpdate message);
    }
}