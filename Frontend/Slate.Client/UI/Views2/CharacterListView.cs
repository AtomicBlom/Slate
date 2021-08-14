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
            return new Grid
                {
                    RowsProportions = { new(), new() }
                }
                .AddChildren(
                    new RadioButton
                    {
                        GridRowSpan = 2,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                    .Bind(viewModel).SelectedCharacter().ToRadioButton(gameCharacter)
                    ,
                    new Label
                    {
                        GridColumn = 1,
                        Text = "Character name goes here"
                    }
                    .Bind(gameCharacter).Name().ToLabel()
                    ,
                    new Label
                    {
                        GridColumn = 1,
                        GridRow = 1,
                        Text = "Character ID goes here"
                    }
                    .Bind(gameCharacter).Id(new ToStringConverter<Guid>()).ToLabel()
                );
        }

		private void RebuildView(Panel panel, CharacterListViewModel viewModel)
        {
            panel.AddChildren(
                new Grid
                    {
                        ColumnsProportions = { new(), new() },
                    }
                    .AddChildren(
                        new Panel
                        {
                            //FIXME: 9patch the background
                            //Background = new NinePatchRegion()
                            Padding = new Thickness(8)
                        }
                            .AddChild(
                                new VerticalStackPanel()
                                {

                                }
                                .Bind(viewModel).Characters().ToChildTemplate(character => ChildTemplate(viewModel, character))
                            ),

                        new TextButton { Text = "Select Character" }
                            .Bind(viewModel).PlayAsCharacterCommand().ToPressedEvent()
                    )
                );
		}
	}
}
