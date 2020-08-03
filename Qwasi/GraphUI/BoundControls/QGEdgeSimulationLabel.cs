using Qwasi.HilbertSpaceMath;
using Qwasi.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Qwasi.GraphUI
{
    public class QGEdgeSimulationLabel : QGEdgeLabel
    {
        protected WPFPushpinButton PushpinButton { get; } = new WPFPushpinButton();
        protected override bool IsLabelPinned
        {
            get => this.PushpinButton.IsPinned;
            set => this.PushpinButton.IsPinned = value;
        }

        protected TextBlock TBStateVector1 { get; } = new TextBlock();
        protected TextBlock TBStateVector2 { get; } = new TextBlock();
        protected TextBlock TBCoefficient1 { get; } = new TextBlock();
        protected TextBlock TBCoefficient2 { get; } = new TextBlock();

        public string StateVector1Tag
        {
            get => this.TBStateVector1.Text;
            private set => this.TBStateVector1.Text = value;
        }

        public string StateVector2Tag
        {
            get => this.TBStateVector2.Text;
            private set => this.TBStateVector2.Text = value;
        }

        public string Coefficient1
        {
            get => this.TBCoefficient1.Text;
            private set => this.TBCoefficient1.Text = value;
        }

        public string Coefficient2
        {
            get => this.TBCoefficient2.Text;
            private set => this.TBCoefficient2.Text = value;
        }

        public void SetEdgeStateCoefficient(HSEdgeState edgeState, string value)
        {
            if (edgeState.Vertex1 == this.ParentEdge.Vertex1 && edgeState.Vertex2 == this.ParentEdge.Vertex2)
            {
                this.StateVector1Tag = " " + edgeState.ToString() + " ";
                this.Coefficient1 = value;
            }
            else if (edgeState.Vertex1 == this.ParentEdge.Vertex2 && edgeState.Vertex2 == this.ParentEdge.Vertex1)
            {
                this.StateVector2Tag = " " + edgeState.ToString() + " ";
                this.Coefficient2 = value;
            }

            this.RefreshDimensions();
        }

        public QGEdgeSimulationLabel()
            : base()
        {
            this.HorizontalAlignment = QGHorizontalAlignment.Left;
            RowDefinition row1 = new RowDefinition();
            RowDefinition row2 = new RowDefinition();
            ColumnDefinition col1 = new ColumnDefinition();
            ColumnDefinition col2 = new ColumnDefinition();
            ColumnDefinition col3 = new ColumnDefinition();
            row1.Height = new GridLength(0, GridUnitType.Auto);
            row2.Height = new GridLength(0, GridUnitType.Auto);
            col1.Width = new GridLength(0, GridUnitType.Auto);
            col2.Width = new GridLength(0, GridUnitType.Auto);
            this.WPFGrid.RowDefinitions.Add(row1);
            this.WPFGrid.RowDefinitions.Add(row2);
            this.WPFGrid.ColumnDefinitions.Add(col1);
            this.WPFGrid.ColumnDefinitions.Add(col2);
            this.WPFGrid.ColumnDefinitions.Add(col3);
            this.WPFGrid.Margin = new Thickness(4, 2, 4, 2);

            this.RegisterHitTestElement(this.PushpinButton);
            this.PushpinButton.Width = 17;
            this.PushpinButton.Height = 17;
            this.WPFGrid.Children.Add(this.PushpinButton);
            Grid.SetColumn(this.PushpinButton, 0);
            Grid.SetRow(this.PushpinButton, 0);

            this.WPFGrid.Children.Add(this.TBStateVector1);
            this.WPFGrid.Children.Add(this.TBStateVector2);
            Grid.SetColumn(this.TBStateVector1, 1);
            Grid.SetColumn(this.TBStateVector2, 1);
            Grid.SetRow(this.TBStateVector1, 0);
            Grid.SetRow(this.TBStateVector2, 1);

            this.WPFGrid.Children.Add(this.TBCoefficient1);
            this.WPFGrid.Children.Add(this.TBCoefficient2);
            Grid.SetColumn(this.TBCoefficient1, 2);
            Grid.SetColumn(this.TBCoefficient2, 2);
            Grid.SetRow(this.TBCoefficient1, 0);
            Grid.SetRow(this.TBCoefficient2, 1);

            this.TBStateVector1.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.TBStateVector2.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.TBCoefficient1.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.TBCoefficient2.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            this.TBStateVector1.FontFamily = new FontFamily("Cambria Math");
            this.TBStateVector1.FontSize = 12;
            this.TBStateVector1.FontWeight = FontWeights.Bold;
            this.TBStateVector1.Foreground = Brushes.Red;
            this.TBStateVector2.FontFamily = new FontFamily("Cambria Math");
            this.TBStateVector2.FontSize = 12;
            this.TBStateVector2.FontWeight = FontWeights.Bold;
            this.TBStateVector2.Foreground = Brushes.Red;
            this.TBCoefficient1.FontFamily = new FontFamily("Cambria Math");
            this.TBCoefficient1.FontSize = 11.5;
            this.TBCoefficient2.FontFamily = new FontFamily("Cambria Math");
            this.TBCoefficient2.FontSize = 11.5;

            this.WPFRectangleFrame.Fill = Brushes.LightYellow.Clone();
            this.WPFRectangleFrame.Fill.Opacity = 0.8;

            this.TBStateVector1.IsHitTestVisible = false;
            this.TBStateVector2.IsHitTestVisible = false;
            this.TBCoefficient1.IsHitTestVisible = false;
            this.TBCoefficient2.IsHitTestVisible = false;
        }
    }
}
