using Qwasi.GraphUI;
using Qwasi.WPF.PlotFrame;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Qwasi.WPF
{
    public class WPFPlotFrame : Border
    {
        static WPFPlotFrame()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFPlotFrame), new FrameworkPropertyMetadata(typeof(WPFPlotFrame)));
        }

        protected WPFPlotPreviewPane PlotPreviewPane { get; private set; }
        protected WPFPlotConfigurationPane PlotConfigurationPane { get; private set; }
        protected WPFPlotActionsPane PlotActionsPane { get; private set; }

        protected ScrollViewer MainScrollViewer { get; } = new ScrollViewer();
        protected Grid MainGrid { get; } = new Grid();
        protected Border PlotPreviewPaneBorder { get; } = new Border();
        protected Border PlotConfigurationPaneBorder { get; } = new Border();
        protected Border PlotActionsPaneBorder { get; } = new Border();

        public bool Visible => this.Visibility == Visibility.Visible;

        public void Show(QwasiAnalyticsData analyticsData, QGEdge edge)
        {
            this.Visibility = Visibility.Visible;
            this.PlotConfigurationPane.LoadData(analyticsData, edge);
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
            this.PlotConfigurationPane.LoadData(null, null);
        }

        public void ReloadAnalyticsData(QwasiAnalyticsData analyticsData) => this.Show(analyticsData, this.PlotConfigurationPane.LoadedEdge);

        public WPFPlotFrame(QwasiEngine qwasiEngine)
        {
            this.BorderBrush = Brushes.DarkGray;
            this.BorderThickness = new Thickness(1);
            this.Background = Brushes.WhiteSmoke.Clone();
            this.Background.Opacity = 0.9;

            this.MainScrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.MainScrollViewer.VerticalAlignment = VerticalAlignment.Stretch;
            this.MainScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            this.MainScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //this.Child = this.MainScrollViewer;
            this.Child = this.MainGrid;

            this.MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            this.MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            this.MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            this.MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            this.MainGrid.Margin = new Thickness(4);

            this.PlotPreviewPane = new WPFPlotPreviewPane();
            this.PlotConfigurationPane = new WPFPlotConfigurationPane();
            this.PlotActionsPane = new WPFPlotActionsPane();
            this.PlotPreviewPane.PreviewedControl = this.PlotConfigurationPane.SizedPlotControl;
            this.PlotActionsPane.SetQwasiEngine(qwasiEngine);
            this.PlotActionsPane.SetBitmapGeneratorFunction(() => this.PlotConfigurationPane.GenerateBitmapImage());

            this.PlotPreviewPaneBorder.BorderThickness = new Thickness(1);
            this.PlotPreviewPaneBorder.BorderBrush = Brushes.Black;
            this.PlotPreviewPaneBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.PlotPreviewPaneBorder.VerticalAlignment = VerticalAlignment.Stretch;

            this.PlotPreviewPaneBorder.Child = this.PlotPreviewPane;
            Grid.SetColumn(this.PlotPreviewPane, 0);
            Grid.SetRow(this.PlotPreviewPane, 0);
            this.MainGrid.Children.Add(this.PlotPreviewPaneBorder);

            this.PlotConfigurationPaneBorder.Margin = new Thickness(0, 4, 0, 0);
            this.PlotConfigurationPaneBorder.BorderThickness = new Thickness(1);
            this.PlotConfigurationPaneBorder.BorderBrush = Brushes.DarkGray;
            this.PlotConfigurationPaneBorder.Child = this.PlotConfigurationPane;
            Grid.SetColumn(this.PlotConfigurationPaneBorder, 0);
            Grid.SetRow(this.PlotConfigurationPaneBorder, 1);
            this.MainGrid.Children.Add(this.PlotConfigurationPaneBorder);

            this.PlotActionsPaneBorder.Margin = new Thickness(0, 4, 0, 0);
            this.PlotActionsPaneBorder.BorderThickness = new Thickness(1);
            this.PlotActionsPaneBorder.BorderBrush = Brushes.DarkGray;
            this.PlotActionsPaneBorder.Child = this.PlotActionsPane;
            Grid.SetColumn(this.PlotActionsPaneBorder, 0);
            Grid.SetRow(this.PlotActionsPaneBorder, 2);
            this.MainGrid.Children.Add(this.PlotActionsPaneBorder);
        }
    }
}
