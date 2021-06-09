using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Client.UI.Common.Model;
using EmptyKeys.UserInterface.Input;
using Networking;
using BinaryVibrance.INPCSourceGenerator;

namespace Client.UI.ViewModels
{
    public partial class CharacterListViewModel
    {
        private readonly GameConnection _gameConnection;

        [ImplementNotifyPropertyChanged]
        private IEnumerable<GameCharacter> _characters = new List<GameCharacter>();

        public CharacterListViewModel(GameConnection gameConnection)
        {
            _gameConnection = gameConnection;
        }

        public ICommand? PlayAsCharacterCommand { get; set; }
        
        public async Task OnNavigatedTo()
        {
            var characters = await _gameConnection.GetCharacters();
            Characters = characters.Select(c => new GameCharacter(c.Id.ToGuid(), c.Name)).ToList();
        }

        //public event PropertyChangedEventHandler? PropertyChanged;

        //protected void RaisePropertyChanged(string? propertyName) =>
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        //[NotifyPropertyChangedInvocator]
        //protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        //{
        //    if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        //    field = value;
        //    RaisePropertyChanged(propertyName);
        //    return true;
        //}
    }
}
