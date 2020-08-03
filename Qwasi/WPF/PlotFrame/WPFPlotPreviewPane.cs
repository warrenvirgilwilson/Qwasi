using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Qwasi.WPF.PlotFrame
{
    public class WPFPlotPreviewPane : Canvas
    {
        public double CanvasAspectRatio => this.ActualWidth / this.ActualHeight;
        public double PreviewedControlAspectRatio => this.PreviewedControl.ActualWidth / this.PreviewedControl.ActualHeight;
        public double PreviewScaleRatio => this.PreviewViewbox.ActualWidth / this.PreviewedControl.ActualWidth;

        protected Viewbox PreviewViewbox { get; } = new Viewbox();
        protected Label PercentageLabel { get; } = new Label();

        private FrameworkElement _PreviewedControl = null;
        public FrameworkElement PreviewedControl
        {
            get => _PreviewedControl;
            set
            {
                if (_PreviewedControl != null)
                {
                    _PreviewedControl.SizeChanged -= __PreviewedControl_SizeChanged;
                }

                _PreviewedControl = value;

                _PreviewedControl.SizeChanged += __PreviewedControl_SizeChanged;
                this.PreviewViewbox.Child = _PreviewedControl;
                __updatePresentation();
            }
        }

        private void __PreviewedControl_SizeChanged(object sender, SizeChangedEventArgs e) => __updatePresentation();
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            __updatePresentation();
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void __updatePresentation()
        {
            if (this.PreviewedControl == null)
                return;

            if (this.PreviewedControl.ActualWidth > this.ActualWidth || this.PreviewedControl.ActualHeight > this.ActualHeight)
            {
                if (this.PreviewedControlAspectRatio >= this.CanvasAspectRatio)
                {
                    this.PreviewViewbox.Width = this.ActualWidth;
                    this.PreviewViewbox.Height = (1.0d / this.PreviewedControlAspectRatio) * this.ActualWidth;
                }
                else
                {
                    this.PreviewViewbox.Width = this.PreviewedControlAspectRatio * this.ActualHeight;
                    this.PreviewViewbox.Height = this.ActualHeight;
                }
            }
            else
            {
                this.PreviewViewbox.Width = this.PreviewedControl.ActualWidth;
                this.PreviewViewbox.Height = this.PreviewedControl.ActualHeight;
            }

            this.PreviewViewbox.UpdateLayout();
            Canvas.SetLeft(this.PreviewViewbox, (this.ActualWidth - this.PreviewViewbox.ActualWidth) / 2.0d);
            Canvas.SetTop(this.PreviewViewbox, (this.ActualHeight - this.PreviewViewbox.ActualHeight) / 2.0d);

            this.PercentageLabel.Content = ((int)Math.Round(this.PreviewScaleRatio * 100d)).ToString() + "%";
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            __updatePresentation();
        }

        public WPFPlotPreviewPane()
        {
            this.Background = Brushes.Black;
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.ClipToBounds = true;

            this.Children.Add(this.PreviewViewbox);

            this.PercentageLabel.Background = Brushes.White.Clone();
            this.PercentageLabel.Background.Opacity = 0.9;
            this.PercentageLabel.BorderBrush = Brushes.DarkGray;
            this.PercentageLabel.BorderThickness = new Thickness(1, 0, 0, 1);
            this.PercentageLabel.Content = "100%";

            Canvas.SetRight(this.PercentageLabel, 0);
            Canvas.SetTop(this.PercentageLabel, 0);
            this.Children.Add(this.PercentageLabel);
        }
    }
}
