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
    public partial class LoginScreen : UserControl {
        
        private Grid e_0;
        
        private TextBlock e_1;
        
        private TextBlock e_2;
        
        private TextBox e_3;
        
        private PasswordBox e_4;
        
        private Button e_5;
        
        public LoginScreen() {
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
            RowDefinition row_e_0_0 = new RowDefinition();
            this.e_0.RowDefinitions.Add(row_e_0_0);
            RowDefinition row_e_0_1 = new RowDefinition();
            row_e_0_1.Height = new GridLength(1F, GridUnitType.Auto);
            this.e_0.RowDefinitions.Add(row_e_0_1);
            RowDefinition row_e_0_2 = new RowDefinition();
            row_e_0_2.Height = new GridLength(1F, GridUnitType.Auto);
            this.e_0.RowDefinitions.Add(row_e_0_2);
            RowDefinition row_e_0_3 = new RowDefinition();
            row_e_0_3.Height = new GridLength(1F, GridUnitType.Auto);
            this.e_0.RowDefinitions.Add(row_e_0_3);
            RowDefinition row_e_0_4 = new RowDefinition();
            this.e_0.RowDefinitions.Add(row_e_0_4);
            ColumnDefinition col_e_0_0 = new ColumnDefinition();
            col_e_0_0.Width = new GridLength(1F, GridUnitType.Star);
            this.e_0.ColumnDefinitions.Add(col_e_0_0);
            ColumnDefinition col_e_0_1 = new ColumnDefinition();
            col_e_0_1.Width = new GridLength(1F, GridUnitType.Auto);
            this.e_0.ColumnDefinitions.Add(col_e_0_1);
            ColumnDefinition col_e_0_2 = new ColumnDefinition();
            col_e_0_2.Width = new GridLength(1F, GridUnitType.Auto);
            this.e_0.ColumnDefinitions.Add(col_e_0_2);
            ColumnDefinition col_e_0_3 = new ColumnDefinition();
            col_e_0_3.Width = new GridLength(1F, GridUnitType.Star);
            this.e_0.ColumnDefinitions.Add(col_e_0_3);
            // e_1 element
            this.e_1 = new TextBlock();
            this.e_0.Children.Add(this.e_1);
            this.e_1.Name = "e_1";
            this.e_1.Margin = new Thickness(8F, 8F, 8F, 8F);
            this.e_1.HorizontalAlignment = HorizontalAlignment.Right;
            this.e_1.VerticalAlignment = VerticalAlignment.Bottom;
            this.e_1.Text = "Username";
            this.e_1.FontFamily = new FontFamily("Segoe UI");
            Grid.SetColumn(this.e_1, 1);
            Grid.SetRow(this.e_1, 1);
            // e_2 element
            this.e_2 = new TextBlock();
            this.e_0.Children.Add(this.e_2);
            this.e_2.Name = "e_2";
            this.e_2.Margin = new Thickness(8F, 8F, 8F, 8F);
            this.e_2.HorizontalAlignment = HorizontalAlignment.Right;
            this.e_2.VerticalAlignment = VerticalAlignment.Top;
            this.e_2.Text = "Password";
            this.e_2.FontFamily = new FontFamily("Segoe UI");
            Grid.SetColumn(this.e_2, 1);
            Grid.SetRow(this.e_2, 2);
            // e_3 element
            this.e_3 = new TextBox();
            this.e_0.Children.Add(this.e_3);
            this.e_3.Name = "e_3";
            this.e_3.MinWidth = 200F;
            this.e_3.Margin = new Thickness(8F, 8F, 8F, 8F);
            this.e_3.HorizontalAlignment = HorizontalAlignment.Left;
            this.e_3.VerticalAlignment = VerticalAlignment.Bottom;
            Grid.SetColumn(this.e_3, 2);
            Grid.SetRow(this.e_3, 1);
            Binding binding_e_3_Text = new Binding("Username");
            this.e_3.SetBinding(TextBox.TextProperty, binding_e_3_Text);
            // e_4 element
            this.e_4 = new PasswordBox();
            this.e_0.Children.Add(this.e_4);
            this.e_4.Name = "e_4";
            this.e_4.MinWidth = 200F;
            this.e_4.Margin = new Thickness(8F, 8F, 8F, 8F);
            this.e_4.HorizontalAlignment = HorizontalAlignment.Left;
            this.e_4.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(this.e_4, 2);
            Grid.SetRow(this.e_4, 2);
            Binding binding_e_4_Text = new Binding("Password");
            this.e_4.SetBinding(PasswordBox.TextProperty, binding_e_4_Text);
            // e_5 element
            this.e_5 = new Button();
            this.e_0.Children.Add(this.e_5);
            this.e_5.Name = "e_5";
            this.e_5.MinWidth = 120F;
            this.e_5.HorizontalAlignment = HorizontalAlignment.Center;
            this.e_5.VerticalAlignment = VerticalAlignment.Top;
            this.e_5.Padding = new Thickness(8F, 8F, 8F, 8F);
            this.e_5.Content = "Log in";
            Grid.SetColumn(this.e_5, 1);
            Grid.SetRow(this.e_5, 3);
            Grid.SetColumnSpan(this.e_5, 2);
            Binding binding_e_5_Command = new Binding("LoginCommand");
            this.e_5.SetBinding(Button.CommandProperty, binding_e_5_Command);
        }
        
        private static void InitializeElementResources(UIElement elem) {
            // Resource - [System.Windows.Controls.TextBlock] Style
            Style r_0_s = new Style(typeof(TextBlock));
            Setter r_0_s_S_0 = new Setter(TextBlock.FontFamilyProperty, new FontFamily("Segoe UI"));
            r_0_s.Setters.Add(r_0_s_S_0);
            elem.Resources.Add(typeof(TextBlock), r_0_s);
        }
    }
}
