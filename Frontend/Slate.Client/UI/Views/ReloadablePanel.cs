using System;
using Microsoft.Xna.Framework;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace Slate.Client.UI.Views
{
	public class ReloadablePanel : Panel
	{
		private bool _constructed;

		public ReloadablePanel(Anchor anchor, Vector2 size, Vector2 positionOffset,
			bool setHeightBasedOnChildren = false, bool scrollOverflow = false, Point? scrollerSize = null,
			bool autoHideScrollbar = true) : base(anchor, size, positionOffset, setHeightBasedOnChildren,
			scrollOverflow, scrollerSize, autoHideScrollbar)
		{
		}

		public Action<Panel>? Build { get; init; }

		public override void Update(GameTime time)
		{
			if (!_constructed) Rebuild();
			base.Update(time);
		}

		public void Rebuild()
		{
			RemoveChildren(e => true);
			Build?.Invoke(this);
			_constructed = true;
		}
	}
}