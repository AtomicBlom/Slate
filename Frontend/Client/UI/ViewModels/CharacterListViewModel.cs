using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Client.Annotations;
using EmptyKeys.UserInterface.Input;
using Networking;

namespace Client.UI.ViewModels
{
    public class CharacterListViewModel : INotifyPropertyChanged
    {
        private readonly GameConnection _gameConnection;
        private IEnumerable<GameCharacter> _characters;

        public CharacterListViewModel(GameConnection gameConnection)
        {
            _gameConnection = gameConnection;
            Characters = new List<GameCharacter>();
        }

        public IEnumerable<GameCharacter> Characters
        {
            get => _characters;
            set => SetField(ref _characters, value);
        }

        public ICommand? PlayAsCharacterCommand { get; set; }
        
        public async Task OnNavigatedTo()
        {
            var characters = await _gameConnection.GetCharacters();
            Characters = characters.Select(c => new GameCharacter(c.Id.ToGuid(), c.Name)).ToList();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void RaisePropertyChanged(string? propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        [NotifyPropertyChangedInvocator]
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
