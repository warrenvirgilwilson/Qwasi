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
    [System.ComponentModel.DefaultEvent("TextChanged")]
    public abstract class WPFControlFieldPair<TTextbox, TValue> : WPFContentControlPair
        where TTextbox : WPFTextbox<TValue>, new()
    {
        static WPFControlFieldPair()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFControlFieldPair<TTextbox, TValue>), new FrameworkPropertyMetadata(typeof(WPFControlFieldPair<TTextbox, TValue>)));
        }

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
            "TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WPFControlFieldPair<TTextbox, TValue>));

        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        private void RaiseTextChangedEvent() => OnTextChanged();
        protected virtual void OnTextChanged() => this.RaiseEvent(new RoutedEventArgs(WPFControlFieldPair<TTextbox, TValue>.TextChangedEvent));

        public static readonly DependencyProperty FieldTextProperty =
            DependencyProperty.Register("FieldText", typeof(string), typeof(WPFControlFieldPair<TTextbox, TValue>), new PropertyMetadata(null));

        public string FieldText
        {
            get { return (string)GetValue(FieldTextProperty); }
            set { this.TextboxControl.Text = value; }
        }

        public static readonly DependencyProperty ParsedValueProperty =
            DependencyProperty.Register("ParsedValue", typeof(TValue), typeof(WPFControlFieldPair<TTextbox, TValue>), new PropertyMetadata(null));

        public TValue ParsedValue
        {
            get { return (TValue)GetValue(ParsedValueProperty); }
            private set { SetValue(ParsedValueProperty, value); }
        }

        public TTextbox TextboxControl { get; } = new TTextbox();
        protected sealed override FrameworkElement BaseControl2 => this.TextboxControl;

        public WPFControlFieldPair()
            : base()
        {
            Binding textBinding = new Binding("Text");
            textBinding.Source = this.TextboxControl;
            this.SetBinding(WPFControlFieldPair<TTextbox, TValue>.FieldTextProperty, textBinding);

            Binding valueBinding = new Binding("ParsedValue");
            valueBinding.Source = this.TextboxControl;
            this.SetBinding(WPFControlFieldPair<TTextbox, TValue>.ParsedValueProperty, valueBinding);

            this.TextboxControl.TextAlignment = TextAlignment.Left;
            this.TextboxControl.TextChanged += (o, e) => this.RaiseTextChangedEvent();
        }
    }
}
