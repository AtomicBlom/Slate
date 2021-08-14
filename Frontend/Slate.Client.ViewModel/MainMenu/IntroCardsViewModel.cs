using Slate.Events.InMemory;

namespace Slate.Client.ViewModel.MainMenu
{
    public class IntroCardsViewModel
    {
        private readonly IEventAggregator _eventAggregator;

        public IntroCardsViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }
        
        public void Finish()
        {
            _eventAggregator.Publish(GameTrigger.AssetsStartedLoading);
        }

        public void Skip()
        {
            _eventAggregator.Publish(GameTrigger.AssetsStartedLoading);
        }
    }
}
