using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinaryVibrance.INPCSourceGenerator;
using Slate.Client.Networking;
using Slate.Client.UI.MVVM;

namespace Slate.Client.UI.ViewModels
{

	public record GameCharacter(Guid Id, string Name);

	public partial class CharacterListViewModel
    {
        private readonly GameConnection _gameConnection;

        [ImplementNotifyPropertyChanged] private IEnumerable<GameCharacter> _characters = new List<GameCharacter>();
        [ImplementNotifyPropertyChanged] private RelayCommand _playAsCharacterCommand;

        private GameCharacter? _selectedCharacter;
        public GameCharacter? SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (SetField(ref _selectedCharacter, value))
                {
                    RaisePropertyChanged(nameof(CanEnterGame));
                    _playAsCharacterCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool CanEnterGame => SelectedCharacter?.Id is not null;


        public CharacterListViewModel(GameConnection gameConnection)
        {
            _gameConnection = gameConnection;

            PlayAsCharacterCommand = new RelayCommand(Execute, CanExecute);
        }

        void Execute() => _gameConnection.PlayAsCharacter(SelectedCharacter.Id);
        bool CanExecute() => CanEnterGame;

        public async Task OnNavigatedTo()
        {
            var characters = await _gameConnection.GetCharacters();
            Characters = characters.Select(c => new GameCharacter(c.Id.ToGuid(), c.Name)).ToList();
        }
    }
}
