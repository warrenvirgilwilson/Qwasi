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
    public enum PartitionAlignment { Left, Right }

    public abstract class WPFControlPairBase : Grid
    {
        static WPFControlPairBase()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFControlPair), new FrameworkPropertyMetadata(typeof(WPFControlPair)));
        }    

        protected Grid PartitionGrid { get; } = new Grid();
        protected abstract FrameworkElement BaseControl1 { get; }
        protected abstract FrameworkElement BaseControl2 { get; }

        protected ColumnDefinition Column1 { get; private set; } = new ColumnDefinition();
        protected ColumnDefinition Column2 { get; private set; } = new ColumnDefinition();

        private double _PartitionLocation = 50;
        public double PartitionLocation
        {
            get { return _PartitionLocation; }
            set
            {
                _PartitionLocation = value;
                __UpdatePartition();
            }
        }

        public GridUnitType _PartitionMetric = GridUnitType.Star;
        public GridUnitType PartitionMetric
        {
            get { return _PartitionMetric; }
            set
            {
                _PartitionMetric = value;
                __UpdatePartition();
            }
        }

        private PartitionAlignment _PartitionAlignment = PartitionAlignment.Left;
        public PartitionAlignment PartitionAlignment
        {
            get { return _PartitionAlignment; }
            set
            {
                _PartitionAlignment = value;
                __UpdatePartition();
            }
        }

        private void __UpdatePartition()
        {
            if (this.PartitionMetric == GridUnitType.Star)
            {
                if (this.PartitionAlignment == PartitionAlignment.Left)
                {
                    this.Column1.Width = new GridLength(this.PartitionLocation, this.PartitionMetric);
                    this.Column2.Width = new GridLength(100 - this.PartitionLocation, this.PartitionMetric);
                }
                else if (this.PartitionAlignment == PartitionAlignment.Right)
                {
                    this.Column1.Width = new GridLength(100 - this.PartitionLocation, this.PartitionMetric);
                    this.Column2.Width = new GridLength(this.PartitionLocation, this.PartitionMetric);
                }
            }
            else if (this.PartitionMetric == GridUnitType.Pixel)
            {
                if (this.PartitionAlignment == PartitionAlignment.Left)
                {
                    this.Column1.Width = new GridLength(this.PartitionLocation, GridUnitType.Pixel);
                    this.Column2.Width = new GridLength(1, GridUnitType.Star);
                }
                else if (this.PartitionAlignment == PartitionAlignment.Right)
                {
                    this.Column1.Width = new GridLength(1, GridUnitType.Star);
                    this.Column2.Width = new GridLength(this.PartitionLocation, GridUnitType.Pixel);
                }
            }
            else if (this.PartitionMetric == GridUnitType.Auto)
            {
                if (this.PartitionAlignment == PartitionAlignment.Left)
                {
                    this.Column1.Width = new GridLength(0, GridUnitType.Auto);
                    this.Column2.Width = new GridLength(1, GridUnitType.Star);
                }
                else if (this.PartitionAlignment == PartitionAlignment.Right)
                {
                    this.Column1.Width = new GridLength(1, GridUnitType.Star);
                    this.Column2.Width = new GridLength(0, GridUnitType.Auto);
                }
            }
        }

        private UIElement _prevControl1 = null;
        private UIElement _prevControl2 = null;
        protected void ArrangeControls()
        {
            if (this.BaseControl1 != _prevControl1)
            {
                if (_prevControl1 != null)
                    this.PartitionGrid.Children.Remove(_prevControl1);

                this.PartitionGrid.Children.Add(this.BaseControl1);
                Grid.SetColumn(this.BaseControl1, 0);
                _prevControl1 = this.BaseControl1;
            }

            if (this.BaseControl2 != _prevControl2)
            {
                if (_prevControl2 != null)
                    this.PartitionGrid.Children.Remove(_prevControl2);

                this.PartitionGrid.Children.Add(this.BaseControl2);
                Grid.SetColumn(this.BaseControl2, 1);
                _prevControl2 = this.BaseControl2;
            }
        }

        public WPFControlPairBase()
            : base()
        {
            this.PartitionLocation = 50;
            this.PartitionMetric = GridUnitType.Star;
            this.PartitionGrid.ColumnDefinitions.Add(this.Column1);
            this.PartitionGrid.ColumnDefinitions.Add(this.Column2);
            this.Children.Add(this.PartitionGrid);

            this.ArrangeControls();
        }
    }

    [System.Windows.Markup.ContentProperty("Content")]
    public abstract class WPFContentControlPair : WPFControlPairBase
    {
        public static readonly DependencyProperty ContentProperty =
               DependencyProperty.Register("Content", typeof(object), typeof(WPFContentControlPair), new PropertyMetadata(null));

        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        protected abstract ContentControl ContentControl { get; }

        public WPFContentControlPair()
            : base()
        {
            Binding contentBinding = new Binding("Content");
            contentBinding.Source = this;
            this.ContentControl.SetBinding(ContentControl.ContentProperty, contentBinding);
        }
    }

    public class WPFControlPair<TControl1, TControl2> : WPFControlPairBase
        where TControl1 : FrameworkElement
        where TControl2 : FrameworkElement
    {
        static WPFControlPair()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFControlPair), new FrameworkPropertyMetadata(typeof(WPFControlPair)));
        }

        protected override FrameworkElement BaseControl1 => this.Control1;
        protected override FrameworkElement BaseControl2 => this.Control2;

        private TControl1 _Control1 = null;
        public TControl1 Control1
        {
            get { return _Control1; }
            set { _Control1 = value; this.ArrangeControls(); }
        }

        private TControl1 _Control2 = null;
        public TControl1 Control2
        {
            get { return _Control2; }
            set { _Control2 = value; this.ArrangeControls(); }
        }

        public WPFControlPair()
            : base()
        {
        }
    }
}
