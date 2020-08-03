using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Qwasi.GraphUI
{
    public static class QGCompoundLayerExtensions
    {
        public static double GetRightEdge(this IQGControl control)
        {
            return control.X + (control.RenderSize.Width + control.GetAlignmentOffset().X);
        }

        public static double GetBottomEdge(this IQGControl control)
        {
            return control.Y + (control.RenderSize.Height + control.GetAlignmentOffset().Y);
        }
    }

    public class QGRootLayer : QGGraphLayer, IQGMouseHandler
    {
        private double _rightEdge = 0;
        private double _bottomEdge = 0;
        protected IQGControl RightmostControl { get; set; } = null;
        protected IQGControl BottommostControl { get; set; } = null;

        protected AdornerLayer WPFCanvasAdornerLayer { get; private set; }
        protected WPFSelectionRectangleAdorner WPFSelectionRectangleAdorner { get; private set; }
        protected Rectangle WPFSelectionRectangle { get; }

        bool IQGMouseHandler.ShouldCaptureMouse() => QGInput.ClickedElements.Count() == 1;

        private Point _mouseDownCoordinates;
        private HashSet<IQGSelectable> _priorSelectedItems = new HashSet<IQGSelectable>();
        void IQGMouseHandler.OnMouseLeftButtonDown(object o, MouseButtonEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
                IQGSelectable.ClearSelectedElements();

            _mouseDownCoordinates = e.GetPosition(this.LayerCanvas);
            Rect bounds = new Rect(_mouseDownCoordinates, _mouseDownCoordinates);

            // Remove and re-add the adorner to keep it in front
            this.WPFCanvasAdornerLayer.Remove(this.WPFSelectionRectangleAdorner);
            this.WPFCanvasAdornerLayer.Add(this.WPFSelectionRectangleAdorner);
            this.WPFSelectionRectangle.Visibility = Visibility.Visible;
            this.WPFSelectionRectangle.Arrange(bounds);

            _priorSelectedItems = new HashSet<IQGSelectable>(IQGSelectable.SelectedElements);
        }

        void IQGMouseHandler.OnMouseLeftButtonUp(object o, MouseButtonEventArgs e)
        {
            this.WPFSelectionRectangle.Visibility = Visibility.Collapsed;
        }

        void IQGMouseHandler.OnMouseLeftButtonMove(object o, MouseEventArgs e)
        {
            Point currentMousePosition = e.GetPosition(this.LayerCanvas);
            Rect bounds = new Rect(_mouseDownCoordinates, currentMousePosition);
            this.WPFSelectionRectangle.Arrange(bounds);

            foreach (IQGControl c in this.AllDescendentControls.OfType<IQGSelectable>().Where(s => s.UserSelectable))
            {
                Point p = ((IQGLayer)this).GetLocalCoordinatesOf(c);
                if (p.X > bounds.Left && p.X < bounds.Right && p.Y > bounds.Top && p.Y < bounds.Bottom)
                    ((IQGSelectable)c).Select();
                else if (!_priorSelectedItems.Contains(c as IQGSelectable))
                    ((IQGSelectable)c).Unselect();
            }
        }

        private IQGControl __selectControl(IEnumerable<IQGControl> controls, Func<IQGControl, IQGControl, IQGControl> selector)
        {
            if (controls == null || !controls.Any())
                return null;

            IQGControl currentSelection = controls.First();
            foreach (IQGControl item in controls)
                currentSelection = selector(currentSelection, item);

            return currentSelection;
        }

        private void __UpdateBoundariesAfterElementRemoval(IQGControl control)
        {
            if (this.RightmostControl == control)
            {
                this.RightmostControl = __selectControl(this.ChildControls, (c1, c2) => c1.GetRightEdge() > c2.GetRightEdge() ? c1 : c2);
                _rightEdge = this.RightmostControl?.GetRightEdge() ?? 0;
            }

            if (this.BottommostControl == control)
            {
                this.BottommostControl = __selectControl(this.ChildControls, (c1, c2) => c1.GetBottomEdge() > c2.GetBottomEdge() ? c1 : c2);
                _bottomEdge = this.BottommostControl?.GetBottomEdge() ?? 0;
            }
        }

        private void __UpdateLayerBoundaries(IQGControl control)
        {
            this.RightmostControl ??= control;
            this.BottommostControl ??= control;

            double elXedge = control.GetRightEdge();
            double elYedge = control.GetBottomEdge();

            if (elXedge > _rightEdge)
            {
                this.RightmostControl = control;
                _rightEdge = elXedge;
            }
            else if (elXedge < _rightEdge && this.RightmostControl == control)
            {
                this.RightmostControl = __selectControl(this.ChildControls, (c1, c2) => c1.GetRightEdge() > c2.GetRightEdge() ? c1 : c2);
                _rightEdge = this.RightmostControl.GetRightEdge();
            }

            if (elYedge > _bottomEdge)
            {
                this.BottommostControl = control;
                _bottomEdge = elYedge;
            }
            else if (elYedge < _bottomEdge && this.BottommostControl == control)
            {
                this.BottommostControl = __selectControl(this.ChildControls, (c1, c2) => c1.GetBottomEdge() > c2.GetBottomEdge() ? c1 : c2);
                _bottomEdge = this.BottommostControl.GetBottomEdge();
            }

            if (this.Width != _rightEdge || this.Height != _bottomEdge)
                this.SetSize(_rightEdge, _bottomEdge);
        }

        private void __updateControlOnMove(object sender, QGPositionChangeEventArgs e)
        {
            __UpdateLayerBoundaries((IQGControl)sender);
        }

        private void __updateControlOnResize(object sender, QGSizeChangeEventArgs e)
        {
            __UpdateLayerBoundaries((IQGControl)sender);
        }

        protected override void OnAddingControl(IQGControl control)
        {
            __UpdateLayerBoundaries(control);
            control.PositionChange += __updateControlOnMove;
            control.SizeChange += __updateControlOnResize;
        }

        protected override void OnRemovingControl(IQGControl control)
        {
            control.PositionChange -= __updateControlOnMove;
            control.SizeChange -= __updateControlOnResize;
            __UpdateBoundariesAfterElementRemoval(control);
        }

        public QGRootLayer()
            : base()
        {
            ((IQGMouseHandler)this).InitializeInterface();

            this.LayerCanvas.Background = Brushes.White;

            this.HorizontalAlignment = QGHorizontalAlignment.Left;
            this.VerticalAlignment = QGVerticalAlignment.Top;
            this.OriginAlignment = QGCoordinateOriginAlignment.TopLeft;

            this.LayerCanvas.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.LayerCanvas.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            this.UserSelectable = false;

            this.WPFSelectionRectangle = new Rectangle();
            this.WPFSelectionRectangle.StrokeThickness = 1;
            this.WPFSelectionRectangle.Stroke = Brushes.Black;
            this.WPFSelectionRectangle.Fill = Brushes.DeepSkyBlue;
            this.WPFSelectionRectangle.Opacity = 0.15;
            this.WPFSelectionRectangle.Visibility = Visibility.Collapsed;
        }

        protected override void OnInitialized(QGControlEventArgs e)
        {
            base.OnInitialized(e);

            this.WPFCanvasAdornerLayer = AdornerLayer.GetAdornerLayer(this.LayerCanvas);
            this.WPFSelectionRectangleAdorner = new WPFSelectionRectangleAdorner(this.LayerCanvas, this.WPFSelectionRectangle);
            this.WPFCanvasAdornerLayer.Add(this.WPFSelectionRectangleAdorner);
        }
    }

    public class WPFSelectionRectangleAdorner : Adorner
    {
        public Rectangle SelectionRectangle { get; }
        protected VisualCollection AdornerVisualCollection { get; }

        protected override int VisualChildrenCount
        {
            get { return this.AdornerVisualCollection.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.AdornerVisualCollection[index];
        }

        public WPFSelectionRectangleAdorner(UIElement rootLayerCanvas, Rectangle selectionRectangle)
            : base(rootLayerCanvas)
        {
            this.SelectionRectangle = selectionRectangle;
            this.AdornerVisualCollection = new VisualCollection(this);
            this.AdornerVisualCollection.Add(selectionRectangle);
        }
    }
}
