using System;
using BinaryVibrance.MLEM.Binding;
using Microsoft.Xna.Framework;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Slate.Client.ViewModel.MainMenu;
using Slate.Client.ViewModel.Services;

namespace Slate.Client.UI.Views
{
	public class CharacterListView : IViewFactory<CharacterListViewModel>
	{
		public Widget CreateView(CharacterListViewModel viewModel)
        {
            return new ReloadablePanel(p => RebuildView(p, viewModel));
        }

        private Widget ChildTemplate(CharacterListViewModel viewModel, GameCharacter gameCharacter)
        {
            return
                    new RadioButton
                    {
                        VerticalAlignment = VerticalAlignment.Center
                    }
                    .Bind(viewModel).SelectedCharacter().ToRadioButton(gameCharacter)
                    .Bind(gameCharacter).Name().ToRadioButtonText()
                    ;
        }

		private void RebuildView(Panel panel, CharacterListViewModel viewModel)
        {
            panel.HorizontalAlignment = HorizontalAlignment.Stretch;
            panel.BorderThickness = new Thickness(1);
            var characterListWindow = new Window("static-panel")
            {
                Width = 200,
                Margin = new Thickness(16),
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = new VerticalStackPanel { }
                    .Bind(viewModel).Characters().ToChildTemplate(character => ChildTemplate(viewModel, character))
            };
            characterListWindow.CloseButton.Visible = false;
            panel.AddChildren(
                new Grid
                    {
                        ColumnsProportions = { new(ProportionType.Auto), new( ProportionType.Fill) },
                    }
                    .AddChildren(
                        characterListWindow,
                        new TextButton
                            {
                                GridColumn = 1,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Text = "Select Character",
                                Margin = new Thickness(16)
                            }
                            .Bind(viewModel).PlayAsCharacterCommand().ToPressedEvent()
                    )
                );
		}
	}
}
