using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Slate.Client.ViewModel.Services
{
    public record GameCharacter(Guid Id, string Name);

    public interface ICharacterService
    {
        Task<IEnumerable<GameCharacter>> GetCharacters();
        Task PlayAsCharacter(Guid selectedCharacterId);
    }
}
