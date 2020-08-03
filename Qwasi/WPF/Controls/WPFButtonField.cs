using System;
using System.Collections.Generic;
using System.Linq;
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
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Qwasi.WPF.Controls
{
    [System.ComponentModel.DefaultEvent("Click")]
    public class WPFButtonField<TTextbox, TValue> : WPFControlFieldPair<TTextbox, TValue>
        where TTextbox : WPFTextbox<TValue>, new()
    {
        static WPFButtonField()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFButtonField<TTextbox, TValue>), new FrameworkPropertyMetadata(typeof(WPFButtonField<TTextbox, TValue>)));
        }

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WPFButtonField<TTextbox, TValue>));

        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        private void RaiseClickEvent() => OnClick(new RoutedEventArgs(WPFButtonField<TTextbox, TValue>.ClickEvent));
        protected virtual void OnClick(RoutedEventArgs e) => this.RaiseEvent(e);

        public WPFButton ButtonControl { get; } = new WPFButton();

        protected WPFLabel ButtonLabel { get; } = new WPFLabel();
        protected override FrameworkElement BaseControl1 => this.ButtonControl;
        protected override ContentControl ContentControl => this.ButtonLabel;

        public WPFButtonField()
            : base()
        {
            this.PartitionAlignment = PartitionAlignment.Right;
            this.PartitionLocation = 40;
            this.PartitionMetric = GridUnitType.Pixel;

            this.ButtonControl.Content = " ";
            this.ButtonControl.Click += (o, e) => this.RaiseClickEvent();

            this.ButtonLabel.HorizontalAlignment = HorizontalAlignment.Center;
            this.ButtonLabel.IsHitTestVisible = false;

            this.TextboxControl.TextAlignment = TextAlignment.Center;
            this.Children.Add(this.ButtonLabel);
        }
    }

    public class WPFButtonField : WPFButtonField<WPFTextbox1, string> { }
    public class WPFButtonIntField : WPFButtonField<WPFIntValuedTextbox, int> { }
}
