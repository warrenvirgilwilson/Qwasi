using LiveCharts;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;
using Qwasi.HilbertSpaceMath;
using Qwasi.WPF.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Qwasi.GraphUI
{
    public class QGEdgeAnalyticsLabel : QGEdgeLabel
    {
        protected WPFPushpinButton PushpinButton { get; } = new WPFPushpinButton();
        protected override bool IsLabelPinned
        {
            get => this.PushpinButton.IsPinned;
            set => this.PushpinButton.IsPinned = value;
        }

        protected QwasiPlotGenerator PlotGenerator { get; } = new QwasiPlotGenerator();

        protected Border ChartContainerBorder { get; } = new Border();
        protected Grid ChartContainer { get; } = new Grid();
        protected Border ChartFrame { get; } = new Border();
        protected Rectangle ChartOverlay { get; } = new Rectangle();
        protected Image RenderedChart { get; } = new Image();

        protected QwasiAnalyticsData AnalyticsData { get; private set; }
        public int MaxSampleCount { get; set; } = 250;
        public int MaxXLabelCount { get; set; } = 10;

        public event EventHandler<QGEdgeLabelEventArgs> ThumbnailPlotClicked;
        private void RaiseThumbnailPlotClickedEvent() => this.ThumbnailPlotClicked?.Invoke(this, new QGEdgeLabelEventArgs(this.ParentEdge));

        public void LoadAnalyticsData(QwasiAnalyticsData data)
        {
            this.AnalyticsData = data;
            this.PlotGenerator.LoadData(this.AnalyticsData, this.ParentEdge);
            __updateChart();
        }

        private IEnumerable<(double Coef, int StepNumber)> __getGraphValues(HSEdgeState basisState, int sampleCount)
        {
            if (this.AnalyticsData.StateEvaluationCount <= sampleCount)
                return this.AnalyticsData.StateEvaluationList.Select(se => (se.GetCoefficient(basisState), se.StepNumber));

            return __downsampleData(basisState, sampleCount);
        }

        private IEnumerable<(double Coef, int StepNumber)> __downsampleData(HSEdgeState basisState, int sampleCount)
        {
            double dataPointsPerSample = (double)this.AnalyticsData.StateEvaluationCount / (double)sampleCount;

            int j = 0;
            for (int i = 1; i <= sampleCount; i++)
            {
                int samplePoint = (int)Math.Round(((double)i) * dataPointsPerSample);

                double pointMax = 0;
                int firstStepValue = this.AnalyticsData.StateEvaluationList[j].StepNumber;
                for (; j < samplePoint; j++)
                {
                    QwasiEvaluationState es = this.AnalyticsData.StateEvaluationList[j];
                    pointMax = Math.MaxMagnitude(pointMax, es.GetCoefficient(basisState));
                }
                int lastStepValue = this.AnalyticsData.StateEvaluationList[j-1].StepNumber;

                int retStepValue;
                if (i == 1)
                    retStepValue = firstStepValue;
                else if (i == sampleCount)
                    retStepValue = lastStepValue;
                else
                    retStepValue = (int)Math.Floor(((double)firstStepValue + (double)lastStepValue) / 2d);

                yield return (pointMax, retStepValue);
            }
        }

        private void __updateChart()
        {
            if (this.AnalyticsData == null)
                return;

            this.PlotGenerator.DesiredXLabelCount = this.MaxXLabelCount;
            this.PlotGenerator.MaxSampleCount = this.MaxSampleCount;
            this.PlotGenerator.YLabelCount = 5;
            this.PlotGenerator.YLabelNumDigits = 2;
            this.PlotGenerator.YRangeMinValue = -1.0d;
            this.PlotGenerator.YRangeMaxValue = 1.0d;

            this.PlotGenerator.IncludeEdgeState1 = true;
            this.PlotGenerator.IncludeEdgeState2 = true;
            this.PlotGenerator.IncludeEdgeState1Square = false;
            this.PlotGenerator.IncludeEdgeState2Square = false;
            this.PlotGenerator.IncludeSumSquare = false;

            this.ChartFrame.Child = this.PlotGenerator.GeneratedChartControl;
            this.PlotGenerator.UpdatePlot();

            this.RefreshDimensions();
        }

        private void __PlotUpdated(object sender, QwasiPlotEventArgs e)
        {
            __renderToBitmap();
        }

        private void __renderToBitmap()
        {
            QwasiScaleValues systemScaleValues = QwasiGraphics.GetSystemScaleRatios(this.ChartFrame);

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)Math.Floor(this.PlotGenerator.GeneratedChartControl.ActualWidth * systemScaleValues.XScale),
                (int)Math.Floor(this.PlotGenerator.GeneratedChartControl.ActualHeight * systemScaleValues.YScale),
                96d * systemScaleValues.XScale, 96d * systemScaleValues.YScale,
                PixelFormats.Pbgra32);
            rtb.Render(this.PlotGenerator.GeneratedChartControl);

            this.RenderedChart.Source = rtb;
            this.RenderedChart.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            this.RenderedChart.Arrange(new Rect(new Point(0, 0),
                new Size(this.PlotGenerator.GeneratedChartControl.ActualWidth, this.PlotGenerator.GeneratedChartControl.ActualHeight)));
            this.RenderedChart.UpdateLayout();
            this.ChartFrame.Child = this.RenderedChart;
            this.ChartFrame.UpdateLayout();

            this.RefreshDimensions();
        }

        public QGEdgeAnalyticsLabel()
            : base()
        {
            this.HorizontalAlignment = QGHorizontalAlignment.Left;
            this.WPFGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            this.WPFGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            this.WPFGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            this.WPFGrid.Margin = new Thickness(4);

            this.RegisterHitTestElement(this.PushpinButton);
            this.PushpinButton.Width = 17;
            this.PushpinButton.Height = 17;
            this.PushpinButton.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.WPFGrid.Children.Add(this.PushpinButton);
            Grid.SetColumn(this.PushpinButton, 0);
            Grid.SetRow(this.PushpinButton, 0);

            this.ChartContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            this.ChartContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            this.ChartContainer.Background = Brushes.White;
            this.ChartContainerBorder.Child = this.ChartContainer;

            this.ChartContainerBorder.BorderThickness = new Thickness(1);
            this.ChartContainerBorder.BorderBrush = Brushes.DarkGray;
            this.WPFGrid.Children.Add(this.ChartContainerBorder);
            Grid.SetColumn(this.ChartContainerBorder, 1);
            Grid.SetRow(this.ChartContainerBorder, 0);
            this.RegisterHitTestElement(this.ChartContainerBorder);

            this.ChartFrame.Width = 250;
            this.ChartFrame.Height = 125;
            this.ChartFrame.Margin = new Thickness(4, 2, 4, 2);
            this.ChartContainer.Children.Add(this.ChartFrame);
            Grid.SetColumn(this.ChartFrame, 0);
            Grid.SetRow(this.ChartFrame, 0);

            TextBlock initialText = new TextBlock();
            initialText.Text = "Please compile analytics to view graphs.";
            initialText.Foreground = Brushes.LightGray;
            initialText.FontSize = 13;
            initialText.FontWeight = FontWeights.Bold;
            initialText.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            initialText.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.ChartFrame.Child = initialText;

            this.ChartOverlay.Fill = Brushes.DeepSkyBlue;
            this.ChartOverlay.Opacity = 0.0;
            this.ChartContainer.Children.Add(this.ChartOverlay);
            Grid.SetColumn(this.ChartOverlay, 0);
            Grid.SetRow(this.ChartOverlay, 0);
            this.ChartOverlay.MouseEnter += (o, e) => this.ChartOverlay.Opacity = 0.15;
            this.ChartOverlay.MouseLeave += (o, e) => this.ChartOverlay.Opacity = 0.0;
            this.ChartOverlay.MouseUp += (o, e) => this.RaiseThumbnailPlotClickedEvent();

            this.PlotGenerator.PlotUpdated += __PlotUpdated;

            this.WPFRectangleFrame.Fill = Brushes.LightYellow.Clone();
            this.WPFRectangleFrame.Fill.Opacity = 0.8;
        }
    }
}
