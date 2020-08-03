using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Qwasi.GraphUI
{
    public class QGStarGraph : QGPerimeterGraph
    {
        protected override QGInteractiveControl CenterControl => this.CenterVertex;
        public QGVertexControl CenterVertex { get; } = new QGVertexControl();

        private void __initGraph()
        {
            ((IQGVertex)this.CenterVertex).LabelControl.LabelPositionFixed = true;
            this.CenterVertex.Fill = Brushes.White;
            ((IQGVertex)this.CenterVertex).EdgeCreated += QGStarGraph_EdgeCreated;
        }

        public QGStarGraph()
            : base()
        {
            __initGraph();
        }

        public QGStarGraph(int perimeterVertexCount)
            : base(perimeterVertexCount)
        {
            __initGraph();
        }

        protected override void GenerateVertexConnections()
        {
            foreach (QGVertexControl vc in this.PerimeterVertexList)
                this.CenterVertex.CreateEdgeIfAbsent(vc);
        }

        private void QGStarGraph_EdgeCreated(object sender, QGVertexEdgeEventArgs e)
        {
            e.Edge.Deleting += __deletePerimeterVertexToo;
        }

        private void __deletePerimeterVertexToo(object sender, QGControlEventArgs e)
        {
            ((QGEdge)sender).OtherVertex(this.CenterVertex).DeleteControl();
        }
    }
}
