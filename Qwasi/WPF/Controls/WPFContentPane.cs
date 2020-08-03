using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Qwasi.WPF.Controls
{
    [System.ComponentModel.DefaultEvent("Loaded")]
    [System.Windows.Markup.ContentProperty("Items")]
    public class WPFContentPane : Grid
    {
        // Using a DependencyProperty as the backing store for TitleText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleTextProperty =
            DependencyProperty.Register("TitleText", typeof(string), typeof(WPFContentPane), new PropertyMetadata(null));

        public string TitleText
        {
            get { return (string)GetValue(TitleTextProperty); }
            set { SetValue(TitleTextProperty, value); }
        }

        protected TextBlock TitleBlock { get; private set; } = new TextBlock();

        protected Border TitleCell { get; } = new Border();
        protected Border ContentCell { get; } = new Border();
        protected StackPanel ContentStackPanel { get; } = new StackPanel();
        public UIElementCollection Items => this.ContentStackPanel.Children;

        public void Inactivate()
        {
            this.Opacity = 0.5;

            foreach (UIElement item in this.Items)
                item.IsEnabled = false;
        }

        public void Activate()
        {
            this.Opacity = 1;

            foreach (UIElement item in this.Items)
                item.IsEnabled = true;
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
        }

        public WPFContentPane()
            : base()
        {
            this.Margin = new Thickness(1, 1, 1, 5);

            RowDefinition row1 = new RowDefinition();
            RowDefinition row2 = new RowDefinition();
            row1.Height = new GridLength(22);
            this.RowDefinitions.Add(row1);
            this.RowDefinitions.Add(row2);

            this.TitleCell.BorderThickness = new Thickness(0);
            this.TitleCell.Background = Brushes.SteelBlue;
            this.TitleCell.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.TitleCell.VerticalAlignment = VerticalAlignment.Stretch;
            Grid.SetRow(this.TitleCell, 0);
            this.Children.Add(this.TitleCell);

            this.TitleBlock.FontSize = 12;
            this.TitleBlock.Foreground = Brushes.White;
            this.TitleBlock.HorizontalAlignment = HorizontalAlignment.Center;
            this.TitleBlock.VerticalAlignment = VerticalAlignment.Center;
            this.TitleCell.Child = this.TitleBlock;

            Binding titleBinding = new Binding("TitleText");
            titleBinding.Source = this;
            this.TitleBlock.SetBinding(TextBlock.TextProperty, titleBinding);

            this.ContentCell.BorderThickness = new Thickness(0);
            this.ContentCell.Margin = new Thickness(0, 1, 0, 0);
            //this.ContentCell.BorderBrush = Brushes.Transparent;
            this.ContentCell.Background = Brushes.WhiteSmoke;
            this.ContentCell.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.ContentCell.Padding = new Thickness(1, 1, 1, 1);
            Grid.SetRow(this.ContentCell, 1);
            this.Children.Add(this.ContentCell);

            this.ContentStackPanel = new StackPanel();
            this.ContentCell.Child = this.ContentStackPanel;
        }
    }
}
