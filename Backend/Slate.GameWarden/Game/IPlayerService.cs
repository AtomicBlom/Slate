using System.Threading.Tasks;
using Slate.Networking.External.Protocol.ClientToServer;

namespace Slate.GameWarden.Game
{
    public interface IHandleClientMessage<in T> where T : ClientToServerMessage
    {
        Task Handle(T message);
    }

    public interface IPlayerService
    {
        void StartService() {}
    }
}