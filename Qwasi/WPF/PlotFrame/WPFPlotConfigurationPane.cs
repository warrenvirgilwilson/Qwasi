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

namespace Qwasi.WPF
{
    public class WPFPlotConfigurationPane : StackPanel
    {
        protected QwasiPlotGenerator PlotGenerator { get; } = new QwasiPlotGenerator();
        public Viewbox SizedPlotControl { get; private set; } = new Viewbox();

        public QwasiAnalyticsData LoadedAnalyticsData { get; private set; } = null;
        public QGEdge LoadedEdge { get; private set; } = null;

        public void LoadData(QwasiAnalyticsData analyticsData, QGEdge edge)
        {
            this.LoadedAnalyticsData = analyticsData;
            this.LoadedEdge = edge;

            this.PlotGenerator.LoadData(analyticsData, edge);

            __updateChart();
            __updatePlotContentPaneLabels();
        }

        private void __updateChart()
        {
            if (this.LoadedAnalyticsData == null || this.LoadedEdge == null)
                return;

            this.PlotGenerator.DesiredXLabelCount = this.DesiredXLabelCountTB.ParsedValue;
            this.PlotGenerator.YLabelCount = this.YLabelCountTB.ParsedValue;
            this.PlotGenerator.YLabelNumDigits = this.YLabelNumDigitsTB.ParsedValue;
            this.PlotGenerator.YRangeMinValue = this.YRangeMinTB.ParsedValue;
            this.PlotGenerator.YRangeMaxValue = this.YRangeMaxTB.ParsedValue;

            this.PlotGenerator.IncludeEdgeState1 = this.EdgeState1Checkbox.IsChecked ?? false;
            this.PlotGenerator.IncludeEdgeState2 = this.EdgeState2Checkbox.IsChecked ?? false;
            this.PlotGenerator.IncludeEdgeState1Square = this.EdgeState1SquareCheckbox.IsChecked ?? false;
            this.PlotGenerator.IncludeEdgeState2Square = this.EdgeState2SquareCheckbox.IsChecked ?? false;
            this.PlotGenerator.IncludeSumSquare = this.SumSquareCheckbox.IsChecked ?? false;

            this.PlotGenerator.EdgeState1Color = this.EdgeState1ColorPicker.SelectedColor ?? Brushes.Transparent.Color;
            this.PlotGenerator.EdgeState2Color = this.EdgeState2ColorPicker.SelectedColor ?? Brushes.Transparent.Color;
            this.PlotGenerator.EdgeState1SquareColor = this.EdgeState1SquareColorPicker.SelectedColor ?? Brushes.Transparent.Color;
            this.PlotGenerator.EdgeState2SquareColor = this.EdgeState2SquareColorPicker.SelectedColor ?? Brushes.Transparent.Color;
            this.PlotGenerator.SumSquareColor = this.SumSquareColorPicker.SelectedColor ?? Brushes.Transparent.Color;

            this.PlotGenerator.UpdatePlot();

            this.PlotGenerator.GeneratedChartControl.Width = this.BaseChartWidth;
            this.PlotGenerator.GeneratedChartControl.Height = this.BaseChartHeight;
            this.SizedPlotControl.Width = this.ChartWidth;
            this.SizedPlotControl.Height = this.ChartHeight;
        }

        protected double BaseChartWidth => this.ChartWidth / this.ChartScaleValue;
        protected double BaseChartHeight => this.ChartHeight / this.ChartScaleValue;

        public double ChartWidth => (double)this.ChartWidthTB.ParsedValue / QwasiGraphics.GetSystemScaleRatios(this).XScale;
        public double ChartHeight => (double)this.ChartHeightTB.ParsedValue / QwasiGraphics.GetSystemScaleRatios(this).YScale;
        public double ChartScaleValue => this.ChartContentScaleTB.ParsedValue;

        protected WPFContentPane PlotContentPane { get; } = new WPFContentPane();
        protected WPFCheckbox EdgeState1Checkbox { get; } = new WPFCheckbox();
        protected WPFCheckbox EdgeState2Checkbox { get; } = new WPFCheckbox();
        protected WPFCheckbox EdgeState1SquareCheckbox { get; } = new WPFCheckbox();
        protected WPFCheckbox EdgeState2SquareCheckbox { get; } = new WPFCheckbox();
        protected WPFCheckbox SumSquareCheckbox { get; } = new WPFCheckbox();

        protected ColorPicker EdgeState1ColorPicker { get; } = new ColorPicker();
        protected ColorPicker EdgeState2ColorPicker { get; } = new ColorPicker();
        protected ColorPicker EdgeState1SquareColorPicker { get; } = new ColorPicker();
        protected ColorPicker EdgeState2SquareColorPicker { get; } = new ColorPicker();
        protected ColorPicker SumSquareColorPicker { get; } = new ColorPicker();

        private void __updatePlotContentPaneLabels()
        {
            if (this.LoadedEdge == null)
                return;

            HSEdgeState es1 = new HSEdgeState(this.LoadedEdge.Vertex1, this.LoadedEdge.Vertex2);
            HSEdgeState es2 = new HSEdgeState(this.LoadedEdge.Vertex2, this.LoadedEdge.Vertex1);

            this.EdgeState1Checkbox.Content = "Edge State 1 " + es1.ToString() + " ";
            this.EdgeState2Checkbox.Content = "Edge State 2 " + es2.ToString() + " ";
            this.EdgeState1SquareCheckbox.Content = "Edge State 1 Squared ";
            this.EdgeState2SquareCheckbox.Content = "Edge State 2 Squared ";
            this.SumSquareCheckbox.Content = "Sum of Squared States ";
        }

        private void __FormatCheckboxAndColorPicker(WPFCheckbox checkbox, ColorPicker colorPicker, Color? initialColor,
            Grid grid, int gridRow, int gridColumn)
        {
            checkbox.IsChecked = true;
            checkbox.HorizontalContentAlignment = HorizontalAlignment.Left;
            checkbox.VerticalAlignment = VerticalAlignment.Center;
            //checkbox.Margin = new Thickness(1, 1, 1, 2);
            Grid.SetRow(checkbox, gridRow);
            Grid.SetColumn(checkbox, gridColumn);
            grid.Children.Add(checkbox);

            colorPicker.SelectedColor = initialColor;
            Grid.SetRow(colorPicker, gridRow);
            Grid.SetColumn(colorPicker, gridColumn + 1);
            grid.Children.Add(colorPicker);

            checkbox.Checked += (o, e) => { colorPicker.IsEnabled = true; __updateChart(); };
            checkbox.Unchecked += (o, e) => { colorPicker.IsEnabled = false; __updateChart(); };
            colorPicker.SelectedColorChanged += (o, e) => __updateChart();
        }

        private void __FormatTextblock(WPFTextblock textblock, string text, Grid grid, int gridRow, int gridColumn)
        {
            textblock.Text = text;
            textblock.HorizontalAlignment = HorizontalAlignment.Stretch;
            textblock.Margin = new Thickness(1, 1, 1, 2);
            Grid.SetRow(textblock, gridRow);
            Grid.SetColumn(textblock, gridColumn);
            grid.Children.Add(textblock);
        }

        private void __FormatTextbox<T>(WPFTextbox<T> textbox, string textValue, Grid grid, int gridRow, int gridColumn)
        {
            textbox.Text = textValue;
            textbox.HorizontalAlignment = HorizontalAlignment.Stretch;
            textbox.Margin = new Thickness(0, 0, 0, 1);
            textbox.ParsedValueChanged += (o, e) => __updateChart();
            Grid.SetRow(textbox, gridRow);
            Grid.SetColumn(textbox, gridColumn);
            grid.Children.Add(textbox);
        }

        private void __initializePlotContentPane()
        {
            this.PlotContentPane.TitleText = "Content";
            //this.TitleCell.Background = Brushes.Gray;
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.25, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            this.PlotContentPane.Items.Add(grid);

            __FormatCheckboxAndColorPicker(this.EdgeState1Checkbox, this.EdgeState1ColorPicker, this.PlotGenerator.EdgeState1Color, grid, 0, 0);
            __FormatCheckboxAndColorPicker(this.EdgeState2Checkbox, this.EdgeState2ColorPicker, this.PlotGenerator.EdgeState2Color, grid, 0, 3);
            __FormatCheckboxAndColorPicker(this.EdgeState1SquareCheckbox, this.EdgeState1SquareColorPicker, this.PlotGenerator.EdgeState1SquareColor, grid, 1, 0);
            __FormatCheckboxAndColorPicker(this.EdgeState2SquareCheckbox, this.EdgeState2SquareColorPicker, this.PlotGenerator.EdgeState2SquareColor, grid, 1, 3);
            __FormatCheckboxAndColorPicker(this.SumSquareCheckbox, this.SumSquareColorPicker, this.PlotGenerator.SumSquareColor, grid, 2, 0);
        }

        protected WPFContentPane DimensionsPane { get; } = new WPFContentPane();
        protected WPFIntValuedTextbox ChartWidthTB { get; } = new WPFIntValuedTextbox();
        protected WPFIntValuedTextbox ChartHeightTB { get; } = new WPFIntValuedTextbox();
        protected WPFDoubleValuedTextbox ChartContentScaleTB { get; } = new WPFDoubleValuedTextbox();
        protected WPFIntValuedTextbox DesiredXLabelCountTB { get; } = new WPFIntValuedTextbox();
        protected WPFIntValuedTextbox YLabelCountTB { get; } = new WPFIntValuedTextbox();
        protected WPFIntValuedTextbox YLabelNumDigitsTB { get; } = new WPFIntValuedTextbox();
        protected WPFDoubleValuedTextbox YRangeMinTB { get; } = new WPFDoubleValuedTextbox();
        protected WPFDoubleValuedTextbox YRangeMaxTB { get; } = new WPFDoubleValuedTextbox();

        private void __initializeDimensionsPane()
        {
            this.DimensionsPane.TitleText = "Dimensions";
            //this.TitleCell.Background = Brushes.Gray;
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            this.DimensionsPane.Items.Add(grid);

            __FormatTextblock(new WPFTextblock(), "Plot Width (Pixels): ", grid, 0, 0);
            __FormatTextbox(this.ChartWidthTB, "1366", grid, 0, 1);

            __FormatTextblock(new WPFTextblock(), "Plot Height (Pixels): ", grid, 0, 2);
            __FormatTextbox(this.ChartHeightTB, "768", grid, 0, 3);

            __FormatTextblock(new WPFTextblock(), "Content Scale Factor: ", grid, 0, 4);
            __FormatTextbox(this.ChartContentScaleTB, "1.0", grid, 0, 5);
            this.ChartContentScaleTB.RegisterPredicate(v => v > 0d, "Value must be a number greater than zero.");

            __FormatTextblock(new WPFTextblock(), "Approx. # X Labels: ", grid, 1, 0);
            __FormatTextbox(this.DesiredXLabelCountTB, "20", grid, 1, 1);
            DesiredXLabelCountTB.ValidatePreviewText = false;
            DesiredXLabelCountTB.RegisterPredicate(v => v >= 2, "Value must be 2 or greater.");

            __FormatTextblock(new WPFTextblock(), "# Y Labels: ", grid, 1, 2);
            __FormatTextbox(this.YLabelCountTB, "7", grid, 1, 3);
            YLabelCountTB.ValidatePreviewText = false;
            YLabelCountTB.RegisterPredicate(v => v >= 2, "Value must be 2 or greater.");

            __FormatTextblock(new WPFTextblock(), "# Y Label Digits: ", grid, 1, 4);
            __FormatTextbox(this.YLabelNumDigitsTB, "2", grid, 1, 5);
            YLabelNumDigitsTB.ValidatePreviewText = false;
            YLabelNumDigitsTB.RegisterPredicate(v => v >= 0 && v <= 15, "Value must be between 0 and 15.");

            __FormatTextblock(new WPFTextblock(), "Y Range Min Value: ", grid, 2, 0);
            __FormatTextbox(this.YRangeMinTB, "-1.0", grid, 2, 1);
            this.YRangeMinTB.RegisterPredicate(v => v >= -1.0d && v < 1.0d, "Value must be greater than or equal to -1.0 and less 1.0.");
            this.YRangeMinTB.RegisterPredicate(v => v < this.PlotGenerator.YRangeMaxValue, "Value must be less than the maximum Y range value.");

            __FormatTextblock(new WPFTextblock(), "Y Range Max Value: ", grid, 2, 2);
            __FormatTextbox(this.YRangeMaxTB, "1.0", grid, 2, 3);
            this.YRangeMaxTB.RegisterPredicate(v => v > -1.0d && v <= 1.0d, "Value must be greater than -1.0 and less than or equal to 1.0.");
            this.YRangeMaxTB.RegisterPredicate(v => v > this.PlotGenerator.YRangeMinValue, "Value must be greater than the minimum Y range value.");
        }

        public BitmapSource GenerateBitmapImage()
        {
            QwasiScaleValues systemScaleValues = QwasiGraphics.GetSystemScaleRatios(this.SizedPlotControl);

            RenderTargetBitmap rtb = new RenderTargetBitmap(
                (int)Math.Floor(this.SizedPlotControl.ActualWidth * systemScaleValues.XScale),
                (int)Math.Floor(this.SizedPlotControl.ActualHeight * systemScaleValues.YScale),
                96d * systemScaleValues.XScale, 96d * systemScaleValues.YScale,
                PixelFormats.Pbgra32);
            rtb.Render(this.SizedPlotControl);

            return rtb;
        }

        public WPFPlotConfigurationPane()
        {
            this.PlotGenerator.MaxSampleCount = -1;
            this.PlotGenerator.AnimateOnLoadingData = true;

            this.SizedPlotControl.Child = this.PlotGenerator.GeneratedChartControl;
            this.SizedPlotControl.Stretch = Stretch.Fill;

            __initializePlotContentPane();
            this.Children.Add(this.PlotContentPane);

            __initializeDimensionsPane();
            this.Children.Add(this.DimensionsPane);
        }
    }
}
