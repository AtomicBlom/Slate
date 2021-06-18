using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Slate.Networking.External.Protocol;

namespace Slate.GameWarden.Game
{
    public class CharacterCoordinator : IDisposable
    {
        private readonly Guid _id;
        private readonly IServiceScope _serviceScope;
        private readonly IEnumerable<IPlayerService> _playerServices;
        private bool _disposed;

        public CharacterCoordinator(Guid id, IServiceScope serviceScope, IEnumerable<IPlayerService> playerServices)
        {
            _id = id;
            _serviceScope = serviceScope;
            _playerServices = playerServices;
        }

        public IAsyncEnumerable<GameServerUpdate> Updates { get; }

        public void StartCoordinating()
        {
            foreach (var playerService in _playerServices)
            {
                playerService.StartService();
            }
        }
        
        public T? GetService<T>()
        {
            return _playerServices.OfType<T>().FirstOrDefault();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _serviceScope.Dispose();
                _disposed = true;
            }
        }
    }
}
