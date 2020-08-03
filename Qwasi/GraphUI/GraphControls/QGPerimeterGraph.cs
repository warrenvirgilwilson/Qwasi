using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Qwasi.GraphUI
{
    public abstract class QGPerimeterGraph : QGGraphLayer
    {
        public double RotationAngle { get; set; }
        public double GraphRadius
        {
            get { return this.Width / 2; }
            set { this.SetSize(value * 2, value * 2); }
        }

        protected abstract QGInteractiveControl CenterControl { get; }

        protected IList<QGVertexControl> PerimeterVertexList { get; private set; }
        public IEnumerable<QGVertexControl> PerimeterVertices
        {
            get { return this.PerimeterVertexList.AsEnumerable(); }
        }

        public override Size RenderSize => new Size(base.RenderSize.Width + 40, base.RenderSize.Height + 40);

        public int NodeCount => this.PerimeterVertexList?.Count ?? 0;

        protected void RefreshVertices() => QGUserInterface.PostRefreshMethod(this, __refreshVerticesCallback);
        private void __refreshVerticesCallback(object sender)
        {
            double separationAngle = (2 * Math.PI) / this.NodeCount;
            for (int i = 0; i < this.PerimeterVertexList.Count; i++)
            {
                double nodeAngle = this.RotationAngle + (i * separationAngle);
                this.PerimeterVertexList[i].SetPosition(
                    this.GraphRadius * Math.Cos(nodeAngle),
                    this.GraphRadius * Math.Sin(nodeAngle));
            }
        }

        public void RefreshVertexConnections() => QGUserInterface.PostRefreshMethod(this, __refreshConnectionsCallback);
        private void __refreshConnectionsCallback(object sender) => this.GenerateVertexConnections();
        protected abstract void GenerateVertexConnections();

        public void AddPerimeterVertices(int vertexCount)
        {
            QGVertexControl[] newVertices = new QGVertexControl[vertexCount];
            for (int i = 0; i < vertexCount; i++)
                newVertices[i] = new QGVertexControl();

            this.AddPerimeterVertices(newVertices);
        }

        public void AddPerimeterVertex(QGVertexControl vertex) => this.AddPerimeterVertices(new QGVertexControl[] { vertex });
        public virtual void AddPerimeterVertices(IEnumerable<QGVertexControl> vertices)
        {
            this.PerimeterVertexList ??= new List<QGVertexControl>(vertices.Count());

            QGUserInterface.LockGlobalRefresh(this);

            foreach (QGVertexControl vc in vertices)
            {
                vc.SetDraggability(true);

                vc.SetDragMobility(true, QGDragParameterType.Active);
                vc.AddDragStartEventHandler(__NodeDragStart, QGDragParameterType.Active);
                vc.AddDragMoveEventHandler(__NodeDragMove, QGDragParameterType.Active);
                vc.AddDragStopEventHandler(__NodeDragStop, QGDragParameterType.Active);

                vc.SetDragMobility(false, QGDragParameterType.Passive);
                vc.SetDragMobility(false, QGDragParameterType.Proxy);
                vc.SetProxyTargets(new IQGControl[] { this }, QGDragParameterType.Passive);
                vc.SetProxyTargets(new IQGControl[] { this }, QGDragParameterType.Proxy);

                vc.Deleting += __deletePerimeterVertex;

                this.PerimeterVertexList.Add(vc);
                this.ChildControls.Add(vc);
            }

            if (this.IsInitialized)
            {
                this.RefreshVertexConnections();
                this.RefreshVertices();
            }

            QGUserInterface.UnlockGlobalRefresh(this);
        }

        public void RemovePerimeterVertex(QGVertexControl vertex) => this.RemovePerimeterVertices(new QGVertexControl[] { vertex });
        public virtual void RemovePerimeterVertices(IEnumerable<QGVertexControl> vertices)
        {
            foreach (QGVertexControl vc in vertices)
                this.PerimeterVertexList.Remove(vc);

            this.RefreshVertexConnections();
            this.RefreshVertices();
        }

        protected override void OnInitialized(QGControlEventArgs e)
        {
            base.OnInitialized(e);

            this.RefreshVertexConnections();
            this.RefreshVertices();
        }

        public QGPerimeterGraph()
            : base()
        {
            this.CenterControl.SetPosition(0, 0);
            this.CenterControl.SetDragMobility(false);
            this.CenterControl.SetProxyTargets(new IQGControl[] { this }, QGDragParameterType.All);
            this.CenterControl.Deleting += (o, e) => this.DeleteControl();
            this.ChildControls.Add(this.CenterControl);

            this.HorizontalAlignment = QGHorizontalAlignment.Center;
            this.VerticalAlignment = QGVerticalAlignment.Center;
        }

        public QGPerimeterGraph(int perimeterVertexCount)
            : this()
        {
            this.AddPerimeterVertices(perimeterVertexCount);
        }

        private void __deletePerimeterVertex(object sender, QGControlEventArgs e)
        {
            this.RemovePerimeterVertex((QGVertexControl)sender);

            if (this.NodeCount <= 1)
            {
                this.DeleteControl();
                return;
            }
        }

        protected override void OnDeleting(QGControlEventArgs e)
        {
            foreach (QGVertexControl vc in this.PerimeterVertices.ToArray())
                vc.DeleteControl();
            this.CenterControl.DeleteControl();

            base.OnDeleting(e);
        }

        double _rotationAngleStart = 0;
        private void __NodeDragStart(object sender, QGDragEventArgs e)
        {
            _rotationAngleStart = this.RotationAngle;
        }

        private void __NodeDragMove(object sender, QGDragEventArgs e)
        {
            this.GraphRadius = ((Vector)e.CurrentPosition).Length;
            this.RotationAngle = _rotationAngleStart + (
                (2 * Math.PI) * (Vector.AngleBetween((Vector)e.DragOrigin, (Vector)e.CurrentPosition) / 360));
            this.RefreshVertices();
        }

        private void __NodeDragStop(object sender, QGDragEventArgs e)
        {
            _rotationAngleStart = 0;
        }
    }
}
