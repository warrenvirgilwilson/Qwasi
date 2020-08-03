using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Qwasi.GraphUI
{
    [XmlRoot("Vertex")]
    public class QGVertexControl : QGVertexControlBase, IQGVertex
    {
        InterfaceStateCatalog<IQGVertex> IInterfaceStateAgent<IQGVertex>.InterfaceStateCatalog { get; set; }

        void IQGVertex.OnVertexStatusChanged(QGVertexStatusEventArgs e)
        {
            if (e.NewStatus == null)
                this.ReleaseColorOverride();
            else if (e.NewStatus == "start")
                this.SetColorOverride(Brushes.Yellow, Brushes.LightYellow);
            else if (e.NewStatus == "marked")
                this.SetColorOverride(Brushes.Red, Brushes.LightSalmon);
        }

        public IEnumerable<QGEdge> GraphEdges => ((IQGVertex)this).GraphEdges;
        public QGEdge GetEdge(IQGVertex v2) => ((IQGVertex)this).GetEdge(v2);
        public bool ContainsEdge(IQGVertex v2) => ((IQGVertex)this).ContainsEdge(v2);
        public QGEdge CreateEdge(IQGVertex v2) => ((IQGVertex)this).CreateEdge(v2);
        public QGEdge CreateEdgeIfAbsent(IQGVertex v2) => ((IQGVertex)this).CreateEdgeIfAbsent(v2);

        protected override void OnDeleting(QGControlEventArgs e)
        {
            QGUserInterface.LockGlobalRefresh(this);

            foreach (QGEdge edge in this.GraphEdges.ToArray())
                edge.DeleteControl();

            base.OnDeleting(e);

            QGUserInterface.UnlockGlobalRefresh(this);
        }
    }
}
