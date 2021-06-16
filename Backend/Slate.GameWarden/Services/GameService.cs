using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc;
using Slate.GameWarden.Game;
using Slate.Networking.External.Protocol;

namespace Slate.GameWarden.Services
{
    internal class GameService : IGameService
    {
        private readonly IPlayerLocator _playerLocator;

        public GameService(IPlayerLocator playerLocator)
        {
            _playerLocator = playerLocator;
        }
        
        public async IAsyncEnumerable<GameServerUpdate> SubscribeAsync(IAsyncEnumerable<GameClientUpdate> clientUpdates, CallContext context = default)
        {
            var awaitLogin = new TaskCompletionSource<CharacterCoordinator>();

            var _ = Task.Run(async () =>
            {
                var clientEnumerator = clientUpdates.GetAsyncEnumerator();
                try
                {
                    if (!clientEnumerator.Current.ShouldSerializeConnectToGameRequest())
                    {
                        throw new Exception($"Expected first message to be a {nameof(ConnectToGameRequest)}");
                    }

                    var connectRequest = clientEnumerator.Current.ConnectToGameRequest;

                    var character = await _playerLocator.GetOrCreatePlayer(connectRequest.CharacterId.ToGuid());
                    if (character is null)
                    {
                        throw new Exception("Could not create or find character");
                    }
                }
                catch (Exception e)
                {
                    awaitLogin.SetException(e);
                }

                while (await clientEnumerator.MoveNextAsync())
                {
                    //FIXME: Process client messages, look up processors via the discriminator
                }
            });

            var characterInstance = await awaitLogin.Task;
            
            await foreach (var update in characterInstance.Updates)
            {
                yield return update;
            }
        }
    }

    internal interface IPlayerLocator
    {
        Task<CharacterCoordinator> GetOrCreatePlayer(Guid characterId);
    }

    internal class PlayerLocator : IPlayerLocator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PlayerLocator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        private Dictionary<Guid, CharacterCoordinator> _loggedInCharacters = new();

        public async Task<CharacterCoordinator> GetOrCreatePlayer(Guid characterId)
        {
            CharacterCoordinator? characterInstance;

            bool initializeCharacter = false;
            lock (_loggedInCharacters)
            {
                if (!_loggedInCharacters.TryGetValue(characterId, out characterInstance))
                {
                    //FIXME: Load cell from character db
                    characterInstance = new CharacterCoordinator(characterId);
                    _loggedInCharacters.Add(characterId, characterInstance);
                    initializeCharacter = true;
                }
            }

            if (initializeCharacter)
            {
                await characterInstance.MoveToCellAsync("Harlan Port 1-1");
            }

            if (characterInstance is null) throw new Exception("Unable to create character");
            
            throw new NotImplementedException();
        }
    }
}
