using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Qwasi.GraphUI;
using Qwasi.WPF.Controls;

namespace Qwasi.WPF
{
    public class WPFGraphBuilderPane : WPFContentPane
    {
        public WPFButton AddVertexButton { get; } = new WPFButton();
        public WPFButton CreateEdgeButton { get; } = new WPFButton();
        public WPFButton DeleteControlsButton { get; } = new WPFButton();
        public WPFButtonIntField AddStarGraphFB { get; } = new WPFButtonIntField();
        public WPFButtonIntField AddPolygonalGraphFB { get; } = new WPFButtonIntField();

        protected WPFGraphControl WPFGraphControl { get; private set; }
        public void SetWPFGraphControl(WPFGraphControl graphControl) => this.WPFGraphControl = graphControl;

        protected IEnumerable<IQGControl> LoadedGraphControls { get; private set; }

        public void LoadGraphControls(IEnumerable<IQGControl> loadControls)
        {
            this.LoadedGraphControls = loadControls.ToArray();

            this.CreateEdgeButton.IsEnabled = this.LoadedGraphControls.OfType<IQGVertex>().Count() == 2;
            this.DeleteControlsButton.IsEnabled = this.LoadedGraphControls.OfType<IQGUserDeletable>().Count() > 0;
        }

        public WPFGraphBuilderPane()
            : base()
        {
            this.TitleText = "Graph Building";

            this.AddVertexButton.Content = "Add Vertex";
            this.CreateEdgeButton.Content = "Create Edge";
            this.DeleteControlsButton.Content = "Delete Selected Controls";
            this.AddStarGraphFB.Content = "Add Star Graph:";
            this.AddStarGraphFB.FieldText = "5";
            this.AddPolygonalGraphFB.Content = "Add Polygonal Graph:";
            this.AddPolygonalGraphFB.FieldText = "5";

            this.AddStarGraphFB.PartitionAlignment = PartitionAlignment.Right;
            this.AddStarGraphFB.PartitionLocation = 40;
            this.AddStarGraphFB.PartitionMetric = GridUnitType.Pixel;
            this.AddPolygonalGraphFB.PartitionAlignment = PartitionAlignment.Right;
            this.AddPolygonalGraphFB.PartitionLocation = 40;
            this.AddPolygonalGraphFB.PartitionMetric = GridUnitType.Pixel;

            this.Items.Add(this.AddVertexButton);
            this.Items.Add(this.CreateEdgeButton);
            this.Items.Add(this.DeleteControlsButton);
            this.Items.Add(this.AddStarGraphFB);
            this.Items.Add(this.AddPolygonalGraphFB);

            this.AddVertexButton.Click += AddVertex_Click;
            this.CreateEdgeButton.Click += CreateEdge_Click;
            this.DeleteControlsButton.Click += DeleteControls_Click;
            this.AddStarGraphFB.Click += AddStarGraph_Click;
            this.AddPolygonalGraphFB.Click += AddPolygonalGraph_Click;

            this.LoadGraphControls(Enumerable.Empty<IQGControl>());
        }

        private Point __getRootLayerMidpoint() => new Point(
            this.WPFGraphControl.ScrollViewer.HorizontalOffset + (this.WPFGraphControl.ActualWidth / 2),
            this.WPFGraphControl.ScrollViewer.VerticalOffset + (this.WPFGraphControl.ActualHeight / 2));

        private void AddVertex_Click(object sender, RoutedEventArgs e)
        {
            IQGVertex v = new QGVertexControl();

            WPFGraphControl.RootLayer.ChildControls.Add(v);
            v.SetPosition(__getRootLayerMidpoint());
        }

        private void CreateEdge_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<IQGVertex> vertices = this.LoadedGraphControls.OfType<IQGVertex>();
            if (vertices.Count() == 2)
                vertices.ElementAt(0).CreateEdgeIfAbsent(vertices.ElementAt(1));
        }

        private void DeleteControls_Click(object sender, RoutedEventArgs e)
        {
            QGUserInterface.LockGlobalRefresh(this);

            foreach (IQGUserDeletable udcontrol in this.LoadedGraphControls.OfType<IQGUserDeletable>())
                udcontrol.UserIssuedDelete();

            QGUserInterface.UnlockGlobalRefresh(this);
        }

        private void AddStarGraph_Click(object sender, RoutedEventArgs e)
        {
            QGStarGraph sg = new QGStarGraph(this.AddStarGraphFB.ParsedValue);

            WPFGraphControl.RootLayer.ChildControls.Add(sg);
            sg.SetPosition(__getRootLayerMidpoint());
        }

        private void AddPolygonalGraph_Click(object sender, RoutedEventArgs e)
        {
            QGPolygonalGraph pg = new QGPolygonalGraph(this.AddPolygonalGraphFB.ParsedValue);

            WPFGraphControl.RootLayer.ChildControls.Add(pg);
            pg.SetPosition(__getRootLayerMidpoint());
        }
    }
}
