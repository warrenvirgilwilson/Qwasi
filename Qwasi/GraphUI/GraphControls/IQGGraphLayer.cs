using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Qwasi.HilbertSpaceMath;

namespace Qwasi.GraphUI
{
    public enum QGVertexEnumerationType { Continuous = 0, Subgraph = 1 }

    public interface IQGGraphLayer : IQGLayer, IQGControl, IInterfaceStateAgent<IQGGraphLayer>
    {
        public string SubgraphTypeName => "Graph Layer";

        public static readonly VariableIdentifier<IQGGraphLayer> VertexEnumerationTypeProperty = VariableIdentifier<IQGGraphLayer>.Create("VertexEnumerationType");
        public sealed QGVertexEnumerationType VertexEnumerationType
        {
            get { return (QGVertexEnumerationType)GetInstance(VertexEnumerationTypeProperty, QGVertexEnumerationType.Continuous); }
            set { SetInstance(VertexEnumerationTypeProperty, value); this.RefreshVertexEnumeration(); }
        }

        public static readonly VariableIdentifier<IQGGraphLayer> LayerIDProperty = VariableIdentifier<IQGGraphLayer>.Create("LayerID");
        public sealed string LayerID
        {
            get { return (string)GetInstance(LayerIDProperty, ""); }
            set { SetInstance(LayerIDProperty, value); this.RefreshVertexEnumeration(); }
        }

        public sealed bool IsSubgraph => this.VertexEnumerationType == QGVertexEnumerationType.Subgraph && this.LayerID != "" && this.LayerID != null;

        public static readonly VariableIdentifier<IQGGraphLayer> GlobalIDPrefixProperty
            = VariableIdentifier<IQGGraphLayer>.Create("GlobalIDPrefix");
        public sealed string GlobalIDPrefix
        {
            get { return (string)GetInstance(GlobalIDPrefixProperty, ""); }
            private set { SetInstance(GlobalIDPrefixProperty, value); }
        }

        void IQGLayer.UpdateControlPosition(IQGControl child)
        {
            if (!this.ChildControls.Contains(child))
                throw new Exception("Cannot update control position: control is not a child of the calling layer.");

            Point wpfControlPoint = this.GetRenderingCoordinatesOf(child.Position);

            foreach (WPFElementInfo elementInfo in child.WPFElementInfoEnumeration)
            {
                Point wpfRenderPoint = wpfControlPoint + child.GetAlignmentOffset(elementInfo.Size) + elementInfo.PositionOffset;

                Canvas.SetLeft(elementInfo.UIElement, wpfRenderPoint.X);
                Canvas.SetTop(elementInfo.UIElement, wpfRenderPoint.Y);
            }
        }

        void IQGLayer.UpdateControlSize(IQGControl child)
        {
            this.UpdateControlPosition(child);
        }

        private void __refreshEnumerationCallback(object sender)
        {
            if (this.ParentLayer == null || !(this.ParentLayer is IQGGraphLayer parentGraphLayer))
                this.EnumerateVertices(0, this.GlobalIDPrefix, ":");
            else if (this.IsSubgraph)
                this.EnumerateVertices(0, this.GlobalIDPrefix, ":");
            else
                parentGraphLayer.RefreshVertexEnumeration();
        }

        public sealed void RefreshVertexEnumeration()
        {
            QGUserInterface.PostRefreshMethod(this, __refreshEnumerationCallback);
        }

        protected int EnumerateVertices(int startIndex, string globalIDPrefix, string delimiter)
        {
            if (this.IsSubgraph && startIndex != 0)
                throw new Exception("Vertex enumeration on a subgraph must begin with 0.");

            this.GlobalIDPrefix = globalIDPrefix;

            if (this.IsSubgraph)
                globalIDPrefix += this.LayerID + delimiter;

            int index = startIndex;
            foreach (IQGControl control in this.ChildControls)
            {
                if (control is IQGVertex vertex)
                {
                    vertex.GlobalIDPrefix = globalIDPrefix;
                    if (vertex.VertexIDType == QGVertexIDType.Sequential)
                        vertex.ID = index++;
                }
                else if (control is IQGGraphLayer layer)
                {
                    if (layer.IsSubgraph)
                        layer.EnumerateVertices(0, globalIDPrefix, delimiter);
                    else
                        index = layer.EnumerateVertices(index, globalIDPrefix, delimiter);
                }
            }

            return index;
        }

        Vector IQGLayer.WPFOriginDisplacement()
        {
            switch (this.OriginAlignment)
            {
                case QGCoordinateOriginAlignment.TopLeft:
                    return new Vector(0, 0);
                case QGCoordinateOriginAlignment.Center:
                    return new Vector(this.Width / 2, this.Height / 2);
                default:
                    break;
            }

            throw new Exception("Unsupported option.");
        }

        private void __initializeWPFElement(object caller, UIElement wpfElement)
        {
            ((Canvas)this.WPFPrimaryElement).Children.Add(wpfElement);
        }

        void IQGLayer.OnAddingControl(IQGControl control)
        {
            control.SetParentLayer(this);

            if (control is IWPFContainer wpfContainer)
                wpfContainer.RegisterWPFElementInitMethod(__initializeWPFElement);
            this.UpdateControlPosition(control);

            if (control is IQGVertex v && v.VertexIDType == QGVertexIDType.Sequential)
                this.RefreshVertexEnumeration();

            if (control is IQGGraphLayer gl)
                gl.RefreshVertexEnumeration();

            this.OnAddingControl(control);
        }

        void IQGLayer.OnRemovingControl(IQGControl control)
        {
            Canvas layerCanvas = this.WPFPrimaryElement as Canvas;

            foreach (UIElement wpfElement in control.WPFElements)
            {
                if (!layerCanvas.Children.Contains(wpfElement))
                    throw new Exception("Cannot remove control from graph layer: it is not present within the layer.");

                layerCanvas.Children.Remove(wpfElement);
            }

            control.SetParentLayer(null);
            if ((control is IQGVertex v && v.VertexIDType == QGVertexIDType.Sequential) ||
                control is IQGGraphLayer)
                this.RefreshVertexEnumeration();

            this.OnRemovingControl(control);
        }

        protected new virtual void OnAddingControl(IQGControl control) { }
        protected new virtual void OnRemovingControl(IQGControl control) { }
    }

    public class QGGraphLayer : QGControl, IQGGraphLayer, IQGSelectable
    {
        InterfaceStateCatalog<IQGSelectable> IInterfaceStateAgent<IQGSelectable>.InterfaceStateCatalog { get; set; }
        InterfaceStateCatalog<IQGGraphLayer> IInterfaceStateAgent<IQGGraphLayer>.InterfaceStateCatalog { get; set; }

        protected WPFGraphLayerCanvas LayerCanvas => this.WPFPrimaryElement as WPFGraphLayerCanvas;

        public QGControlCollection ChildControls { get; }
        public IEnumerable<IQGControl> AllDescendentControls => ((IQGLayer)this).AllDescendentControls;

        public QGVertexEnumerationType VertexEnumerationType
        {
            get => ((IQGGraphLayer)this).VertexEnumerationType;
            set => ((IQGGraphLayer)this).VertexEnumerationType = value;
        }

        public static readonly VariableIdentifier<IQGGraphLayer> LayerIDProperty = VariableIdentifier<IQGGraphLayer>.Create("LayerID");
        public string LayerID => ((IQGGraphLayer)this).LayerID;

        public bool IsSubgraph => ((IQGGraphLayer)this).IsSubgraph;

        public void RefreshVertexEnumeration() => ((IQGGraphLayer)this).RefreshVertexEnumeration();

        protected void SetSize(Size size) => this.SetSize(size.Width, size.Height);
        protected virtual void SetSize(double width, double height)
        {
            this.LayerCanvas.Width = width;
            this.LayerCanvas.Height = height;
        }

        public QGCoordinateOriginAlignment OriginAlignment { get; set; } = QGCoordinateOriginAlignment.Center;

        void IQGGraphLayer.OnAddingControl(IQGControl control) => this.OnAddingControl(control);
        void IQGGraphLayer.OnRemovingControl(IQGControl control) => this.OnRemovingControl(control);

        protected virtual void OnAddingControl(IQGControl control) { }
        protected virtual void OnRemovingControl(IQGControl control) { }

        protected override void OnSizeChange(QGSizeChangeEventArgs e)
        {
            foreach (IQGControl c in this.ChildControls)
                ((IQGLayer)this).UpdateControlPosition(c);

            base.OnSizeChange(e);
        }

        public event EventHandler<QGSelectionEventArgs> SelectionGained;
        public event EventHandler<QGSelectionEventArgs> SelectionLost;

        protected virtual void OnSelectionGained(QGSelectionEventArgs e)
        {
            this.LayerCanvas.Background = new SolidColorBrush(Color.FromArgb(15, 0, 0, 255));
            this.SelectionGained?.Invoke(this, e);
        }

        protected virtual void OnSelectionLost(QGSelectionEventArgs e)
        {
            this.LayerCanvas.Background = null;
            this.SelectionLost?.Invoke(this, e);
        }

        void IQGSelectable.RaiseSelectionGainedEvent(QGSelectionEventArgs e) => this.OnSelectionGained(e);
        void IQGSelectable.RaiseSelectionLostEvent(QGSelectionEventArgs e) => this.OnSelectionLost(e);

        public bool UserSelectable
        {
            get => ((IQGSelectable)this).UserSelectable;
            set => ((IQGSelectable)this).UserSelectable = value;
        }

        protected override void OnDeleting(QGControlEventArgs e)
        {
            QGUserInterface.LockGlobalRefresh(this);

            base.OnDeleting(e);

            QGUserInterface.UnlockGlobalRefresh(this);
        }

        public QGGraphLayer()
        {
            this.ChildControls = new QGControlCollection(this);

            WPFGraphLayerCanvas canvas = new WPFGraphLayerCanvas();
            this.RegisterWPFElement(canvas);
            this.SetWPFPrimaryElement(canvas);

            this.LayerCanvas.Width = 300;
            this.LayerCanvas.Height = 300;
        }
    }

    public class WPFGraphLayerCanvas : Canvas
    {
    }

    public class WPFCanvasChildEventArgs : EventArgs
    {
        public DependencyObject Child { get; }

        public WPFCanvasChildEventArgs(DependencyObject child)
        {
            this.Child = child;
        }
    }
}
