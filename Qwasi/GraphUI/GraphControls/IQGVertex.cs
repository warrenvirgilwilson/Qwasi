using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Qwasi.GraphUI
{
    public enum QGVertexIDType { Sequential = 0, Local = 1, Global = 2 }

    public static class QGVertexExtensions
    {
        public static void SetAsStart(this IQGVertex v) => v.SetVertexStatus("start");
        public static void MarkVertex(this IQGVertex v) => v.SetVertexStatus("marked");

        public static bool IsStartVertex(this IQGVertex v) => IQGVertex.StartVertex == v;
        public static bool IsMarked(this IQGVertex v) => v.VertexStatus == "marked";
    }

    public interface IQGVertex : IQGControl, IInterfaceStateAgent<IQGVertex>
    {
        public static IQGVertex StartVertex { get; internal set; } = null;

        public static readonly VariableIdentifier<IQGVertex> VertexStatusProperty = VariableIdentifier<IQGVertex>.Create("VertexStatus");
        public sealed string VertexStatus
        {
            get { return (string)GetInstance(VertexStatusProperty); }
            private set { SetInstance(VertexStatusProperty, value); }
        }

        public sealed void ClearVertexStatus()
        {
            string prevStatus = this.VertexStatus;
            __ClearVertexStatus();
            this.RaiseVertexStatusChangedEvent(new QGVertexStatusEventArgs(prevStatus, null));
        }

        private void __ClearVertexStatus()
        {
            if (this.IsStartVertex())
                IQGVertex.StartVertex = null;
            this.VertexStatus = null;
        }

        public sealed void SetVertexStatus(string statusName)
        {
            string prevStatus = this.VertexStatus;
            __ClearVertexStatus();
            this.VertexStatus = statusName;

            if (this.VertexStatus == "start")
            {
                IQGVertex.StartVertex?.ClearVertexStatus();
                IQGVertex.StartVertex = this;
            }

            this.RaiseVertexStatusChangedEvent(new QGVertexStatusEventArgs(prevStatus, statusName));
        }

        public static readonly EventIdentifier<IQGVertex> VertexStatusChangedEvent = EventIdentifier<IQGVertex>.Create("VertexStatusChanged");
        protected void RaiseVertexStatusChangedEvent(QGVertexStatusEventArgs e) { RaiseEvent(VertexStatusChangedEvent, e); OnVertexStatusChanged(e); }
        protected void OnVertexStatusChanged(QGVertexStatusEventArgs e) { }
        public event EventHandler<QGVertexStatusEventArgs> VertexStatusChanged
        {
            add { AddEventHandler(VertexStatusChangedEvent, value); }
            remove { RemoveEventHandler(VertexStatusChangedEvent, value); }
        }

        private static readonly VariableIdentifier<IQGVertex> _VertexIDTypeProperty = VariableIdentifier<IQGVertex>.Create("VertexIDType");
        private QGVertexIDType _VertexIDType
        {
            get { return (QGVertexIDType)GetInstance(_VertexIDTypeProperty); }
            set { SetInstance(_VertexIDTypeProperty, value); }
        }

        public QGVertexIDType VertexIDType
        {
            get => _VertexIDType;
            set
            {
                QGVertexIDType prevIDType = _VertexIDType;
                _VertexIDType = value;

                if (prevIDType == QGVertexIDType.Sequential || _VertexIDType == QGVertexIDType.Sequential)
                    (this.ParentLayer as IQGGraphLayer)?.RefreshVertexEnumeration();

                __refreshLabelValue();
            }
        }

        public void RefreshLabel() => this.LabelControl?.Refresh();

        public static readonly VariableIdentifier<IQGVertex> LabelControlProperty = VariableIdentifier<IQGVertex>.Create("LabelControl");
        public QGVertexLabel LabelControl
        {
            get { return (QGVertexLabel)GetInstance(LabelControlProperty); }
            private set { SetInstance(LabelControlProperty, value); }
        }

        protected static readonly VariableIdentifier<IQGVertex> IDObjectProperty = VariableIdentifier<IQGVertex>.Create("IDObject");
        protected object IDObject
        {
            get { return (object)GetInstance(IDObjectProperty); }
            set { SetInstance(IDObjectProperty, value); }
        }

        private void __refreshLabelValue() => this.LabelControl.Content = this.GlobalID;

        public sealed object ID
        {
            get { return this.IDObject; }
            set
            {
                this.IDObject = value == null ? "" : value;

                if (this.LabelControl == null)
                {
                    this.LabelControl = new QGVertexLabel();
                    this.BindingLayer.ChildControls.Add(this.LabelControl);
                }

                __refreshLabelValue();
            }
        }

        public static readonly VariableIdentifier<IQGVertex> GlobalIDPrefixProperty = VariableIdentifier<IQGVertex>.Create("GlobalIDPrefix");
        public string GlobalIDPrefix
        {
            get { return (string)GetInstance(GlobalIDPrefixProperty); }
            set { SetInstance(GlobalIDPrefixProperty, value); __refreshLabelValue(); }
        }

        public string GlobalID => (this.VertexIDType != QGVertexIDType.Global ? this.GlobalIDPrefix : "") + this.ID.ToString();

        public static readonly EventIdentifier<IQGVertex> EdgeCreatedEvent = EventIdentifier<IQGVertex>.Create("EdgeCreated");
        private void RaiseEdgeCreatedEvent(QGVertexEdgeEventArgs e) => RaiseEvent(EdgeCreatedEvent, e);
        protected void OnEdgeCreated(QGVertexEdgeEventArgs e) { }
        public event EventHandler<QGVertexEdgeEventArgs> EdgeCreated
        {
            add { AddEventHandler(EdgeCreatedEvent, value); }
            remove { RemoveEventHandler(EdgeCreatedEvent, value); }
        }

        protected static readonly VariableIdentifier<IQGVertex> GraphEdgeCollectionPropertyProperty = VariableIdentifier<IQGVertex>.Create("GraphEdgeCollection");
        protected ICollection<QGEdge> GraphEdgeCollection
        {
            get { return (ICollection<QGEdge>)GetInstance(GraphEdgeCollectionPropertyProperty); }
            set { SetInstance(GraphEdgeCollectionPropertyProperty, value); }
        }

        public sealed IEnumerable<QGEdge> GraphEdges
        {
            get { return this.GraphEdgeCollection.AsEnumerable(); }
        }

        public sealed QGEdge GetEdge(IQGVertex v2)
        {
            return this.GraphEdges.FirstOrDefault(edge => edge.ContainsVertex(v2));
        }

        public sealed bool ContainsEdge(IQGVertex v2) => this.GraphEdges.Any(item => item.Vertex2 == v2);

        public sealed QGEdge CreateEdge(IQGVertex v2)
        {
            if (v2 == this || v2 == null || this.ContainsEdge(v2))
                throw new Exception("Invalid vertex with which to create edge.");

            QGEdge edge = new QGEdge(this, v2);
            this.GraphEdgeCollection.Add(edge);
            v2.GraphEdgeCollection.Add(edge);

            this.RaiseEdgeCreatedEvent(new QGVertexEdgeEventArgs(edge));

            return edge;
        }

        public sealed QGEdge CreateEdgeIfAbsent(IQGVertex v2)
        {
            if (this.ContainsEdge(v2))
                return this.GetEdge(v2);

            return this.CreateEdge(v2);
        }

        public sealed void RemoveEdge(QGEdge edge)
        {
            if (!this.GraphEdgeCollection.Contains(edge))
                throw new Exception("Cannot remove an edge from a vertex that it is not bound to.");

            this.GraphEdgeCollection.Remove(edge);
        }

        void IInterfaceStateAgent<IQGVertex>.Constructor()
        {
            _VertexIDType = QGVertexIDType.Sequential;
            this.ID = null;
            this.GraphEdgeCollection = new List<QGEdge>();
        }
    }

    public class QGVertexStatusEventArgs : QGControlEventArgs
    {
        public string PreviousStatus { get; } = null;
        public string NewStatus { get; } = null;

        public QGVertexStatusEventArgs(string prevStatus, string newStatus)
        {
            this.PreviousStatus = prevStatus;
            this.NewStatus = newStatus;
        }
    }

    public class QGVertexEdgeEventArgs : QGControlEventArgs
    {
        public QGEdge Edge { get; }

        public QGVertexEdgeEventArgs(QGEdge edge)
        {
            this.Edge = edge;
        }
    }
}
