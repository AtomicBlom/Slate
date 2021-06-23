using System;
using System.Threading.Tasks;
using Slate.GameWarden.Game;

namespace Slate.GameWarden.Services
{
    public interface IPlayerLocator
    {
        Task<PlayerConnection> GetOrCreateCharacter(Guid playerId, Guid characterId);
    }
}