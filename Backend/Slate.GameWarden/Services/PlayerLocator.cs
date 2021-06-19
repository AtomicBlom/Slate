using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Slate.GameWarden.Game;
using StrongInject;

namespace Slate.GameWarden.Services
{
    internal class PlayerLocator : IPlayerLocator
    {
        private readonly Func<Guid, CharacterCoordinator> _serviceScopeFactory;

        public PlayerLocator(Func<Guid, CharacterCoordinator> serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        private readonly Dictionary<Guid, CharacterCoordinator> _loggedInCharacters = new();

        public async Task<CharacterCoordinator> GetOrCreatePlayer(Guid characterId)
        {
            CharacterCoordinator? characterInstance;

            bool initializeCharacter = false;
            lock (_loggedInCharacters)
            {
                if (!_loggedInCharacters.TryGetValue(characterId, out characterInstance))
                {
                    
                    characterInstance = _serviceScopeFactory(characterId);
                    _loggedInCharacters.Add(characterId, characterInstance);
                    initializeCharacter = true;
                }
            }

            if (characterInstance is null) throw new Exception("Unable to create character");

            if (initializeCharacter)
            {
                var cellPlayerService = characterInstance
                                            .GetService<CellPlayerService>()
                                        ?? throw new Exception($"Could not locate {nameof(CellPlayerService)}");
                await cellPlayerService.MoveToCellAsync("Harlan Port 1-1");
            }

            return characterInstance;
        }
    }
}