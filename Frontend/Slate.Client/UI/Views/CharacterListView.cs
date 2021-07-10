using System;
using Microsoft.Xna.Framework;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using Slate.Client.UI.ViewModels;

namespace Slate.Client.UI.Views
{
	public class CharacterListView
	{
		public static Element CreateView(CharacterListViewModel viewModel)
		{
			return new ReloadablePanel(Anchor.AutoLeft, Vector2.One, Vector2.Zero, true)
			{
				Build = p => RebuildView(p, viewModel),
				Texture = new StyleProp<NinePatch>(null)
			};
		}

		private static void RebuildView(Panel panel, CharacterListViewModel viewModel)
		{
			panel.AddChildren(
				new Panel(Anchor.AutoLeft, new Vector2(400, 1.0f), Vector2.Zero, scrollOverflow:true, autoHideScrollbar:false)
				{

				}.BindCollection(viewModel, vm => vm.Characters, (item) =>
				{
					return new RadioButton(Anchor.AutoLeft, new Vector2(1.0f, 50), string.Empty, group: "Characters")
					{
						OnPressed = e =>
						{
							viewModel.SelectedCharacter = item;
						}
					}.AddChildren(
						new Paragraph(Anchor.AutoLeft, 1.0f, string.Empty)
							.BindParagraph(item, character => character.Name),
						new Paragraph(Anchor.AutoLeft, 1.0f, string.Empty)
							.BindParagraph(item, character => character.Id, new ToStringConverter())
					);
				}),
				new Button(Anchor.BottomRight, new Vector2(200, 48), "Select Character")
					.BindPressed(viewModel.PlayAsCharacterCommand)
			);
		}
	}
}
