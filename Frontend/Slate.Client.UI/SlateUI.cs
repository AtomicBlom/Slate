using Serilog;
using Slate.Client.UI.Framework;

namespace Slate.Client.UI
{
    public class SlateUI
    {
        private readonly Dispatcher _dispatcher;

        public SlateUI(ILogger logger)
        {
            _dispatcher = new Dispatcher(logger);
        }



        public void DisplayElement(LayoutElement layoutElement)
        {

        }
    }
}
