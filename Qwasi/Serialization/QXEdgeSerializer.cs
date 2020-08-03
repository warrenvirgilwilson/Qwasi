using Qwasi.GraphUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Qwasi.Serialization
{
    public class QXEdgeSerializer : QXSerializerBase<QGXmlEdgeInfo>
    {
        public static List<QGXmlEdgeInfo> LoadedEdges { get; } = new List<QGXmlEdgeInfo>();

        public override string XmlElementName => "Edge";

        protected override QGXmlEdgeInfo GetObjectInstance() => new QGXmlEdgeInfo();

        public QXEdgeSerializer()
        {
            this.RegisterXmlAttribute("Vertex1", e => e.Vertex1Index.ToString(), (e, s) => e.Vertex1Index = int.Parse(s));
            this.RegisterXmlAttribute("Vertex2", e => e.Vertex2Index.ToString(), (e, s) => e.Vertex2Index = int.Parse(s));
        }
    }

    public class QGXmlEdgeInfo
    {
        public int Vertex1Index { get; set; }
        public int Vertex2Index { get; set; }

        public QGXmlEdgeInfo(int vertex1Index, int vertex2Index)
        {
            this.Vertex1Index = vertex1Index;
            this.Vertex2Index = vertex2Index;
        }

        public QGXmlEdgeInfo()
            : this(-1, -1)
        {
        }
    }
}
