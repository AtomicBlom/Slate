using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Slate.Client.Networking;
using Slate.Client.ViewModel.Services;

namespace Slate.Client.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly GameConnection _gameConnection;

        public CharacterService(GameConnection gameConnection)
        {
            _gameConnection = gameConnection;
        }

        public async Task<IEnumerable<GameCharacter>> GetCharacters()
        {
            var characters = await _gameConnection.GetCharacters();
            return characters.Select(c => new GameCharacter(c.Id.ToGuid(), c.Name));
        }

        public Task PlayAsCharacter(Guid selectedCharacterId)
        {
            _gameConnection.PlayAsCharacter(selectedCharacterId);
            return Task.CompletedTask;
        }
    }
}
