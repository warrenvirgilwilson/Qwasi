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
    public class WPFButton : Button
    {
        static WPFButton()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFPanelButton), new FrameworkPropertyMetadata(typeof(WPFPanelButton)));
        }

        public WPFButton()
        {
            this.FontSize = 11;
            this.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xDF, 0xDA, 0xE2));
            this.BorderThickness = new Thickness(1);
            this.BorderBrush = Brushes.Transparent;
        }
    }
}
