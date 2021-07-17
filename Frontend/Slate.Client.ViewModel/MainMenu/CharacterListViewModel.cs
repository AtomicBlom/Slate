using System.Collections.Generic;
using System.Threading.Tasks;
using BinaryVibrance.INPCSourceGenerator;
using Slate.Client.UI.MVVM;
using Slate.Client.ViewModel.Services;

namespace Slate.Client.ViewModel.MainMenu
{
    
	public partial class CharacterListViewModel
    {
        private readonly ICharacterService _characterService;

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


        public CharacterListViewModel(ICharacterService characterService)
        {
            _characterService = characterService;

            PlayAsCharacterCommand = new RelayCommand(Execute, CanExecute);
        }

        void Execute() => _characterService.PlayAsCharacter(SelectedCharacter.Id);
        bool CanExecute() => CanEnterGame;

        public async Task OnNavigatedTo()
        {
            Characters = await _characterService.GetCharacters();
        }
    }
}
