using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class WPFPushpinButton : Label
    {
        static WPFPushpinButton()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFPushpinLabel), new FrameworkPropertyMetadata(typeof(WPFPushpinLabel)));
        }

        public bool _IsPinned = false;
        public bool IsPinned
        {
            get => _IsPinned;
            set
            {
                if (value == _IsPinned)
                    return;

                _IsPinned = value;
                if (_IsPinned == true)
                    this.RaisePinnedEvent();
                else
                    this.RaiseUnpinnedEvent();
            }
        }

        public event EventHandler<WPFPushpinEventArgs> Pinned;
        protected virtual void OnPinned(WPFPushpinEventArgs e) => this.Pinned?.Invoke(this, e);
        private void RaisePinnedEvent()
        {
            this.Style = this.PinnedStyle;
            this.OnPinned(new WPFPushpinEventArgs(this.IsPinned));
            this.Refresh();
        }

        public event EventHandler<WPFPushpinEventArgs> Unpinned;
        protected virtual void OnUnpinned(WPFPushpinEventArgs e) => this.Unpinned?.Invoke(this, e);
        private void RaiseUnpinnedEvent()
        {
            this.Style = this.UnpinnedStyle;
            this.OnUnpinned(new WPFPushpinEventArgs(this.IsPinned));
            this.Refresh();
        }

        public Brush MouseOverHighlightBrush { get; set; }

        protected Style PinnedStyle { get; }
        protected Style UnpinnedStyle { get; }

        public static readonly DependencyProperty Brush1Property =
            DependencyProperty.Register("Brush1", typeof(Brush), typeof(WPFPushpinButton), new PropertyMetadata(null));

        public Brush Brush1
        {
            get { return (Brush)GetValue(Brush1Property); }
            set { SetValue(Brush1Property, value); }
        }

        public static readonly DependencyProperty Brush2Property =
            DependencyProperty.Register("Brush2", typeof(Brush), typeof(WPFPushpinButton), new PropertyMetadata(null));

        public Brush Brush2
        {
            get { return (Brush)GetValue(Brush2Property); }
            set { SetValue(Brush2Property, value); }
        }

        public static readonly DependencyProperty Brush3Property =
            DependencyProperty.Register("Brush3", typeof(Brush), typeof(WPFPushpinButton), new PropertyMetadata(null));

        public Brush Brush3
        {
            get { return (Brush)GetValue(Brush3Property); }
            set { SetValue(Brush3Property, value); }
        }

        public static readonly DependencyProperty PinColorBrushProperty =
            DependencyProperty.Register("PinColorBrush", typeof(SolidColorBrush), typeof(WPFPushpinButton), new PropertyMetadata(null));

        public SolidColorBrush PinColorBrush
        {
            get { return (SolidColorBrush)GetValue(PinColorBrushProperty); }
            set { SetValue(PinColorBrushProperty, value); }
        }

        public Color __getBrushColor(Color mainColor, double multiplier)
        {
            byte R = (byte)(int)(0xFFd - ((0xFF - mainColor.R) * multiplier));
            byte G = (byte)(int)(0xFFd - ((0xFF - mainColor.G) * multiplier));
            byte B = (byte)(int)(0xFFd - ((0xFF - mainColor.B) * multiplier));
            return Color.FromRgb(R, G, B);
        }

        protected void Refresh()
        {
            Rect r = new Rect(0, 0, this.Width, this.Height);
            this.Arrange(r);
        }

        public WPFPushpinButton()
        {
            this.PinnedStyle = this.FindResource("PushpinPinned") as Style;
            this.UnpinnedStyle = this.FindResource("PushpinUnpinned") as Style;
            this.Style = this.UnpinnedStyle;

            Binding binding = new Binding("Foreground");
            binding.Source = this;
            binding.Mode = BindingMode.TwoWay;
            this.SetBinding(PinColorBrushProperty, binding);

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(PinColorBrushProperty, typeof(WPFPushpinButton));
            dpd.AddValueChanged(this, __PinColorBrushChanged);

            this.Foreground = Brushes.Black;
            this.MouseOverHighlightBrush = Brushes.DarkCyan;
        }

        private void __PinColorBrushChanged(object o, EventArgs e)
        {
            this.Brush1 = this.PinColorBrush;
            this.Brush2 = new SolidColorBrush(__getBrushColor(((SolidColorBrush)this.PinColorBrush).Color, 0.0476d));
            this.Brush3 = new SolidColorBrush(__getBrushColor(((SolidColorBrush)this.PinColorBrush).Color, 0.0847d));
        }

        private Brush _NormalForeground = null;
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            _NormalForeground = this.Foreground;
            this.Foreground = this.MouseOverHighlightBrush;
            this.Refresh();

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.Foreground = _NormalForeground;
            this.Refresh();

            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            this.IsPinned = !this.IsPinned;

            base.OnMouseDown(e);
        }
    }

    public class WPFPushpinEventArgs : EventArgs
    {
        public bool IsPinned { get; }

        public WPFPushpinEventArgs(bool isPinned)
        {
            this.IsPinned = isPinned;
        }
    }
}
