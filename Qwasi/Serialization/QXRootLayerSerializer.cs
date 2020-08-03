using Qwasi.GraphUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qwasi.Serialization
{
    public class QXRootLayerSerializer : QXLayerSerializer<QGRootLayer>
    {
        public override string XmlElementName => "QwasiGraph";

        protected QXStarGraphSerializer StarGraphSerializer { get; set; }
        protected QXPolygonalGraphSerializer PolygonalGraphSerializer { get; set; }

        protected override QGRootLayer GetObjectInstance()
        {
            return new QGRootLayer();
        }

        protected override void OnBeforeSerializing(QXSerializingEventArgs<QGRootLayer> e)
        {
            base.OnBeforeSerializing(e);

            QXVertexSerializer.VertexToIndexDictionary.Clear();
            int vertexIndex = 0;
            foreach (IQGVertex v in e.SerializingTarget.AllDescendentControls.OfType<IQGVertex>())
                QXVertexSerializer.VertexToIndexDictionary.Add(v, vertexIndex++);
        }

        protected override void OnAfterSerializing(QXSerializingEventArgs<QGRootLayer> e)
        {
            base.OnAfterSerializing(e);

            QXVertexSerializer.VertexToIndexDictionary.Clear();
        }

        protected override void OnBeforeDeserializing(QXDeserializingEventArgs<QGRootLayer> e)
        {
            base.OnBeforeDeserializing(e);

            QXVertexSerializer.IndexToVertexDictionary.Clear();
            QXEdgeSerializer.LoadedEdges.Clear();
        }

        protected override void OnAfterDeserializing(QXDeserializingEventArgs<QGRootLayer> e)
        {
            base.OnAfterDeserializing(e);

            foreach (QGXmlEdgeInfo ei in QXEdgeSerializer.LoadedEdges)
            {
                IQGVertex vertex1 = QXVertexSerializer.IndexToVertexDictionary[ei.Vertex1Index];
                IQGVertex vertex2 = QXVertexSerializer.IndexToVertexDictionary[ei.Vertex2Index];

                vertex1.CreateEdge(vertex2);
            }

            QXVertexSerializer.IndexToVertexDictionary.Clear();
            QXEdgeSerializer.LoadedEdges.Clear();
        }

        public QXRootLayerSerializer()
        {
            this.PositionProcessingMode = QXProcessingMode.Ignore;

            this.StarGraphSerializer = new QXStarGraphSerializer();
            this.RegisterXmlElementList(this.StarGraphSerializer,
                rl => rl.ChildControls.OfType<QGStarGraph>(),
                (rl, sg) => rl.ChildControls.Add((QGStarGraph)sg));

            this.PolygonalGraphSerializer = new QXPolygonalGraphSerializer();
            this.RegisterXmlElementList(this.PolygonalGraphSerializer,
                rl => rl.ChildControls.OfType<QGPolygonalGraph>(),
                (rl, pg) => rl.ChildControls.Add((QGPolygonalGraph)pg));
        }
    }
}
