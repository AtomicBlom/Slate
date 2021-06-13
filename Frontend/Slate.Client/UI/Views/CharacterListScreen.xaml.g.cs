// -----------------------------------------------------------
//  
//  This file was generated, please do not modify.
//  
// -----------------------------------------------------------
namespace Slate.Client.UI.Views {
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.ObjectModel;
    using EmptyKeys.UserInterface;
    using EmptyKeys.UserInterface.Charts;
    using EmptyKeys.UserInterface.Data;
    using EmptyKeys.UserInterface.Controls;
    using EmptyKeys.UserInterface.Controls.Primitives;
    using EmptyKeys.UserInterface.Input;
    using EmptyKeys.UserInterface.Interactions.Core;
    using EmptyKeys.UserInterface.Interactivity;
    using EmptyKeys.UserInterface.Media;
    using EmptyKeys.UserInterface.Media.Effects;
    using EmptyKeys.UserInterface.Media.Animation;
    using EmptyKeys.UserInterface.Media.Imaging;
    using EmptyKeys.UserInterface.Shapes;
    using EmptyKeys.UserInterface.Renderers;
    using EmptyKeys.UserInterface.Themes;
    
    
    [GeneratedCodeAttribute("Empty Keys UI Generator", "3.2.0.0")]
    public partial class CharacterListScreen : UserControl {
        
        private Grid e_0;
        
        private ItemsControl e_1;
        
        private Button e_5;
        
        public CharacterListScreen() {
            Style style = UserControlStyle.CreateUserControlStyle();
            style.TargetType = this.GetType();
            this.Style = style;
            this.InitializeComponent();
        }
        
        private void InitializeComponent() {
            InitializeElementResources(this);
            // e_0 element
            this.e_0 = new Grid();
            this.Content = this.e_0;
            this.e_0.Name = "e_0";
            ColumnDefinition col_e_0_0 = new ColumnDefinition();
            col_e_0_0.Width = new GridLength(400F, GridUnitType.Pixel);
            this.e_0.ColumnDefinitions.Add(col_e_0_0);
            ColumnDefinition col_e_0_1 = new ColumnDefinition();
            this.e_0.ColumnDefinitions.Add(col_e_0_1);
            // e_1 element
            this.e_1 = new ItemsControl();
            this.e_0.Children.Add(this.e_1);
            this.e_1.Name = "e_1";
            Func<UIElement, UIElement> e_1_dtFunc = e_1_dtMethod;
            this.e_1.ItemTemplate = new DataTemplate(e_1_dtFunc);
            Grid.SetColumn(this.e_1, 0);
            Binding binding_e_1_ItemsSource = new Binding("Characters");
            this.e_1.SetBinding(ItemsControl.ItemsSourceProperty, binding_e_1_ItemsSource);
            // e_5 element
            this.e_5 = new Button();
            this.e_0.Children.Add(this.e_5);
            this.e_5.Name = "e_5";
            this.e_5.Margin = new Thickness(16F, 16F, 16F, 16F);
            this.e_5.HorizontalAlignment = HorizontalAlignment.Right;
            this.e_5.VerticalAlignment = VerticalAlignment.Bottom;
            this.e_5.Padding = new Thickness(16F, 8F, 16F, 8F);
            this.e_5.Content = "Play";
            Grid.SetColumn(this.e_5, 1);
            Grid.SetRow(this.e_5, 0);
            Binding binding_e_5_Command = new Binding("PlayAsCharacterCommand");
            this.e_5.SetBinding(Button.CommandProperty, binding_e_5_Command);
        }
        
        private static void InitializeElementResources(UIElement elem) {
            // Resource - [ItemBackgroundBrush] SolidColorBrush
            elem.Resources.Add("ItemBackgroundBrush", new SolidColorBrush(new ColorW(0, 0, 0, 255)));
        }
        
        private static UIElement e_1_dtMethod(UIElement parent) {
            // e_2 element
            StackPanel e_2 = new StackPanel();
            e_2.Parent = parent;
            e_2.Name = "e_2";
            e_2.Margin = new Thickness(16F, 16F, 16F, 16F);
            e_2.HorizontalAlignment = HorizontalAlignment.Stretch;
            e_2.Background = new SolidColorBrush(new ColorW(0, 0, 0, 255));
            e_2.Background.Opacity = 0.5F;
            e_2.Orientation = Orientation.Vertical;
            // e_3 element
            TextBlock e_3 = new TextBlock();
            e_2.Children.Add(e_3);
            e_3.Name = "e_3";
            Binding binding_e_3_Text = new Binding("Name");
            e_3.SetBinding(TextBlock.TextProperty, binding_e_3_Text);
            // e_4 element
            TextBlock e_4 = new TextBlock();
            e_2.Children.Add(e_4);
            e_4.Name = "e_4";
            Binding binding_e_4_Text = new Binding("IdAsString");
            e_4.SetBinding(TextBlock.TextProperty, binding_e_4_Text);
            return e_2;
        }
    }
}
