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
    public class QGPolygonalGraph : QGPerimeterGraph
    {
        protected override QGInteractiveControl CenterControl => this.CenterDummyVertex;
        public QGDummyVertexControl CenterDummyVertex { get; } = new QGDummyVertexControl();

        public QGPolygonalGraph()
            : base()
        {
            this.CenterDummyVertex.Fill = Brushes.White;
        }

        public QGPolygonalGraph(int perimeterVertexCount)
            : base(perimeterVertexCount)
        {
            this.CenterDummyVertex.Fill = Brushes.White;
        }

        protected override void GenerateVertexConnections()
        {
            for (int i = 0; i < this.NodeCount; i++)
            {
                QGEdge edge = this.PerimeterVertexList[i].CreateEdgeIfAbsent(this.PerimeterVertexList[(i + 1 < this.NodeCount) ? i + 1 : 0]);
                edge.UserDeletable = false;
            }
        }

        public override void AddPerimeterVertices(IEnumerable<QGVertexControl> vertices)
        {
            if (this.NodeCount >= 2)
                this.PerimeterVertexList[0].GetEdge(this.PerimeterVertexList[this.NodeCount - 1]).DeleteControl();

            base.AddPerimeterVertices(vertices);
        }
    }
}
