using BinaryVibrance.INPCSourceGenerator;
using Slate.Client.UI.MVVM;

namespace Slate.Client.ViewModel.MainMenu
{
    public partial class IntroCardsViewModel
    {
        [ImplementNotifyPropertyChanged(PropertyAccess.GetterPrivate)]
        private RelayCommand? _nextCommand;

        public void Finish()
        {
            NextCommand?.Execute(null);
        }

        public void Skip()
        {
            NextCommand?.Execute(null);
        }
    }
}
