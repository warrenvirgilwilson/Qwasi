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
    public class WPFCheckbox : CheckBox
    {
        static WPFCheckbox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFCombobox), new FrameworkPropertyMetadata(typeof(WPFCombobox)));
        }

        public WPFCheckbox()
        {
            this.FontSize = 11;
            this.Background = Brushes.White;
            this.BorderBrush = Brushes.Gray;
            this.HorizontalContentAlignment = HorizontalAlignment.Right;
            this.VerticalContentAlignment = VerticalAlignment.Bottom;
            this.IsThreeState = false;
            this.IsEnabled = true;
            this.IsChecked = false;
        }
    }
}
