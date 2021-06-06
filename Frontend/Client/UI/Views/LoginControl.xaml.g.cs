// -----------------------------------------------------------
//  
//  This file was generated, please do not modify.
//  
// -----------------------------------------------------------
namespace EmptyKeys.UserInterface.Generated {
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
    public partial class LoginControl : UserControl {
        
        private Grid e_0;
        
        private TextBlock e_1;
        
        private TextBlock e_2;
        
        private TextBox e_3;
        
        private PasswordBox e_4;
        
        public LoginControl() {
            Style style = UserControlStyle.CreateUserControlStyle();
            style.TargetType = this.GetType();
            this.Style = style;
            this.InitializeComponent();
        }
        
        private void InitializeComponent() {
            // e_0 element
            this.e_0 = new Grid();
            this.Content = this.e_0;
            this.e_0.Name = "e_0";
            RowDefinition row_e_0_0 = new RowDefinition();
            this.e_0.RowDefinitions.Add(row_e_0_0);
            RowDefinition row_e_0_1 = new RowDefinition();
            this.e_0.RowDefinitions.Add(row_e_0_1);
            ColumnDefinition col_e_0_0 = new ColumnDefinition();
            this.e_0.ColumnDefinitions.Add(col_e_0_0);
            ColumnDefinition col_e_0_1 = new ColumnDefinition();
            this.e_0.ColumnDefinitions.Add(col_e_0_1);
            // e_1 element
            this.e_1 = new TextBlock();
            this.e_0.Children.Add(this.e_1);
            this.e_1.Name = "e_1";
            this.e_1.Text = "Username";
            this.e_1.FontFamily = new FontFamily("Segoe UI");
            Grid.SetColumn(this.e_1, 0);
            Grid.SetRow(this.e_1, 0);
            // e_2 element
            this.e_2 = new TextBlock();
            this.e_0.Children.Add(this.e_2);
            this.e_2.Name = "e_2";
            this.e_2.Text = "Username";
            this.e_2.FontFamily = new FontFamily("Segoe UI");
            Grid.SetColumn(this.e_2, 0);
            Grid.SetRow(this.e_2, 1);
            // e_3 element
            this.e_3 = new TextBox();
            this.e_0.Children.Add(this.e_3);
            this.e_3.Name = "e_3";
            this.e_3.MinWidth = 200F;
            this.e_3.HorizontalAlignment = HorizontalAlignment.Left;
            this.e_3.VerticalAlignment = VerticalAlignment.Bottom;
            Grid.SetColumn(this.e_3, 1);
            Grid.SetRow(this.e_3, 0);
            // e_4 element
            this.e_4 = new PasswordBox();
            this.e_0.Children.Add(this.e_4);
            this.e_4.Name = "e_4";
            this.e_4.MinWidth = 200F;
            this.e_4.HorizontalAlignment = HorizontalAlignment.Left;
            this.e_4.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(this.e_4, 1);
            Grid.SetRow(this.e_4, 1);
        }
    }
}
