using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Qwasi.GraphUI
{
    public enum QGHorizontalAlignment { Left, Center, Right }
    public enum QGVerticalAlignment { Top, Center, Bottom }

    public delegate void QGControlActionDelegate(IQGControl caller);

    public interface IQGControl : IWPFContainer
    {
        public IQGLayer ParentLayer { get; }
        internal void SetParentLayer(IQGLayer parentLayer);
        public IQGLayer BindingLayer { get; }

        public Point Position { get; }

        public void SetPosition(Point p);
        public void SetPosition(double x, double y) => this.SetPosition(new Point(x, y));

        double X { get; }
        double Y { get; }

        Size Size { get; }
        Size RenderSize { get; }

        double Width { get; }
        double Height { get; }

        QGHorizontalAlignment HorizontalAlignment { get; set; }
        QGVerticalAlignment VerticalAlignment { get; set; }

        event EventHandler<QGSizeChangeEventArgs> SizeChange;
        event EventHandler<QGPositionChangeEventArgs> PositionChange;

        bool IsInitialized { get; }
        event EventHandler<QGControlEventArgs> Initialized;

        void DeleteControl();
        event EventHandler<QGControlEventArgs> Deleting;

        public sealed void RegisterControlInitMethod(QGControlActionDelegate method)
        {
            this.Initialized += (o, e) => method(this);
            if (this.IsInitialized)
                method(this);
        }

        internal Vector GetAlignmentOffset() => this.GetAlignmentOffset(this.HorizontalAlignment, this.VerticalAlignment, this.Size);
        internal Vector GetAlignmentOffset(Size size) => this.GetAlignmentOffset(this.HorizontalAlignment, this.VerticalAlignment, size);
        internal Vector GetAlignmentOffset(QGHorizontalAlignment horizontalAlignment, QGVerticalAlignment verticalAlignment, Size size)
        {
            Vector alignmentOffset = new Vector();

            switch (horizontalAlignment)
            {
                case QGHorizontalAlignment.Left:
                    alignmentOffset.X = 0;
                    break;
                case QGHorizontalAlignment.Center:
                    alignmentOffset.X = -(size.Width / 2);
                    break;
                case QGHorizontalAlignment.Right:
                    alignmentOffset.X = -size.Width;
                    break;
                default:
                    break;
            }

            switch (verticalAlignment)
            {
                case QGVerticalAlignment.Top:
                    alignmentOffset.Y = 0;
                    break;
                case QGVerticalAlignment.Center:
                    alignmentOffset.Y = -(size.Height / 2);
                    break;
                case QGVerticalAlignment.Bottom:
                    alignmentOffset.Y = -size.Height;
                    break;
                default:
                    break;
            }

            return alignmentOffset;
        }
    }

    public abstract class QGControl : IQGControl
    {
        InterfaceStateCatalog<IWPFContainer> IInterfaceStateAgent<IWPFContainer>.InterfaceStateCatalog { get; set; }

        public IQGLayer ParentLayer { get; private set; }
        void IQGControl.SetParentLayer(IQGLayer parentLayer) => this.ParentLayer = parentLayer;

        private IQGLayer _BindingLayer = null;
        public virtual IQGLayer BindingLayer => _BindingLayer ??= new QGBindingLayer(this);
        
        public UIElement WPFPrimaryElement => ((IWPFContainer)this).WPFPrimaryElement;
        protected void SetWPFPrimaryElement(UIElement element) => ((IWPFContainer)this).SetWPFPrimaryElement(element);
        protected void RegisterWPFElement(UIElement element) => ((IWPFContainer)this).RegisterWPFElement(element);
        protected IEnumerable<WPFElementInfo> WPFElementInfoEnumeration => ((IWPFContainer)this).WPFElementInfoEnumeration;
        protected IEnumerable<UIElement> WPFElements => ((IWPFContainer)this).WPFElements;

        private Point _Position = new Point(0, 0);
        public virtual Point Position => _Position;

        public void SetPosition(double x, double y) => this.SetPosition(new Point(x, y));
        public virtual void SetPosition(Point p)
        {
            if (p == null)
                return;

            Point prev = this.Position;
            _Position = p;

            if (prev.Equals(_Position))
                return;

            this.ParentLayer?.UpdateControlPosition(this);
            this.RaisePositionChangeEvent(new QGPositionChangeEventArgs(prev, _Position));
        }

        public double X => this.Position.X;
        public double Y => this.Position.Y;

        public virtual Size Size => (this.WPFPrimaryElement is FrameworkElement fe) ? new Size(fe.Width, fe.Height) : this.WPFPrimaryElement.RenderSize;
        public double Width => this.Size.Width;
        public double Height => this.Size.Height;

        public virtual Size RenderSize => this.Size;

        public QGHorizontalAlignment HorizontalAlignment { get; set; } = QGHorizontalAlignment.Center;
        public QGVerticalAlignment VerticalAlignment { get; set; } = QGVerticalAlignment.Center;

        public event EventHandler<QGPositionChangeEventArgs> PositionChange;
        public event EventHandler<QGSizeChangeEventArgs> SizeChange;

        protected virtual void OnPositionChange(QGPositionChangeEventArgs e) => this.PositionChange?.Invoke(this, e);
        protected virtual void OnSizeChange(QGSizeChangeEventArgs e) => this.SizeChange?.Invoke(this, e);

        private void RaisePositionChangeEvent(QGPositionChangeEventArgs e) => this.OnPositionChange(e);
        private void RaiseSizeChangeEvent(QGSizeChangeEventArgs e) => this.OnSizeChange(e);

        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;

        protected virtual void OnMouseEnter(MouseEventArgs e) => this.MouseEnter?.Invoke(this, e);
        protected virtual void OnMouseLeave(MouseEventArgs e) => this.MouseLeave?.Invoke(this, e);

        public bool IsInitialized { get; protected set; } = false;
        public event EventHandler<QGControlEventArgs> Initialized;
        private void RaiseInitializedEvent() { this.IsInitialized = true; this.OnInitialized(new QGControlEventArgs()); }
        protected virtual void OnInitialized(QGControlEventArgs e) => this.Initialized?.Invoke(this, e);

        public event EventHandler<QGControlEventArgs> Deleting;
        private void RaiseDeletingEvent() { this.OnDeleting(new QGControlEventArgs()); }
        protected virtual void OnDeleting(QGControlEventArgs e) => this.Deleting?.Invoke(this, e);

        protected bool IsDeleting { get; private set; } = false;
        public void DeleteControl()
        {
            if (this.IsDeleting || this.ParentLayer == null)
                return;

            if (this is IQGSelectable asSelectable && asSelectable.IsSelected)
                asSelectable.Unselect();

            this.IsDeleting = true;
            this.RaiseDeletingEvent();
            ((QGBindingLayer)this.BindingLayer).DeleteBindingLayer();
            this.ParentLayer.ChildControls.Remove(this);
            this.ParentLayer = null;
        }

        public void RegisterControlInitMethod(QGControlActionDelegate method) => ((IQGControl)this).RegisterControlInitMethod(method);

        public QGControl()
        {
            (this as IWPFContainer)?.RegisterWPFPrimaryElementInitMethod(__WPFInitializePrimaryElement);
        }

        private void __WPFInitializePrimaryElement(object wpfObject, UIElement wpfPrimaryElement)
        {
            if (wpfPrimaryElement is FrameworkElement fe)
            {
                fe.SizeChanged += __wpfPrimaryElementSizeChanged;
                fe.MouseEnter += (o, e) => this.OnMouseEnter(e);
                fe.MouseLeave += (o, e) => this.OnMouseLeave(e);
                fe.Loaded += (o, e) => this.RaiseInitializedEvent();
            }
        }

        private void __wpfPrimaryElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RaiseSizeChangeEvent(new QGSizeChangeEventArgs(e.PreviousSize, e.NewSize));
            this.ParentLayer?.UpdateControlSize(this);
        }
    }

    public class QGControlEventArgs : EventArgs
    {
    }

    public class QGPositionChangeEventArgs : QGControlEventArgs
    {
        public Point PreviousPosition { get; private set; }
        public Point CurrentPosition { get; private set; }

        public Vector Displacement
        {
            get { return this.CurrentPosition - this.PreviousPosition; }
        }

        public QGPositionChangeEventArgs(Point prev, Point current)
        {
            this.PreviousPosition = prev;
            this.CurrentPosition = current;
        }
    }

    public class QGSizeChangeEventArgs : QGControlEventArgs
    {
        public Size OriginalSize { get; private set; }
        public Size NewSize { get; private set; }

        public Size SizeDifference
        {
            get { return new Size(this.NewSize.Width - this.OriginalSize.Width, this.NewSize.Height - this.OriginalSize.Height); }
        }

        public QGSizeChangeEventArgs(Size origSize, Size newSize)
        {
            this.OriginalSize = origSize;
            this.NewSize = newSize;
        }
    }
}
