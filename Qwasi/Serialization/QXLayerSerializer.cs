using Qwasi.GraphUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qwasi.Serialization
{
    public abstract class QXLayerSerializer<TLayer> : QXControlSerializer<TLayer>
        where TLayer : IQGGraphLayer
    {
        protected QXVertexSerializer VertexSerializer { get; set; }
        protected QXEdgeSerializer EdgeSerializer { get; set; }

        public QXLayerSerializer(QXVertexSerializer vertexSerializer, QXEdgeSerializer edgeSerializer)
        {
            this.RegisterXmlAttribute("ID", l => l.LayerID, (l, s) => l.LayerID = s);
            this.SetAttributeProcessingMode("ID", QXProcessingMode.IgnoreIfEmpty);

            this.RegisterXmlAttribute("VertexEnumType",
                l => l.VertexEnumerationType.ToString(),
                (l, s) => l.VertexEnumerationType = (QGVertexEnumerationType)Enum.Parse(typeof(QGVertexEnumerationType), s));
            this.SetAttributeProcessingMode("VertexEnumType", QXProcessingMode.IgnoreIfEmpty);

            this.VertexSerializer = vertexSerializer;
            this.EdgeSerializer = edgeSerializer;

            this.RegisterXmlEnclosedElementList("Vertices", this.VertexSerializer,
                l => l.ChildControls.OfType<IQGVertex>(),
                (l, v) => l.ChildControls.Add((IQGVertex)v));

            this.RegisterXmlEnclosedElementList("Edges", this.EdgeSerializer,
                l => l.ChildControls.OfType<QGEdge>().Select(
                    e => new QGXmlEdgeInfo(QXVertexSerializer.VertexToIndexDictionary[e.Vertex1], QXVertexSerializer.VertexToIndexDictionary[e.Vertex2])),
                (l, ei) => QXEdgeSerializer.LoadedEdges.Add((QGXmlEdgeInfo)ei));
        }

        public QXLayerSerializer()
            : this(new QXVertexSerializer(), new QXEdgeSerializer())
        {
        }
    }
}
