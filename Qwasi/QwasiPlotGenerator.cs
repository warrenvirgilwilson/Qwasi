using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using Qwasi.GraphUI;
using Qwasi.HilbertSpaceMath;
using Qwasi.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace Qwasi
{
    public class QwasiPlotGenerator
    {
        public Chart GeneratedChartControl { get; private set; } = new CartesianChart();

        protected Axis XAxis { get; } = new Axis();
        protected Axis YAxis { get; } = new Axis();

        protected LineSeries LineSeriesES1 { get; private set; }
        protected LineSeries LineSeriesES2 { get; private set; }
        protected LineSeries LineSeriesES1Square { get; private set; }
        protected LineSeries LineSeriesES2Square { get; private set; }
        protected LineSeries LineSeriesSumSquare { get; private set; }

        public QwasiAnalyticsData LoadedAnalyticsData { get; private set; } = null;
        public QGEdge LoadedEdge { get; private set; } = null;

        public bool AnimateOnLoadingData { get; set; } = false;

        public int MaxSampleCount { get; set; } = -1;
        public int DesiredXLabelCount { get; set; } = 10;
        public int YLabelCount { get; set; } = 5;
        public int YLabelNumDigits { get; set; } = 2;
        public double YRangeMinValue { get; set; } = -1.0d;
        public double YRangeMaxValue { get; set; } = 1.0d;

        public bool IncludeEdgeState1 { get; set; } = true;
        public bool IncludeEdgeState2 { get; set; } = true;
        public bool IncludeEdgeState1Square { get; set; } = true;
        public bool IncludeEdgeState2Square { get; set; } = true;
        public bool IncludeSumSquare { get; set; } = true;

        public Color EdgeState1Color { get; set; } = (Color)ColorConverter.ConvertFromString("#FF2195F2");
        public Color EdgeState2Color { get; set; } = (Color)ColorConverter.ConvertFromString("#FFF34336");
        public Color EdgeState1SquareColor { get; set; } = (Color)ColorConverter.ConvertFromString("#FF2195F2");
        public Color EdgeState2SquareColor { get; set; } = (Color)ColorConverter.ConvertFromString("#FFF34336");
        public Color SumSquareColor { get; set; } = Colors.Green;

        public event EventHandler<QwasiPlotEventArgs> PlotUpdated;
        protected virtual void OnPlotUpdated(QwasiPlotEventArgs e) => this.PlotUpdated?.Invoke(this, e);
        private void RaisePlotUpdatedEvent() => this.OnPlotUpdated(new QwasiPlotEventArgs());

        private bool _hasRunAnimation = false;
        public void LoadData(QwasiAnalyticsData analyticsData, QGEdge edge)
        {
            _hasRunAnimation = !this.AnimateOnLoadingData;
            this.GeneratedChartControl.DisableAnimations = !this.AnimateOnLoadingData;
            this.LoadedAnalyticsData = analyticsData;
            this.LoadedEdge = edge;
        }

        private IEnumerable<(double Coef, int StepNumber)> __getGraphValues(HSEdgeState basisState)
        {
            if (this.LoadedAnalyticsData.StateEvaluationCount <= this.MaxSampleCount || this.MaxSampleCount < 0)
                return this.LoadedAnalyticsData.StateEvaluationList.Select(se => (se.GetCoefficient(basisState), se.StepNumber));

            return __downsampleData(basisState, this.MaxSampleCount);
        }

        private IEnumerable<(double Coef, int StepNumber)> __downsampleData(HSEdgeState basisState, int sampleCount)
        {
            double dataPointsPerSample = (double)this.LoadedAnalyticsData.StateEvaluationCount / (double)sampleCount;

            int j = 0;
            for (int i = 1; i <= sampleCount; i++)
            {
                int samplePoint = (int)Math.Round(((double)i) * dataPointsPerSample);

                double pointMax = 0;
                int firstStepValue = this.LoadedAnalyticsData.StateEvaluationList[j].StepNumber;
                for (; j < samplePoint; j++)
                {
                    QwasiEvaluationState es = this.LoadedAnalyticsData.StateEvaluationList[j];
                    pointMax = Math.MaxMagnitude(pointMax, es.GetCoefficient(basisState));
                }
                int lastStepValue = this.LoadedAnalyticsData.StateEvaluationList[j - 1].StepNumber;

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

        protected void __initializeChart()
        {
            //this.XAxis.Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = true };
            this.GeneratedChartControl.AxisX.Add(this.XAxis);

            this.XAxis.FontFamily = new FontFamily("Cambria Math");
            this.YAxis.FontFamily = new FontFamily("Cambria Math");

            this.YAxis.MinValue = -1;
            this.YAxis.MaxValue = 1;
            this.YAxis.Separator = new LiveCharts.Wpf.Separator { Step = 0.5, IsEnabled = true };
            this.GeneratedChartControl.AxisY.Add(this.YAxis);

            this.GeneratedChartControl.Hoverable = false;
            this.GeneratedChartControl.IsHitTestVisible = false;
            this.GeneratedChartControl.Background = Brushes.White;
            this.GeneratedChartControl.LegendLocation = LegendLocation.Top;
            this.GeneratedChartControl.ChartLegend.Margin = new Thickness(0, 0, 0, 4);
            this.XAxis.Loaded += (o, e) => this.UpdatePlot();
            this.YAxis.Loaded += (o, e) => this.UpdatePlot();
        }

        public void UpdatePlot()
        {
            if (this.LoadedAnalyticsData == null || this.LoadedEdge == null)
                return;

            if (!this.XAxis.IsLoaded || !this.YAxis.IsLoaded)
                return;

            if (_hasRunAnimation)
                this.GeneratedChartControl.DisableAnimations = true;
            else
                _hasRunAnimation = true;

            this.YAxis.MaxValue = this.YRangeMaxValue;
            this.YAxis.MinValue = this.YRangeMinValue;

            double yStep = (this.YRangeMaxValue - this.YRangeMinValue) / (this.YLabelCount - 1);
            this.YAxis.Separator = new LiveCharts.Wpf.Separator { Step = Math.Round(yStep, 15, MidpointRounding.ToZero), IsEnabled = true };
            this.YAxis.LabelFormatter = v => Math.Round(v, this.YLabelNumDigits, MidpointRounding.AwayFromZero).ToString();

            HSEdgeState es1 = new HSEdgeState(this.LoadedEdge.Vertex1, this.LoadedEdge.Vertex2);
            HSEdgeState es2 = new HSEdgeState(this.LoadedEdge.Vertex2, this.LoadedEdge.Vertex1);

            var es1values = __getGraphValues(es1).ToArray();
            var es2values = __getGraphValues(es2).ToArray();

            double xStep = (int)Math.Round((double)(es1values.Length - 1) / (double)(this.DesiredXLabelCount - 1), MidpointRounding.AwayFromZero);
            xStep = xStep < 1.0d ? 1.0d : xStep;
            this.XAxis.Separator = new LiveCharts.Wpf.Separator { Step = xStep, IsEnabled = false };
            this.XAxis.Labels = new List<string>(es1values.Select(v => v.StepNumber.ToString()));
            //List<string> list = new List<string>(es1values.Select(v => v.StepNumber.ToString()));

            this.GeneratedChartControl.Series.Clear();
            this.GeneratedChartControl.FontFamily = new FontFamily("Cambria Math");
            this.GeneratedChartControl.FontWeight = FontWeights.Normal;

            if (this.IncludeEdgeState1)
            {
                this.LineSeriesES1 = new LineSeries
                {
                    Title = "C " + es1.ToString(),
                    Values = new ChartValues<double>(es1values.Select(v => v.Coef)),
                    PointGeometry = null,
                    AreaLimit = 0,
                    LineSmoothness = 0.1,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(this.EdgeState1Color),
                    Fill = new SolidColorBrush(this.EdgeState1Color) { Opacity = 0.1 }
                };
                this.LineSeriesES1.Loaded += __seriesLoaded;
                this.GeneratedChartControl.Series.Add(this.LineSeriesES1);
            }

            if (this.IncludeEdgeState2)
            {
                this.LineSeriesES2 = new LineSeries
                {
                    Title = "D " + es2.ToString(),
                    Values = new ChartValues<double>(es2values.Select(v => v.Coef)),
                    PointGeometry = null,
                    AreaLimit = 0,
                    LineSmoothness = 0.1,
                    StrokeThickness = 1,
                    Stroke = new SolidColorBrush(this.EdgeState2Color),
                    Fill = new SolidColorBrush(this.EdgeState2Color) { Opacity = 0.1 }
                };
                this.LineSeriesES2.Loaded += __seriesLoaded;
                this.GeneratedChartControl.Series.Add(this.LineSeriesES2);
            }

            if (this.IncludeEdgeState1Square)
            {
                this.LineSeriesES1Square = new LineSeries
                {
                    Title = "[C²]",
                    Values = new ChartValues<double>(es1values.Select(v => v.Coef * v.Coef)),
                    PointGeometry = null,
                    AreaLimit = 0,
                    LineSmoothness = 0.1,
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(this.EdgeState1SquareColor),
                    StrokeDashArray = new DoubleCollection { 2, 2 },
                    Fill = Brushes.Transparent
                };
                this.LineSeriesES1Square.Loaded += __seriesLoaded;
                this.GeneratedChartControl.Series.Add(this.LineSeriesES1Square);
            }

            if (this.IncludeEdgeState2Square)
            {
                this.LineSeriesES2Square = new LineSeries
                {
                    Values = new ChartValues<double>(es2values.Select(v => v.Coef * v.Coef)),
                    Title = "[D²]",
                    PointGeometry = null,
                    AreaLimit = 0,
                    LineSmoothness = 0.1,
                    StrokeThickness = 2,
                    Stroke = new SolidColorBrush(this.EdgeState2SquareColor),
                    StrokeDashArray = new DoubleCollection { 2, 2 },
                    Fill = Brushes.Transparent
                };
                this.LineSeriesES2Square.Loaded += __seriesLoaded;
                this.GeneratedChartControl.Series.Add(this.LineSeriesES2Square);
            }

            if (this.IncludeSumSquare)
            {
                List<(double SumSquare, int StepNumber)> sumSquareValues = new List<(double SumSquare, int StepNumber)>(es1values.Length);
                for (int i = 0; i < es1values.Length; i++)
                    sumSquareValues.Add(((es1values[i].Coef * es1values[i].Coef) + (es2values[i].Coef * es2values[i].Coef), es1values[i].StepNumber));

                this.LineSeriesSumSquare = new LineSeries
                {
                    Values = new ChartValues<double>(sumSquareValues.Select(v => v.SumSquare)),
                    Title = "[C² + D²]",
                    PointGeometry = null,
                    AreaLimit = 0,
                    LineSmoothness = 0.1,
                    StrokeThickness = 3,
                    Stroke = new SolidColorBrush(this.SumSquareColor),
                    StrokeDashArray = new DoubleCollection { 3, 1 },
                    Fill = Brushes.Transparent
                };
                this.LineSeriesSumSquare.Loaded += __seriesLoaded;
                this.GeneratedChartControl.Series.Add(this.LineSeriesSumSquare);
            }

            if (this.GeneratedChartControl.Series.Count == 0)
                this.RaisePlotUpdatedEvent();
        }

        private void __seriesLoaded(object sender, RoutedEventArgs e)
        {
            foreach (Series series in this.GeneratedChartControl.Series)
                if (!series.IsLoaded)
                    return;

            this.RaisePlotUpdatedEvent();
        }

        public QwasiPlotGenerator()
        {
            __initializeChart();
        }
    }

    public class QwasiPlotEventArgs : EventArgs
    {
    }
}
