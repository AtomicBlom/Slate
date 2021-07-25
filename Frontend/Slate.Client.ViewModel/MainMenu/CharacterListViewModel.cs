using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using BinaryVibrance.INPCSourceGenerator;
using Slate.Client.UI.MVVM;
using Slate.Client.ViewModel.Services;

namespace Slate.Client.ViewModel.MainMenu
{
    
	public partial class CharacterListViewModel
    {
        private readonly ICharacterService _characterService;
        private readonly Action _characterSelectedAction;

        [ImplementNotifyPropertyChanged(PropertyAccess.SetterPrivate)] 
        private IEnumerable<GameCharacter> _characters = new List<GameCharacter>();
        private readonly RelayCommand _playAsCharacterCommand;

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
        public ICommand PlayAsCharacterCommand => _playAsCharacterCommand;


        public CharacterListViewModel(ICharacterService characterService, Action characterSelectedAction)
        {
            _characterService = characterService;
            _characterSelectedAction = characterSelectedAction;

            _playAsCharacterCommand = new RelayCommand(Execute, CanExecute);
        }

        void Execute()
        {
            _characterService.PlayAsCharacter(SelectedCharacter!.Id);
            _characterSelectedAction();
        }

        bool CanExecute() => CanEnterGame;

        public async Task OnNavigatedTo()
        {
            Characters = await _characterService.GetCharacters();
        }
    }
}
