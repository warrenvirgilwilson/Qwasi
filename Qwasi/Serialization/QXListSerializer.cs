using System;
using System.Collections.Generic;
using System.Text;

namespace Qwasi.Serialization
{
    public class QXEnclosedListSerializer : QXSerializerBase<IEnumerable<object>>
    {
        public override string XmlElementName { get; }

        protected override IEnumerable<object> GetObjectInstance()
        {
            return new List<object>();
        }

        public QXEnclosedListSerializer(string elementName, IQXSerializer childSerializer)
        {
            this.XmlElementName = elementName;

            this.RegisterXmlElementList(childSerializer, list => list, (list, child) => ((IList<object>)list).Add(child));
            this.IgnoreUnregisteredElements = false;
        }
    }

    public class QXListSerializer : QXExplicitSerializer<IEnumerable<object>>
    {
        public QXListSerializer(string elementName, IQXSerializer childSerializer)
            : base(elementName)
        {
            this.RegisterXmlElementList(childSerializer, list => list, (list, child) => ((IList<object>)list).Add(child));
            this.IgnoreUnregisteredElements = false;

            this.InstanceRetrievalMethod = () => new List<object>();
        }
    }
}
