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
    public class WPFLabel : Label
    {
        static WPFLabel()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFLabel), new FrameworkPropertyMetadata(typeof(WPFLabel)));
        }

        public WPFLabel()
        {
            this.FontSize = 11;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;
            this.Padding = new Thickness(2, 1, 1, 1);
        }
    }
}
