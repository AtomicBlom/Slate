using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Slate.GameWarden.Game;

namespace Slate.GameWarden.Services
{
    internal class PlayerLocator : IPlayerLocator
    {
        private readonly Func<CharacterIdentifier, PlayerConnection> _serviceScopeFactory;

        public PlayerLocator(Func<CharacterIdentifier, PlayerConnection> serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        private readonly Dictionary<Guid, PlayerConnection> _loggedInCharacters = new();

        public async Task<PlayerConnection> GetOrCreateCharacter(CharacterIdentifier characterIdentifier)
        {
            var (playerId, characterId) = characterIdentifier;
            PlayerConnection? characterInstance;

            bool initializeCharacter = false;
            lock (_loggedInCharacters)
            {
                if (!_loggedInCharacters.TryGetValue(characterId, out characterInstance))
                {
                    
                    characterInstance = _serviceScopeFactory(characterIdentifier);
                    _loggedInCharacters.Add(characterId, characterInstance);
                    initializeCharacter = true;
                }
            }

            if (characterInstance is null) throw new Exception("Unable to create character");

            if (initializeCharacter)
            {
                var cellPlayerService = characterInstance
                                            .GetService<PlayerCellService>()
                                        ?? throw new Exception($"Could not locate {nameof(PlayerCellService)}");
                await cellPlayerService.MoveToCellAsync("Harlan Port 1-1");
            }

            return characterInstance;
        }
    }
}