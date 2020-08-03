using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qwasi.GraphUI;

namespace Qwasi.Serialization
{
    public abstract class QXPerimeterGraphSerializer<TGraph> : QXLayerSerializer<TGraph>
        where TGraph : QGPerimeterGraph
    {
        protected QXListSerializer VertexListSerializer { get; set; }

        public QXPerimeterGraphSerializer()
        {
            this.RegisterXmlAttribute("GraphRadius", pg => pg.GraphRadius.ToString(), (pg, s) => pg.GraphRadius = double.Parse(s));
            this.RegisterXmlAttribute("RotationAngle", pg => pg.RotationAngle.ToString(), (pg, s) => pg.RotationAngle = double.Parse(s));

            this.VertexSerializer.PositionProcessingMode = QXProcessingMode.Ignore;
            this.UnregisterXmlElement("Vertices");
            this.VertexListSerializer = new QXListSerializer("Vertices", this.VertexSerializer);
            this.RegisterXmlElement(this.VertexListSerializer, o => o.PerimeterVertices, (o, list) => o.AddPerimeterVertices((IEnumerable<QGVertexControl>)((IList<object>)list).Cast<QGVertexControl>()));
            this.MoveXmlElementToTop("Vertices"); ///
        }
    }

    public class QXStarGraphSerializer : QXPerimeterGraphSerializer<QGStarGraph>
    {
        public override string XmlElementName => "StarGraph";
        protected override QGStarGraph GetObjectInstance() => new QGStarGraph();

        public QXStarGraphSerializer()
            : base()
        {
            QXVertexSerializer centerVertexSerializer = new QXVertexSerializer("CenterVertex");
            centerVertexSerializer.PositionProcessingMode = QXProcessingMode.Ignore;

            this.RegisterXmlElement(centerVertexSerializer, sg => sg.CenterVertex, (sg, cv) => { });
            this.SetChildInstanceOverride("CenterVertex", sg => sg.CenterVertex);
            this.MoveXmlElementToTop("CenterVertex");
        }
    }

    public class QXPolygonalGraphSerializer : QXPerimeterGraphSerializer<QGPolygonalGraph>
    {
        public override string XmlElementName => "PolygonalGraph";
        protected override QGPolygonalGraph GetObjectInstance() => new QGPolygonalGraph();

        public QXPolygonalGraphSerializer()
            : base()
        {
        }
    }
}
