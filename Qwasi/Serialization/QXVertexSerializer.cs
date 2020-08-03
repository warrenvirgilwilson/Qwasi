using Qwasi.GraphUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Qwasi.Serialization
{
    public class QXVertexSerializer : QXControlSerializer<IQGVertex>
    {
        public static Dictionary<IQGVertex, int> VertexToIndexDictionary { get; } = new Dictionary<IQGVertex, int>();
        public static Dictionary<int, IQGVertex> IndexToVertexDictionary { get; } = new Dictionary<int, IQGVertex>();

        public override string XmlElementName { get; } = "Vertex"
;
        protected override IQGVertex GetObjectInstance() => new QGVertexControl();

        public QXVertexSerializer()
        {
            this.HorizontalAlignmentProcessingMode = QXProcessingMode.Ignore;
            this.VerticalAlignmentProcessingMode = QXProcessingMode.Ignore;

            this.RegisterXmlAttribute("Index", v => VertexToIndexDictionary[v].ToString(), (v, s) => IndexToVertexDictionary.Add(int.Parse(s), v));
            this.RegisterXmlAttribute("ID", v => v.ID.ToString(), (v, s) => v.ID = s);
            this.RegisterXmlAttribute("IDType", v => v.VertexIDType.ToString(), (v, s) => v.VertexIDType = (QGVertexIDType)Enum.Parse(typeof(QGVertexIDType), s));

            this.RegisterXmlAttribute("VertexStatus", v => v.VertexStatus, (v, s) => v.SetVertexStatus(s));
            this.SetAttributeProcessingMode("VertexStatus", QXProcessingMode.IgnoreIfEmpty);
        }

        public QXVertexSerializer(string elementName)
            : this()
        {
            this.XmlElementName = elementName;
        }
    }
}
