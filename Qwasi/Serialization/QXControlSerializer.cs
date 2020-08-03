using Qwasi.GraphUI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Qwasi.Serialization
{
    public abstract class QXControlSerializer<T> : QXSerializerBase<T>
        where T : IQGControl
    {
        public QXProcessingMode PositionProcessingMode
        {
            get => this.GetAttributeProcessingMode("Position");
            set => this.SetAttributeProcessingMode("Position", value);
        }

        public QXProcessingMode HorizontalAlignmentProcessingMode
        {
            get => this.GetAttributeProcessingMode("HorizontalAlignment");
            set => this.SetAttributeProcessingMode("HorizontalAlignment", value);
        }

        public QXProcessingMode VerticalAlignmentProcessingMode
        {
            get => this.GetAttributeProcessingMode("VerticalAlignment");
            set => this.SetAttributeProcessingMode("VerticalAlignment", value);
        }

        public QXControlSerializer()
        {
            this.RegisterXmlAttribute("Position", v => v.Position.ToString(), (v, s) => v.SetPosition(Point.Parse(s)));
            this.RegisterXmlAttribute("HorizontalAlignment",
                c => c.HorizontalAlignment.ToString(),
                (c, s) => c.HorizontalAlignment = (QGHorizontalAlignment)Enum.Parse(typeof(QGHorizontalAlignment), s));
            this.RegisterXmlAttribute("VerticalAlignment",
                c => c.VerticalAlignment.ToString(),
                (c, s) => c.VerticalAlignment = (QGVerticalAlignment)Enum.Parse(typeof(QGVerticalAlignment), s));

            this.HorizontalAlignmentProcessingMode = QXProcessingMode.Ignore;
            this.VerticalAlignmentProcessingMode = QXProcessingMode.Ignore;
            this.PositionProcessingMode = QXProcessingMode.Required;
        }
    }
}
