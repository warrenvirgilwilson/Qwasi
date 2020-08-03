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
    public class WPFCombobox : ComboBox
    {
        static WPFCombobox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFCombobox), new FrameworkPropertyMetadata(typeof(WPFCombobox)));
        }

        public WPFCombobox()
        {
            this.FontSize = 11;
            this.Background = Brushes.White;
            //this.BorderThickness = new Thickness(1);
            this.BorderBrush = Brushes.Gray;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Center;
        }
    }
}
