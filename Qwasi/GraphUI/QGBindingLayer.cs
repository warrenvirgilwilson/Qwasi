using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Qwasi.GraphUI
{
    public class QGBindingLayer : IQGLayer
    {
        public IQGControl ParentControl { get; }

        public QGControlCollection ChildControls { get; }
        public QGCoordinateOriginAlignment OriginAlignment { get; set; } = QGCoordinateOriginAlignment.Center;

        protected AdornerLayer WPFAdornerLayer { get; private set; }
        protected IDictionary<IQGControl, QGControlAdorner> ControlAdorners { get; } = new Dictionary<IQGControl, QGControlAdorner>();

        Vector IQGLayer.WPFOriginDisplacement()
        {
            switch (this.OriginAlignment)
            {
                case QGCoordinateOriginAlignment.TopLeft:
                    return new Vector(0, 0);
                case QGCoordinateOriginAlignment.Center:
                    return new Vector(this.ParentControl.Width / 2, this.ParentControl.Height / 2);
                default:
                    break;
            }

            throw new Exception("Unsupported option.");
        }

        void IQGLayer.UpdateControlPosition(IQGControl child) => this.UpdateControlPosition(child);
        public void UpdateControlPosition(IQGControl child)
        {
            if (!this.ChildControls.Contains(child))
                throw new Exception("Cannot update control position: control is not a child of the calling layer.");

            Point wpfControlPoint = ((IQGLayer)this).GetRenderingCoordinatesOf(child.Position);

            foreach (WPFElementInfo elementInfo in child.WPFElementInfoEnumeration)
            {
                Point wpfRenderPoint = wpfControlPoint + child.GetAlignmentOffset(elementInfo.Size) + elementInfo.PositionOffset;
                Rect wpfDimensions = new Rect(wpfRenderPoint, elementInfo.Size);

                if (!Double.IsFinite(elementInfo.Size.Width) || !Double.IsFinite(elementInfo.Size.Height))
                    break;

                elementInfo.UIElement.Arrange(wpfDimensions);
            }
        }

        void IQGLayer.UpdateControlSize(IQGControl child)
        {
            this.UpdateControlPosition(child);
        }

        private void __initializeWPFElement(object caller, UIElement wpfElement)
        {
            IQGControl control = (IQGControl)caller;

            QGControlAdorner adorner = this.ControlAdorners[control];
            adorner.AdornerVisualCollection.Add(wpfElement);
        }

        void IQGLayer.OnAddingControl(IQGControl control)
        {
            control.SetParentLayer(this);

            QGControlAdorner adorner = new QGControlAdorner(this.ParentControl);
            this.ControlAdorners.Add(control, adorner);
            this.WPFAdornerLayer?.Add(adorner);

            if (control is IWPFContainer wpfContainer)
                wpfContainer.RegisterWPFElementInitMethod(__initializeWPFElement);
            this.UpdateControlPosition(control);

            this.OnAddingControl(control);
        }

        void IQGLayer.OnRemovingControl(IQGControl control)
        {
            if (!this.ControlAdorners.ContainsKey(control))
                throw new Exception("Cannot remove control from binding layer: it is not present within the layer.");

            control.SetParentLayer(null);
            QGControlAdorner adorner = this.ControlAdorners[control];
            this.ControlAdorners.Remove(control);
            this.WPFAdornerLayer?.Remove(adorner);

            this.OnRemovingControl(control);
        }

        protected virtual void OnAddingControl(IQGControl control) { }
        protected virtual void OnRemovingControl(IQGControl control) { }

        public bool IsInitialized { get; private set; } = false;
        private void __initialize(UIElement wpfAdornedElement)
        {
            this.WPFAdornerLayer = AdornerLayer.GetAdornerLayer(wpfAdornedElement);
            foreach (QGControlAdorner adorner in this.ControlAdorners.Values)
                this.WPFAdornerLayer.Add(adorner);

            this.IsInitialized = true;
        }

        public void DeleteBindingLayer()
        {
            this.ChildControls.Clear();

        }

        public QGBindingLayer(IQGControl parentControl)
        {
            this.ChildControls = new QGControlCollection(this);

            this.ParentControl = parentControl;
            this.ParentControl.RegisterControlInitMethod(c => __initialize(this.ParentControl.WPFPrimaryElement));
        }
    }

    public class QGControlAdorner : Adorner
    {
        public IQGControl ParentControl { get; }

        public VisualCollection AdornerVisualCollection { get; private set; }

        protected override int VisualChildrenCount
        {
            get { return this.AdornerVisualCollection.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.AdornerVisualCollection[index];
        }

        public QGControlAdorner(IQGControl parentControl)
            : base(parentControl.WPFPrimaryElement)
        {
            this.ParentControl = parentControl;
            this.AdornerVisualCollection = new VisualCollection(this);
        }
    }
}
