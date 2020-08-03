using Qwasi.HilbertSpaceMath;
using Qwasi.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Qwasi.GraphUI
{
    public abstract class QGEdgeLabel : QGLabel
    {
        protected QGEdge ParentEdge => (this.ParentLayer as QGBindingLayer)?.ParentControl as QGEdge;

        protected Grid WPFGrid { get; private set; } = new Grid();
        protected override FrameworkElement WPFInnerControl => this.WPFGrid;

        protected abstract bool IsLabelPinned { get; set; }

        private List<UIElement> _HitTestElementList = new List<UIElement>();
        protected IEnumerable<UIElement> HitTestElements => _HitTestElementList.AsEnumerable();
        protected void RegisterHitTestElement(UIElement element)
        {
            _HitTestElementList.Add(element);
            element.MouseEnter += __hitTestMouseEnter;
            element.MouseLeave += __hitTestMouseLeave;
        }

        protected void UnregisterHitTestElement(UIElement element) => _HitTestElementList.Remove(element);

        private void __hitTestMouseEnter(object sender, MouseEventArgs e)
        {
            _ShouldHide = false;
        }

        private void __hitTestMouseLeave(object sender, MouseEventArgs e)
        {
            if (!this.HitTestElements.Any(e => e.IsMouseOver))
                this.HideIfNotPinned();
        }

        protected override Point GetDynamicLabelCoordinates()
        {
            Vector ev = this.ParentEdge.GetEdgeVector(this.ParentEdge.Vertex2);
            ev.Normalize();
            if (ev.Y < 0)
                ev.Negate();

            Vector labelVector = new Vector(ev.Y, -ev.X);
            Vector labelOffset = new Vector(0, labelVector.Y * this.Height / 2);

            return this.ParentEdge.GetEdgeMidpoint() + ((labelVector * 8) + labelOffset);
        }

        public void Pin()
        {
            this.IsLabelPinned = true;
        }

        public void Unpin()
        {
            this.IsLabelPinned = false;
            this.Hide();
        }

        public void Show()
        {
            this.Visibility = Visibility.Visible;
            this.ParentEdge.ApplyHighlight();
        }

        public void Hide()
        {
            this.Visibility = Visibility.Collapsed;
            this.ParentEdge.RemoveHighlight();
        }

        public void HideIfNotPinned()
        {
            this.ParentEdge.RemoveHighlight();
            if (!this.IsLabelPinned)
                this.Hide();
        }

        private bool _ShouldHide = false;
        Timer _Timer = null;
        public void TimedHide()
        {
            this.ParentEdge.ApplyHighlight();

            if (_Timer == null)
            {
                _Timer = new Timer(100);
                _Timer.Elapsed += __hideCallback;
            }

            _ShouldHide = true;
            _Timer.Start();
        }

        private void __hideCallback(object o, ElapsedEventArgs e)
        {
            _Timer.Stop();
            if (!_ShouldHide)
                return;

            QGEdgeLabel el = o as QGEdgeLabel;
            this.WPFPrimaryElement.Dispatcher.Invoke(() => this.HideIfNotPinned());
        }

        public QGEdgeLabel()
            : base()
        {
            this.RegisterHitTestElement(this.WPFPrimaryElement);
        }
    }

    public class QGEdgeLabelEventArgs : EventArgs
    {
        public QGEdge Edge { get; }

        public QGEdgeLabelEventArgs(QGEdge edge)
        {
            this.Edge = edge;
        }
    }
}
