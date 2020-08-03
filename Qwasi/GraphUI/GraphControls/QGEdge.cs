using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
using Qwasi.GraphUI;

namespace Qwasi.GraphUI
{
    public class QGEdge : QGInteractiveControl
    {
        private IEnumerable<IQGControl> _ProxyTargets
        {
            get
            {
                yield return this.Vertex1;
                yield return this.Vertex2;
            }
        }

        public override void SetPosition(Point p) { }
        public override Size Size => new Size(0, 0);

        public IQGVertex Vertex1 { get; private set; }
        public IQGVertex Vertex2 { get; private set; }

        public bool ContainsVertex(IQGVertex v)
        {
            return v == this.Vertex1 || v == this.Vertex2;
        }

        public override Point Position => this.GetEdgeMidpoint() - this.ParentLayer.WPFOriginDisplacement();
        public Point GetEdgeMidpoint()
        {
            Point p1 = this.ParentLayer.GetLocalCoordinatesOf(Vertex1);
            Point p2 = this.ParentLayer.GetLocalCoordinatesOf(Vertex2);

            p1 = this.ParentLayer.GetRenderingCoordinatesOf(p1);
            p2 = this.ParentLayer.GetRenderingCoordinatesOf(p2);

            return (Point)(((Vector)p1 + (Vector)p2) / 2);
        }

        public Line WPFSurfaceLine { get; private set; }
        public Line WPFEdgeLine { get; private set; }

        private void __refresh()
        {
            this.WPFEdgeLine.StrokeThickness = _isHighlighted ? this.CurrentHighlightThickness : this.CurrentLineThickness;
            this.WPFEdgeLine.Stroke = _isHighlighted ? this.CurrentHighlightBrush : this.CurrentLineBrush;
        }

        private double _LineThickness = 1.75;
        public double LineThickness
        {
            get { return _LineThickness; }
            set { _LineThickness = value; __refresh(); }
        }

        private double _SelectedLineThickness = 3.5;
        public double SelectedLineThickness
        {
            get { return _SelectedLineThickness; }
            set { _SelectedLineThickness = value; __refresh(); }
        }

        public double CurrentLineThickness => this.IsSelected ? this.SelectedLineThickness : this.LineThickness;

        private double _HighlightThickness = 3.5;
        public double HighlightThickness
        {
            get { return _HighlightThickness; }
            set { _HighlightThickness = value; __refresh(); }
        }

        private double _SelectedHighlightThickness = 3.5;
        public double SelectedHighlightThickness
        {
            get { return _SelectedHighlightThickness; }
            set { _SelectedHighlightThickness = value; __refresh(); }
        }

        public double CurrentHighlightThickness => this.IsSelected ? this.SelectedHighlightThickness : this.HighlightThickness;

        public double SurfaceThickness
        {
            get { return WPFSurfaceLine.StrokeThickness; }
            set { WPFSurfaceLine.StrokeThickness = value; }
        }

        private Brush _LineBrush = Brushes.DarkBlue;
        public Brush LineBrush
        {
            get { return _LineBrush; }
            set { _LineBrush = value; __refresh(); }
        }

        private Brush _SelectedLineBrush = Brushes.DeepSkyBlue;
        public Brush SelectedLineBrush
        {
            get { return _SelectedLineBrush; }
            set { _SelectedLineBrush = value; __refresh(); }
        }

        public Brush CurrentLineBrush => this.IsSelected ? this.SelectedLineBrush : this.LineBrush;

        private Brush _HighlightBrush = Brushes.DarkBlue;
        public Brush HighlightBrush
        {
            get { return _HighlightBrush; }
            set { _HighlightBrush = value; __refresh(); }
        }

        private Brush _SelectedHighlightBrush = Brushes.DeepSkyBlue;
        public Brush SelectedHighlightBrush
        {
            get { return _SelectedHighlightBrush; }
            set { _SelectedHighlightBrush = value; __refresh(); }
        }

        public Brush CurrentHighlightBrush => this.IsSelected ? this.SelectedHighlightBrush : this.HighlightBrush;

        public Vector GetEdgeVector(IQGVertex pointingTo)
        {
            if (this.ParentLayer == null)
                return default(Vector);

            if (pointingTo != Vertex1 && pointingTo != Vertex2)
                throw new Exception("Nope.");

            IQGVertex vTo = pointingTo == this.Vertex1 ? this.Vertex1 : this.Vertex2;
            IQGVertex vFrom = pointingTo == this.Vertex1 ? this.Vertex2 : this.Vertex1;

            Point pTo = this.ParentLayer.GetLocalCoordinatesOf(vTo);
            Point pFrom = this.ParentLayer.GetLocalCoordinatesOf(vFrom);

            return pTo - pFrom;
        }

        public IQGVertex OtherVertex(IQGVertex v)
        {
            if (v == this.Vertex1)
                return this.Vertex2;
            if (v == this.Vertex2)
                return this.Vertex1;

            throw new Exception("Vertex v is not present in the edge.");
        }

        public void __VertexDragStart(object sender, QGDragEventArgs e)
        {
            this.WPFEdgeLine.StrokeDashArray = new DoubleCollection { 2, 1 };
        }

        public void __VertexDragStop(object sender, QGDragEventArgs e)
        {
            this.WPFEdgeLine.StrokeDashArray = null;
        }

        private Point _p1 = default(Point);
        private Point _p2 = default(Point);

        public event EventHandler<QGEdgeEventArgs> Refreshed;
        protected virtual void OnRefreshed(QGEdgeEventArgs e) => this.Refreshed?.Invoke(this, e);
        private void RaiseRefreshedEvent() => this.OnRefreshed(new QGEdgeEventArgs());

        public virtual void Refresh()
        {
            if (!this.IsInitialized)
                return;

            Point p1 = this.ParentLayer.GetLocalCoordinatesOf(this.Vertex1);
            Point p2 = this.ParentLayer.GetLocalCoordinatesOf(this.Vertex2);

            if (p1 == _p1 && p2 == _p2)
                return;

            _p1 = p1;
            _p2 = p2;

            Vector p1top2 = _p2 - _p1;

            if (p1top2.Length == 0)
                return;

            p1top2.Normalize();
            p1top2 *= 7;

            Point wpfP1 = this.ParentLayer.GetRenderingCoordinatesOf(p1);
            Point wpfP2 = this.ParentLayer.GetRenderingCoordinatesOf(p2);

            WPFSurfaceLine.X1 = this.WPFEdgeLine.X1 = wpfP1.X + p1top2.X;
            WPFSurfaceLine.Y1 = this.WPFEdgeLine.Y1 = wpfP1.Y + p1top2.Y;

            WPFSurfaceLine.X2 = this.WPFEdgeLine.X2 = wpfP2.X - p1top2.X;
            WPFSurfaceLine.Y2 = this.WPFEdgeLine.Y2 = wpfP2.Y - p1top2.Y;

            this.Vertex1.RefreshLabel();
            this.Vertex2.RefreshLabel();
            this.RaiseRefreshedEvent();
        }

        private bool _isHighlighted = false;
        public void ApplyHighlight()
        {
            _isHighlighted = true;
            __refresh();
        }

        public void RemoveHighlight()
        {
            _isHighlighted = false;
            __refresh();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            this.ApplyHighlight();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.RemoveHighlight();
            base.OnMouseLeave(e);
        }

        protected override void OnSelectionGained(QGSelectionEventArgs e)
        {
            __refresh();
            base.OnSelectionGained(e);
        }

        protected override void OnSelectionLost(QGSelectionEventArgs e)
        {
            __refresh();
            base.OnSelectionLost(e);
        }

        private bool _inColorOverride = false;
        private Brush _prevLineBrush, _prevHighlightBrush, _prevSelectedLineBrush, _prevSelectedHighlightBrush;
        public void SetColorOverride(Brush lineBrush, Brush highlightBrush, Brush selectedLineBrush, Brush selectedHighlightBrush)
        {
            if (!_inColorOverride)
            {
                _prevLineBrush = this.LineBrush;
                _prevHighlightBrush = this.HighlightBrush;
                _prevSelectedLineBrush = this.SelectedLineBrush;
                _prevSelectedHighlightBrush = this.SelectedHighlightBrush;
            }

            _inColorOverride = true;

            this.LineBrush = lineBrush ?? this.LineBrush;
            this.HighlightBrush = highlightBrush ?? this.HighlightBrush;
            this.SelectedLineBrush = selectedLineBrush ?? this.SelectedLineBrush;
            this.SelectedHighlightBrush = selectedHighlightBrush ?? this.SelectedHighlightBrush;
        }

        public void ReleaseColorOverride()
        {
            if (!_inColorOverride)
                return;

            this.LineBrush = _prevLineBrush;
            this.HighlightBrush = _prevHighlightBrush;
            this.SelectedLineBrush = _prevSelectedLineBrush;
            this.SelectedHighlightBrush = _prevSelectedHighlightBrush;

            _inColorOverride = false;
        }

        private void __positionChange(object sender, QGPositionChangeEventArgs e)
        {
            this.Refresh();
        }

        private void __registerParent(IQGControl sender)
        {
            if (this.ParentLayer == null && this.Vertex1.IsInitialized && this.Vertex2.IsInitialized)
                IQGLayer.FindNearestCommonLayer(this.Vertex1, this.Vertex2).ChildControls.Add(this);
        }

        public QGEdge(IQGVertex v1, IQGVertex v2)
            : base()
        {
            this.Vertex1 = v1;
            this.Vertex2 = v2;

            this.WPFSurfaceLine = new Line();
            this.SurfaceThickness = 15;
            this.WPFSurfaceLine.Stroke = Brushes.Transparent;

            this.WPFEdgeLine = new Line();
            this.WPFEdgeLine.StrokeThickness = this.LineThickness;
            this.WPFEdgeLine.Stroke = Brushes.DarkBlue;

            this.RegisterWPFElement(this.WPFEdgeLine);
            this.RegisterWPFElement(this.WPFSurfaceLine);
            this.SetWPFPrimaryElement(this.WPFSurfaceLine);

            ///////
            ((IQGDraggable)this.Vertex1).AddDragStartEventHandler(__VertexDragStart, QGDragParameterType.All);
            ((IQGDraggable)this.Vertex2).AddDragStartEventHandler(__VertexDragStart, QGDragParameterType.All);

            ((IQGDraggable)this.Vertex1).AddDragStopEventHandler(__VertexDragStop, QGDragParameterType.All);
            ((IQGDraggable)this.Vertex2).AddDragStopEventHandler(__VertexDragStop, QGDragParameterType.All);

            this.Vertex1.PositionChange += __positionChange;
            this.Vertex2.PositionChange += __positionChange;

            this.Vertex1.RegisterControlInitMethod(__registerParent);
            this.Vertex2.RegisterControlInitMethod(__registerParent);

            ((IQGDraggable)this).SetDraggability(true, QGDragParameterType.Active);
            ((IQGDraggable)this).SetDraggability(false, QGDragParameterType.Passive);
            ((IQGDraggable)this).SetDraggability(false, QGDragParameterType.Proxy);

            ((IQGDraggable)this).SetDragMobility(false);

            this.SetProxyTargets(_ProxyTargets, QGDragParameterType.Active);
            //this.UserSelectable = true;
        }

        protected override void OnInitialized(QGControlEventArgs e)
        {
            foreach (IQGLayer layer in Vertex1.ParentLayer.AncestorEnumeration.TakeWhile(l => l != this.ParentLayer))
                if (layer is IQGControl asControl)
                    asControl.PositionChange += __positionChange;

            foreach (IQGLayer layer in Vertex2.ParentLayer.AncestorEnumeration.TakeWhile(l => l != this.ParentLayer))
                if (layer is IQGControl asControl)
                    asControl.PositionChange += __positionChange;

            this.Refresh();
            base.OnInitialized(e);
        }

        protected override void OnDeleting(QGControlEventArgs e)
        {
            ((IQGDraggable)this.Vertex1).RemoveDragStartEventHandler(__VertexDragStart, QGDragParameterType.All);
            ((IQGDraggable)this.Vertex2).RemoveDragStartEventHandler(__VertexDragStart, QGDragParameterType.All);

            ((IQGDraggable)this.Vertex1).RemoveDragStopEventHandler(__VertexDragStop, QGDragParameterType.All);
            ((IQGDraggable)this.Vertex2).RemoveDragStopEventHandler(__VertexDragStop, QGDragParameterType.All);

            this.Vertex1.PositionChange -= __positionChange;
            this.Vertex2.PositionChange -= __positionChange;

            foreach (IQGLayer layer in Vertex1.ParentLayer.AncestorEnumeration.TakeWhile(l => l != this.ParentLayer))
                if (layer is IQGControl asControl)
                    asControl.PositionChange -= __positionChange;

            foreach (IQGLayer layer in Vertex2.ParentLayer.AncestorEnumeration.TakeWhile(l => l != this.ParentLayer))
                if (layer is IQGControl asControl)
                    asControl.PositionChange -= __positionChange;

            this.Vertex1.RemoveEdge(this);
            this.Vertex2.RemoveEdge(this);

            base.OnDeleting(e);
        }
    }

    public class QGEdgeEventArgs : QGControlEventArgs
    {
    }
}