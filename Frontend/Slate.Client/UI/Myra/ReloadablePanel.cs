using System;
using Myra.Graphics2D.UI;

namespace Slate.Client.UI.Views
{
    internal class ReloadablePanel : Panel
    {
        private readonly Action<Panel> _build;

        public ReloadablePanel(Action<Panel> build)
        {
            _build = build;
            build(this);
        }

        public void Rebuild()
        {
            while (ChildrenCount > 0)
            {
                RemoveChild(GetChild(0));
            }

            _build(this);
        }
    }
}