using System;
using BinaryVibrance.MLEM.Binding;
using Myra.Graphics2D;
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
            panel.AddChildren(
                new Grid
                    {
                        ColumnsProportions = { new(), new() },
                        ShowGridLines = true
                    }
                    .AddChildren(
                        new Panel
                        {
                            //FIXME: 9patch the background
                            //Background = new NinePatchRegion()
                            Padding = new Thickness(8)
                        }
                            .AddChild(
                                new VerticalStackPanel
                                {

                                }
                                .Bind(viewModel).Characters().ToChildTemplate(character => ChildTemplate(viewModel, character))
                            ),

                        new TextButton
                            {
                                GridColumn = 1,
                                HorizontalAlignment = HorizontalAlignment.Right,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Text = "Select Character"
                            }
                            .Bind(viewModel).PlayAsCharacterCommand().ToPressedEvent()
                    )
                );
		}
	}
}
