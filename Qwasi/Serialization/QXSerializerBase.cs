using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Xml;

namespace Qwasi.Serialization
{
    public enum QXProcessingMode { Required, Optional, IgnoreIfEmpty, Ignore }
    public delegate string QXAttributeEncoder<T>(T owner);
    public delegate void QXAttributeDecoder<T>(T owner, string encodedValue);

    public enum QXElementType { SingleElement, List }
    public delegate IEnumerable<object> QXElementListSelector<T>(T parent);
    public delegate void QXContentIntegrator<T>(T parent, object deserializedChild);
    public delegate object QXSingleElementSelector<T>(T parent);

    public delegate object QXInstanceOverrideMethod<T>(T parent);

    public delegate T QXExplicitInstanceRetrievalMethod<T>();

    public interface IQXSerializer
    {
        public string XmlElementName { get; }

        public void Serialize(object target, XmlWriter writer);
        public object Deserialize(XmlReader reader);
        public object Deserialize(object childInstance, XmlReader reader);
    }

    public abstract class QXSerializerBase<T> : IQXSerializer
    {
        private QXAttributeInfoCollection<T> _AttributeInfoCollection = new QXAttributeInfoCollection<T>();
        private QXElementInfoCollection<T> _ElementInfoCollection = new QXElementInfoCollection<T>();

        public abstract string XmlElementName { get; }

        public bool IgnoreUnregisteredAttributes { get; set; } = true;
        public bool IgnoreUnregisteredElements { get; set; } = true;

        protected QXAttributeInfo<T> GetAttributeInfo(string attributeName)
        {
            if (!_AttributeInfoCollection.Contains(attributeName))
                throw new Exception("There has been no attribute \"" + attributeName + "\" registered to this serializer.");

            return _AttributeInfoCollection[attributeName];
        }

        protected void MoveXmlAttributeToTop(string attributeName) => this.SetXmlAttributeProcessingIndex(attributeName, 0);
        protected void SetXmlAttributeProcessingIndex(string attributeName, int positionIndex)
        {
            if (!_AttributeInfoCollection.Contains(attributeName))
                throw new Exception("Cannot change the processing index for attribute \"" + attributeName + "\" because no such attribute has been registered.");

            QXAttributeInfo<T> ai = _AttributeInfoCollection[attributeName];
            _AttributeInfoCollection.Remove(attributeName);
            _AttributeInfoCollection.Insert(positionIndex, ai);
        }

        protected void RegisterXmlAttribute(string attributeName, QXAttributeEncoder<T> encoder, QXAttributeDecoder<T> decoder)
        {
            if (_AttributeInfoCollection.Contains(attributeName))
                throw new Exception("The attribute \"" + attributeName + "\" has already been registered to the serializer.");

            _AttributeInfoCollection.Add(new QXAttributeInfo<T>(attributeName) { Encoder = encoder, Decoder = decoder });
        }

        protected void UnregisterXmlAttribute(string attributeName)
        {
            if (!_AttributeInfoCollection.Contains(attributeName))
                throw new Exception("Cannot unregister attribute \"" + attributeName + "\" because no such attribute has been registered.");

            _AttributeInfoCollection.Remove("attributeName");
        }

        protected QXProcessingMode GetAttributeProcessingMode(string attributeName)
        {
            return this.GetAttributeInfo(attributeName).ProcessingMode;
        }

        protected void SetAttributeProcessingMode(string attributeName, QXProcessingMode mode)
        {
            this.GetAttributeInfo(attributeName).ProcessingMode = mode;
        }

        protected void WriteXmlAttributes(T owner, XmlWriter writer)
        {
            foreach (QXAttributeInfo<T> ai in _AttributeInfoCollection)
            {
                if (ai.ProcessingMode == QXProcessingMode.Ignore)
                    continue;

                string attributeValue = ai.Encoder(owner);
                if (ai.ProcessingMode == QXProcessingMode.IgnoreIfEmpty && (attributeValue == null || attributeValue == ""))
                    continue;

                writer.WriteAttributeString(ai.AttributeName, ai.Encoder(owner));
            }
        }

        protected void ReadXmlAttributes(T owner, XmlReader reader)
        {
            reader.MoveToElement();
            HashSet<string> encounteredAttributes = new HashSet<string>();

            int attributeCount = reader.AttributeCount;
            for (int i = 0; i < attributeCount; i++)
            {
                reader.MoveToAttribute(i);
                string attributeName = reader.Name;
                string attributeValue = reader.Value;

                if (!_AttributeInfoCollection.Contains(attributeName))
                {
                    if (!this.IgnoreUnregisteredAttributes)
                        throw new Exception("Unregistered attribute \"" + attributeName + "\" encountered while reading attribues.");
                    else
                        continue;
                }

                QXAttributeInfo<T> attributeInfo = this.GetAttributeInfo(attributeName);
                if (attributeInfo.ProcessingMode == QXProcessingMode.Ignore)
                    continue;

                // Process the attribute value
                attributeInfo.Decoder(owner, attributeValue);
                encounteredAttributes.Add(attributeName);
            }

            foreach (QXAttributeInfo<T> ai in _AttributeInfoCollection)
                if (ai.ProcessingMode == QXProcessingMode.Required && !encounteredAttributes.Contains(ai.AttributeName))
                    throw new Exception("Required attribute \"" + ai.AttributeName + "\" not present.");
        }

        protected QXElementInfo<T> GetElementInfo(string elementName)
        {
            if (!_ElementInfoCollection.Contains(elementName))
                throw new Exception("There has been no serializer for element \"" + elementName + "\" registered to the parent serializer.");

            return _ElementInfoCollection[elementName];
        }

        protected void MoveXmlElementToTop(string elementName) => this.SetXmlElementProcessingIndex(elementName, 0);
        protected void SetXmlElementProcessingIndex(string elementName, int positionIndex)
        {
            if (!_ElementInfoCollection.Contains(elementName))
                throw new Exception("Cannot change the processing index for element \"" + elementName + "\" because no such element has been registered.");

            QXElementInfo<T> ei = _ElementInfoCollection[elementName];
            _ElementInfoCollection.Remove(elementName);
            _ElementInfoCollection.Insert(positionIndex, ei);
        }

        protected void RegisterXmlElement(IQXSerializer elementSerializer, QXSingleElementSelector<T> selector, QXContentIntegrator<T> integrator)
        {
            if (_ElementInfoCollection.Contains(elementSerializer.XmlElementName))
                throw new Exception("The serializer for element \"" + elementSerializer.XmlElementName + "\" has already been registered to the parent serializer.");

            _ElementInfoCollection.Add(new QXElementInfo<T>(elementSerializer) {
                ContentSelector = o => new object[] { selector(o) }, ContentIntegrator = integrator, ElementType = QXElementType.SingleElement });
        }

        protected void RegisterXmlElementList(IQXSerializer elementSerializer, QXElementListSelector<T> selector, QXContentIntegrator<T> integrator)
        {
            if (_ElementInfoCollection.Contains(elementSerializer.XmlElementName))
                throw new Exception("The serializer for element \"" + elementSerializer.XmlElementName + "\" has already been registered to the parent serializer.");

            _ElementInfoCollection.Add(new QXElementInfo<T>(elementSerializer) {
                ContentSelector = selector, ContentIntegrator = integrator, ElementType = QXElementType.List});
        }

        protected void RegisterXmlEnclosedElementList(string enclosingElementName,
            IQXSerializer elementSerializer, QXElementListSelector<T> selector, QXContentIntegrator<T> integrator)
        {
            object listSelector(T parent)
            {
                return selector(parent);
            }

            void listIntegrator(T parent, object deserializedChild)
            {
                foreach (object item in (IEnumerable<object>)deserializedChild)
                    integrator(parent, item);
            }

            QXEnclosedListSerializer listSerializer = new QXEnclosedListSerializer(enclosingElementName, elementSerializer);
            this.RegisterXmlElement(listSerializer, listSelector, listIntegrator);
            this.SetElementProcessingMode(enclosingElementName, QXProcessingMode.IgnoreIfEmpty);
        }

        protected void UnregisterXmlElement(IQXSerializer elementSerializer) => this.UnregisterXmlElement(elementSerializer.XmlElementName);
        protected void UnregisterXmlElement(string elementName)
        {
            if (!_ElementInfoCollection.Contains(elementName))
                throw new Exception("Cannot unregister element \"" + elementName + "\" because no such element has been registered.");

            _ElementInfoCollection.Remove(elementName);
        }

        protected QXProcessingMode GetElementProcessingMode(string elementName)
        {
            return this.GetElementInfo(elementName).ProcessingMode;
        }

        protected void SetElementProcessingMode(string elementName, QXProcessingMode mode)
        {
            this.GetElementInfo(elementName).ProcessingMode = mode;
        }

        protected void SetChildInstanceOverride(string elementName, QXInstanceOverrideMethod<T> overrideMethod)
        {
            this.GetElementInfo(elementName).ChildInstanceOverride = overrideMethod;
        }

        protected void WriteXmlChildElements(T parent, XmlWriter writer)
        {
            foreach (QXElementInfo<T> ei in _ElementInfoCollection)
            {
                if (ei.ProcessingMode == QXProcessingMode.Ignore)
                    continue;

                IEnumerable<object> objectsToSerialize = ei.ContentSelector(parent);
                foreach (object o in objectsToSerialize)
                    ei.ElementSerializer.Serialize(o, writer);
            }
        }

        protected void ReadXmlChildElements(T parent, XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                foreach (QXElementInfo<T> ei in _ElementInfoCollection)
                    if (ei.ProcessingMode == QXProcessingMode.Required)
                        throw new Exception("Element \"" + this.XmlElementName + "\" is empty, but requires child element \"" + ei.ElementName + "\".");

                return;
            }

            Dictionary<QXElementInfo<T>, int> encounteredElements = _ElementInfoCollection.ToDictionary(ei => ei, ei => 0);

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Whitespace || reader.NodeType == XmlNodeType.SignificantWhitespace)
                    continue;
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != this.XmlElementName)
                        throw new Exception("End element tag is not of the type expected.");
                    break;
                }

                if (reader.NodeType != XmlNodeType.Element)
                    throw new Exception();

                string elementName = reader.Name;

                QXElementInfo<T> elementInfo = _ElementInfoCollection.Contains(elementName) ? this.GetElementInfo(elementName) : null;
                if (elementInfo == null && !this.IgnoreUnregisteredElements)
                    throw new Exception("Unregistered element \"" + elementName + "\" encountered while reading elements.");
                else if (elementInfo == null || elementInfo.ProcessingMode == QXProcessingMode.Ignore)
                {
                    reader.Skip();
                    reader.ReadEndElement();
                    continue;
                }

                if (reader.IsEmptyElement && elementInfo.ProcessingMode == QXProcessingMode.IgnoreIfEmpty)
                    continue;

                // Process the element
                object deserializedChild;
                if (elementInfo.ChildInstanceOverride != null)
                    deserializedChild = elementInfo.ElementSerializer.Deserialize(elementInfo.ChildInstanceOverride(parent), reader);
                else
                    deserializedChild = elementInfo.ElementSerializer.Deserialize(reader);

                elementInfo.ContentIntegrator(parent, deserializedChild);
                encounteredElements[elementInfo]++;

                if (elementInfo.ElementType == QXElementType.SingleElement && encounteredElements[elementInfo] > 1)
                    throw new Exception("Element \"" + elementInfo.ElementName + "\" only expected to appear once.");
            }

            foreach (QXElementInfo<T> ei in encounteredElements.Keys)
            {
                if (ei.ProcessingMode == QXProcessingMode.Required)
                {
                    if ((ei.ElementType == QXElementType.SingleElement && encounteredElements[ei] != 1) ||
                        (ei.ElementType == QXElementType.List && encounteredElements[ei] == 0))
                        throw new Exception("Required element \"" + ei.ElementName + "\" not encountered during deserialization.");
                }
            }

            reader.ReadEndElement();
        }

        //------

        public event EventHandler<QXSerializingEventArgs<T>> BeforeSerializing;
        protected virtual void OnBeforeSerializing(QXSerializingEventArgs<T> e) => this.BeforeSerializing?.Invoke(this, e);
        private void RaiseBeforeSerializing(QXSerializingEventArgs<T> e) => this.OnBeforeSerializing(e);

        public event EventHandler<QXSerializingEventArgs<T>> AfterSerializing;
        protected virtual void OnAfterSerializing(QXSerializingEventArgs<T> e) => this.AfterSerializing?.Invoke(this, e);
        private void RaiseAfterSerializing(QXSerializingEventArgs<T> e) => this.OnAfterSerializing(e);

        public event EventHandler<QXDeserializingEventArgs<T>> BeforeDeserializing;
        protected virtual void OnBeforeDeserializing(QXDeserializingEventArgs<T> e) => this.BeforeDeserializing?.Invoke(this, e);
        private void RaiseBeforeDeserializing(QXDeserializingEventArgs<T> e) => this.OnBeforeDeserializing(e);

        public event EventHandler<QXDeserializingEventArgs<T>> AfterDeserializing;
        protected virtual void OnAfterDeserializing(QXDeserializingEventArgs<T> e) => this.AfterDeserializing?.Invoke(this, e);
        private void RaiseAfterDeserializing(QXDeserializingEventArgs<T> e) => this.OnAfterDeserializing(e);

        protected abstract T GetObjectInstance();

        public void Serialize(object target, XmlWriter writer)
        {
            if (!(target is T))
                throw new Exception("Cannot serialize object because it is not an instance of the type required.");

            this.RaiseBeforeSerializing(new QXSerializingEventArgs<T>((T)target));
            writer.WriteStartElement(this.XmlElementName);

            this.WriteXmlAttributes((T)target, writer);
            this.WriteXmlChildElements((T)target, writer);

            writer.WriteEndElement();
            this.RaiseAfterSerializing(new QXSerializingEventArgs<T>((T)target));
        }

        public object Deserialize(XmlReader reader) => this.Deserialize(this.GetObjectInstance(), reader);
        public object Deserialize(object childInstance, XmlReader reader)
        {
            if (reader.Name != this.XmlElementName)
                throw new Exception("Cannot read element \"" + XmlElementName + "\" because it is of the wrong type.");

            if (!(childInstance is T deserializedObject))
                throw new Exception("Deserialized object instance override is not of the type expected.");

            this.RaiseBeforeDeserializing(new QXDeserializingEventArgs<T>(deserializedObject));

            this.ReadXmlAttributes(deserializedObject, reader);
            reader.MoveToContent();
            this.ReadXmlChildElements(deserializedObject, reader);

            this.RaiseAfterDeserializing(new QXDeserializingEventArgs<T>(deserializedObject));
            return deserializedObject;
        }
    }

    public class QXExplicitSerializer<T> : QXSerializerBase<T>
    {
        private QXAttributeInfoCollection<T> _AttributeInfoCollection = new QXAttributeInfoCollection<T>();
        private QXElementInfoCollection<T> _ElementInfoCollection = new QXElementInfoCollection<T>();

        public override string XmlElementName { get; }

        public QXExplicitInstanceRetrievalMethod<T> InstanceRetrievalMethod { get; set; }
        protected override T GetObjectInstance()
        {
            return this.InstanceRetrievalMethod();
        }

        public new void MoveXmlAttributeToTop(string attributeName) => base.MoveXmlAttributeToTop(attributeName);
        public new void SetXmlAttributeProcessingIndex(string attributeName, int positionIndex) => base.SetXmlAttributeProcessingIndex(attributeName, positionIndex);
        public new void RegisterXmlAttribute(string attributeName, QXAttributeEncoder<T> encoder, QXAttributeDecoder<T> decoder)
            => base.RegisterXmlAttribute(attributeName, encoder, decoder);
        public new void UnregisterXmlAttribute(string attributeName) => base.UnregisterXmlAttribute(attributeName);
        public new QXProcessingMode GetAttributeProcessingMode(string attributeName) => base.GetAttributeProcessingMode(attributeName);
        public new void SetAttributeProcessingMode(string attributeName, QXProcessingMode mode) => base.SetAttributeProcessingMode(attributeName, mode);

        public new void MoveXmlElementToTop(string elementName) => base.MoveXmlElementToTop(elementName);
        public new void SetXmlElementProcessingIndex(string elementName, int positionIndex) => base.SetXmlElementProcessingIndex(elementName, positionIndex);
        public new void RegisterXmlElement(IQXSerializer elementSerializer, QXSingleElementSelector<T> selector, QXContentIntegrator<T> integrator)
            => base.RegisterXmlElement(elementSerializer, selector, integrator);
        public new void RegisterXmlElementList(IQXSerializer elementSerializer, QXElementListSelector<T> selector, QXContentIntegrator<T> integrator)
            => base.RegisterXmlElementList(elementSerializer, selector, integrator);
        public new void RegisterXmlEnclosedElementList(string enclosingElementName,
            IQXSerializer elementSerializer, QXElementListSelector<T> selector, QXContentIntegrator<T> integrator)
            => base.RegisterXmlEnclosedElementList(enclosingElementName, elementSerializer, selector, integrator);
        public new void UnregisterXmlElement(string elementName) => base.UnregisterXmlElement(elementName);
        public new void UnregisterXmlElement(IQXSerializer elementSerializer) => base.UnregisterXmlElement(elementSerializer);
        public new QXProcessingMode GetElementProcessingMode(string elementName)
            => base.GetElementProcessingMode(elementName);
        public new void SetElementProcessingMode(string elementName, QXProcessingMode mode)
            => base.SetElementProcessingMode(elementName, mode);
        public new void SetChildInstanceOverride(string elementName, QXInstanceOverrideMethod<T> overrideMethod)
            => base.SetChildInstanceOverride(elementName, overrideMethod);

        public QXExplicitSerializer(string xmlElementName)
            : base()
        {
            this.XmlElementName = xmlElementName;
        }
    }

    public class QXAttributeInfo<T>
    {
        public string AttributeName { get; }
        public QXAttributeEncoder<T> Encoder { get; set; } = null;
        public QXAttributeDecoder<T> Decoder { get; set; } = null;

        public QXProcessingMode ProcessingMode { get; set; } = QXProcessingMode.Optional;

        public QXAttributeInfo(string attributeName)
        {
            this.AttributeName = attributeName;
        }
    }

    public class QXAttributeInfoCollection<T> : System.Collections.ObjectModel.KeyedCollection<string, QXAttributeInfo<T>>
    {
        protected override string GetKeyForItem(QXAttributeInfo<T> item) => item.AttributeName;
    }

    public class QXElementInfo<T>
    {
        public IQXSerializer ElementSerializer { get; }
        public string ElementName => this.ElementSerializer.XmlElementName;

        public QXElementListSelector<T> ContentSelector { get; set; } = null;
        public QXContentIntegrator<T> ContentIntegrator { get; set; } = null;

        public QXInstanceOverrideMethod<T> ChildInstanceOverride { get; set; } = null;

        public QXProcessingMode ProcessingMode { get; set; } = QXProcessingMode.Optional;
        public QXElementType ElementType { get; set; } = QXElementType.List;

        public QXElementInfo(IQXSerializer elementSerializer)
        {
            this.ElementSerializer = elementSerializer;
        }
    }

    public class QXElementInfoCollection<T> : System.Collections.ObjectModel.KeyedCollection<string, QXElementInfo<T>>
    {
        protected override string GetKeyForItem(QXElementInfo<T> item) => item.ElementName;
    }

    public class QXSerializingEventArgs<T> : EventArgs
    {
        public T SerializingTarget { get; }

        public QXSerializingEventArgs(T serializingTarget)
        {
            this.SerializingTarget = serializingTarget;
        }
    }

    public class QXDeserializingEventArgs<T> : EventArgs
    {
        public T DeserializedObject { get; }

        public QXDeserializingEventArgs(T deserializingObject)
        {
            this.DeserializedObject = deserializingObject;
        }
    }
}
