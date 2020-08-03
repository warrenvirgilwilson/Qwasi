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
    [System.ComponentModel.DefaultEvent("SelectionChanged")]
    [System.Windows.Markup.ContentProperty("Items")]
    public class WPFLabeledCombobox : WPFLabeledControl
    {
        static WPFLabeledCombobox()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFLabeledCombobox), new FrameworkPropertyMetadata(typeof(WPFLabeledCombobox)));
        }

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(WPFLabeledCombobox));

        public event RoutedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }

        private void RaiseSelectionChangedEvent() => OnSelectionChanged(new RoutedEventArgs(WPFLabeledCombobox.SelectionChangedEvent));
        protected virtual void OnSelectionChanged(RoutedEventArgs e) => this.RaiseEvent(e);

        public WPFCombobox ComboboxControl { get; } = new WPFCombobox();

        protected override FrameworkElement BaseControl2 => this.ComboboxControl;

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(object), typeof(WPFLabeledCombobox), new PropertyMetadata(null));

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public ItemCollection Items => this.ComboboxControl.Items;

        public WPFLabeledCombobox()
            : base()
        {
            this.PartitionMetric = GridUnitType.Star;
            this.PartitionAlignment = PartitionAlignment.Left;
            this.PartitionLocation = 50;
            this.ComboboxControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.ComboboxControl.SelectionChanged += (o, e) => this.RaiseSelectionChangedEvent();

            Binding binding = new Binding("SelectedIndex");
            binding.Source = this;
            this.ComboboxControl.SetBinding(WPFCombobox.SelectedIndexProperty, binding);
        }
    }
}
