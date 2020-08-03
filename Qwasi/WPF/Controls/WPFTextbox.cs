using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public delegate bool WPFTextboxValuePredicate<T>(T value);

    public abstract class WPFTextbox<T> : TextBox
    {
        public static readonly DependencyProperty ParsedValueProperty =
            DependencyProperty.Register("ParsedValue", typeof(T), typeof(WPFTextbox<T>), new PropertyMetadata(null));

        public T ParsedValue
        {
            get { return (T)GetValue(ParsedValueProperty); }
            private set { SetValue(ParsedValueProperty, value); }
        }

        public event EventHandler<WPFValueBasedTextboxEventArgs<T>> ParsedValueChanged;
        protected void OnParsedValueChanged(WPFValueBasedTextboxEventArgs<T> e) => this.ParsedValueChanged?.Invoke(this, e);
        private void RaiseParsedValueChangedEvent(WPFValueBasedTextboxEventArgs<T> e) => this.OnParsedValueChanged(e);

        public bool ValidatePreviewText { get; set; } = true;
        public bool ParseOnTextChange { get; set; } = true;
        public string ParseErrorText { get; set; } = "Text input is invalid.";

        private string _lastValidText = null;
        private List<(WPFTextboxValuePredicate<T> Predicate, string ErrorMessage)> _ValuePredicates
            = new List<(WPFTextboxValuePredicate<T> Predicate, string ErrorMessage)>();

        public void RegisterPredicate(WPFTextboxValuePredicate<T> predicate) => _ValuePredicates.Add((predicate, null));
        public void RegisterPredicate(WPFTextboxValuePredicate<T> predicate, string errorMessage) => _ValuePredicates.Add((predicate, errorMessage));

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            if (!this.ValidatePreviewText)
                return;

            string fullText = this.Text.Insert(this.CaretIndex, e.Text);
            T parsedValue;
            e.Handled = !__ParseAndValidate(fullText, out parsedValue, false);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                __processText(true);

            base.OnKeyDown(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            __processText(true);
            base.OnLostFocus(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (this.ParseOnTextChange || _lastValidText == null)
                __processText(false);

            base.OnTextChanged(e);
        }

        private void __processText(bool breakOnError)
        {
            if (this.Text == _lastValidText)
                return;

            T parsedValue;
            if (!__ParseAndValidate(this.Text, out parsedValue, breakOnError))
            {
                if (breakOnError)
                    this.Text = _lastValidText;
                return;
            }

            _lastValidText = this.Text;
            T oldValue = this.ParsedValue;
            if (object.Equals(parsedValue, oldValue))
                return;

            this.ParsedValue = parsedValue;
            this.RaiseParsedValueChangedEvent(new WPFValueBasedTextboxEventArgs<T>(parsedValue, oldValue));
        }

        protected abstract bool TryParseText(string text, out T parsedValue);
        private bool __ParseAndValidate(string text, out T parsedValue, bool showError)
        {
            if (!TryParseText(text, out parsedValue))
            {
                if (showError)
                    MessageBox.Show(this.ParseErrorText ?? "Invalid text input.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_ValuePredicates != null)
            {
                foreach (var item in _ValuePredicates)
                {
                    if (!item.Predicate(parsedValue))
                    {
                        if (showError)
                            MessageBox.Show(item.ErrorMessage ?? this.ParseErrorText ?? "Invalid text input.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
            }

            return true;
        }

        public WPFTextbox()
        {
            this.ParsedValue = default(T);

            this.FontSize = 11;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.TextAlignment = TextAlignment.Left;
            this.VerticalContentAlignment = VerticalAlignment.Center;
            //this.BorderBrush = Brushes.Transparent;
            this.BorderThickness = new Thickness(1);
            this.BorderBrush = Brushes.LightGray;
        }
    }

    public class WPFValueBasedTextboxEventArgs<T> : EventArgs
    {
        T NewValue { get; }
        T OldValue { get; }

        public WPFValueBasedTextboxEventArgs(T newValue, T oldValue)
        {
            this.NewValue = newValue;
            this.OldValue = oldValue;
        }
    }

    public class WPFTextbox1 : WPFTextbox<string>
    {
        protected override bool TryParseText(string text, out string parsedValue)
        {
            parsedValue = text;
            return true;
        }
    }

    public class WPFIntValuedTextbox : WPFTextbox<int>
    {
        public bool PositiveValuesOnly { get; set; } = true;

        protected override bool TryParseText(string text, out int parsedValue)
        {
            if (!int.TryParse(text, out parsedValue))
                return false;

            return this.PositiveValuesOnly ? parsedValue >= 0 : true;
        }
    }

    public class WPFDoubleValuedTextbox : WPFTextbox<double>
    {
        protected override bool TryParseText(string text, out double parsedValue)
        {
            return double.TryParse(text, out parsedValue);
        }

        public WPFDoubleValuedTextbox()
            : base()
        {
            this.ValidatePreviewText = false;
            this.ParseOnTextChange = true;
        }
    }
}
