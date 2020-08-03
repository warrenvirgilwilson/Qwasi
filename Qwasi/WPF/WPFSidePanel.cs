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

namespace Qwasi.WPF
{
    [System.ComponentModel.DefaultEvent("Loaded")]
    [System.Windows.Markup.ContentProperty("Children")]
    public class WPFSidePanel : Border
    {
        protected StackPanel MainStackPanel { get; } = new StackPanel();
        public UIElementCollection Children => this.MainStackPanel.Children;

        public WPFSidePanel()
            : base()
        {
            this.Background = new SolidColorBrush(Color.FromArgb(0x99, 0x7C, 0x7B, 0x95));
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.Padding = new Thickness(1, 1, 1, 1);
            this.BorderBrush = Brushes.Transparent;

            this.MainStackPanel = new StackPanel();
            this.Child = MainStackPanel;
        }
    }
}
