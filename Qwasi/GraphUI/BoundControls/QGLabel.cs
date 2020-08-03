using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Qwasi.GraphUI
{
    public abstract class QGLabel : QGControl
    {
        public sealed override IQGLayer BindingLayer => null;

        protected Rectangle WPFRectangleFrame { get; private set; } = new Rectangle();
        protected abstract FrameworkElement WPFInnerControl { get; }

        public Thickness Margin
        {
            get => this.WPFInnerControl.Margin;
            set => this.WPFInnerControl.Margin = value;
        }

        private bool _LabelPositionFixed = false;
        public bool LabelPositionFixed
        {
            get { return _LabelPositionFixed; }
            set
            {
                _LabelPositionFixed = value;
                this.Refresh();
            }
        }

        private Vector _FixedLabelDisplacement = new Vector(15, -5);
        public Vector FixedLabelDisplacement
        {
            get { return _FixedLabelDisplacement; }
            set
            {
                if (value == _FixedLabelDisplacement)
                    return;

                _FixedLabelDisplacement = value;
                this.Refresh();
            }
        }

        private Visibility _Visibility = Visibility.Visible;
        public Visibility Visibility
        {
            get => _Visibility;
            set
            {
                _Visibility = value;
                foreach (UIElement element in this.WPFElements)
                    element.Visibility = _Visibility;

                if (_Visibility == Visibility.Visible)
                    this.Refresh();
            }
        }

        protected virtual bool UseDynamicCoordinates => !this.LabelPositionFixed && this.ParentLayer != null && this.IsInitialized;

        protected virtual Point GetFixedLabelCoordinates()
        {
            return (Point)this.FixedLabelDisplacement;
        }

        protected virtual Point GetDynamicLabelCoordinates()
        {
            return this.GetFixedLabelCoordinates();
        }

        public virtual void Refresh()
        {
            if (_Visibility != Visibility.Visible)
                return;

            this.RefreshDimensions();
            Point labelPosition;
            if (this.UseDynamicCoordinates)
                labelPosition = this.GetDynamicLabelCoordinates();
            else
                labelPosition = this.GetFixedLabelCoordinates();

            if (labelPosition == this.Position)
                return;

            this.SetPosition(labelPosition);
            this.RefreshDimensions();
        }

        protected override void OnInitialized(QGControlEventArgs e)
        {
            base.OnInitialized(e);
            this.Refresh();
        }

        protected virtual void RefreshDimensions()
        {
            this.WPFInnerControl.Width = this.WPFInnerControl.Height = double.NaN;
            this.WPFInnerControl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size newSize = this.WPFInnerControl.DesiredSize;
            this.WPFInnerControl.Width = this.WPFRectangleFrame.Width = this.WPFInnerControl.DesiredSize.Width;
            this.WPFInnerControl.Height = this.WPFRectangleFrame.Height = this.WPFInnerControl.DesiredSize.Height;
            this.ParentLayer?.UpdateControlSize(this);
        }

        public QGLabel()
        {
            this.RegisterWPFElement(this.WPFRectangleFrame);
            this.RegisterWPFElement(this.WPFInnerControl);
            this.SetWPFPrimaryElement(this.WPFRectangleFrame);

            this.HorizontalAlignment = QGHorizontalAlignment.Center;
            this.VerticalAlignment = QGVerticalAlignment.Center;

            this.WPFRectangleFrame.RadiusX = 2;
            this.WPFRectangleFrame.RadiusY = 2;
            this.WPFRectangleFrame.Fill = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255));
            this.WPFRectangleFrame.Stroke = new SolidColorBrush(Color.FromArgb(48, 0, 0, 0));
            this.WPFRectangleFrame.StrokeThickness = 1;
        }
    }
}
