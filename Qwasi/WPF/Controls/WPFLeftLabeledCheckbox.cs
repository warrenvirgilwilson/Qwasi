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
    [System.ComponentModel.DefaultEvent("Checked")]
    [System.ComponentModel.DefaultProperty("IsChecked")]
    public class WPFLeftLabeledCheckbox : WPFLabeledControl
    {
        static WPFLeftLabeledCheckbox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFLeftLabeledCheckbox), new FrameworkPropertyMetadata(typeof(WPFLeftLabeledCheckbox)));
        }

        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent(
            "Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WPFLeftLabeledCheckbox));

        public event RoutedEventHandler Checked
        {
            add { AddHandler(CheckedEvent, value); }
            remove { RemoveHandler(CheckedEvent, value); }
        }

        private void RaiseCheckedEvent() => OnChecked(new RoutedEventArgs(WPFLeftLabeledCheckbox.CheckedEvent));
        protected virtual void OnChecked(RoutedEventArgs e) => this.RaiseEvent(e);

        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent(
            "Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WPFLeftLabeledCheckbox));

        public event RoutedEventHandler Unchecked
        {
            add { AddHandler(UncheckedEvent, value); }
            remove { RemoveHandler(UncheckedEvent, value); }
        }

        private void RaiseUncheckedEvent() => OnUnchecked(new RoutedEventArgs(WPFLeftLabeledCheckbox.UncheckedEvent));
        protected virtual void OnUnchecked(RoutedEventArgs e) => this.RaiseEvent(e);

        public WPFCheckbox CheckboxControl { get; } = new WPFCheckbox();
        protected override FrameworkElement BaseControl2 => this.CheckboxControl;

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(WPFLeftLabeledCheckbox), new PropertyMetadata(null));

        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public WPFLeftLabeledCheckbox()
            : base()
        {
            this.CheckboxControl.Content = null;
            this.IsChecked = false;
            this.CheckboxControl.Checked += (o, e) => this.RaiseCheckedEvent();
            this.CheckboxControl.Unchecked += (o, e) => this.RaiseUncheckedEvent();

            this.Margin = new Thickness(0, 1, 0, 0);

            Binding binding = new Binding("IsChecked");
            binding.Source = this;
            this.CheckboxControl.SetBinding(WPFCheckbox.IsCheckedProperty, binding);
        }
    }
}
