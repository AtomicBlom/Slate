using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinaryVibrance.INPCSourceGenerator;
using EmptyKeys.UserInterface.Input;
using Slate.Client.Networking;
using Slate.Client.UI.Common.Model;

namespace Slate.Client.UI.ViewModels
{
    public partial class CharacterListViewModel
    {
        private readonly GameConnection _gameConnection;

        [ImplementNotifyPropertyChanged] private IEnumerable<GameCharacter> _characters = new List<GameCharacter>();
        [ImplementNotifyPropertyChanged] private ICommand? _playAsCharacterCommand = null;

        private GameCharacter? _selectedCharacter;
        public GameCharacter? SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (SetField(ref _selectedCharacter, value))
                {
                    RaisePropertyChanged(nameof(CanEnterGame));
                    RaisePropertyChanged(nameof(PlayAsCharacterCommand));
                }
            }
        }

        public bool CanEnterGame => SelectedCharacter?.Id is not null;


        public CharacterListViewModel(GameConnection gameConnection)
        {
            _gameConnection = gameConnection;

            PlayAsCharacterCommand = new RelayCommand(Execute, CanExecute);
        }

        void Execute(object o) => _gameConnection.PlayAsCharacter(SelectedCharacter.Id);
        bool CanExecute(object o) => CanEnterGame;

        public async Task OnNavigatedTo()
        {
            var characters = await _gameConnection.GetCharacters();
            Characters = characters.Select(c => new GameCharacter(c.Id.ToGuid(), c.Name)).ToList();
        }
    }
}
