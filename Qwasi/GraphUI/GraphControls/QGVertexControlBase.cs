using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Input;

namespace Qwasi.GraphUI
{
    public abstract class QGVertexControlBase : QGInteractiveControl
    {
        private double _Radius = 4;
        public double Radius
        {
            get { return _Radius; }
            set
            {
                _Radius = value;
                __refresh();
            }
        }

        public double StrokeThickness
        {
            get { return this.WPFVertexCircle.StrokeThickness; }
            set { this.WPFVertexCircle.StrokeThickness = value; }
        }

        public double _HighlightThickness = 2;
        public double HighlightThickness
        {
            get { return _HighlightThickness; }
            set { _HighlightThickness = value; __refresh(); }
        }

        public double _SelectedHighlightThickness = 2;
        public double SelectedHighlightThickness
        {
            get { return _SelectedHighlightThickness; }
            set { _SelectedHighlightThickness = value; __refresh(); }
        }

        public double CurrentHighlightThickness => this.IsSelected ? this.SelectedHighlightThickness : this.HighlightThickness;

        public double TotalRadius
        {
            get
            {
                return this.Radius + ((this.HighlightThickness > this.SelectedHighlightThickness) ?
                    this.HighlightThickness : this.SelectedHighlightThickness);
            }
        }

        private Brush _prevFill = null;
        private Brush _prevSelectedFill = null;
        public void SetColorOverride(Brush fill, Brush selectedFill)
        {
            if (_prevFill == null)
            {
                _prevFill = this.Fill;
                _prevSelectedFill = this.SelectedFill;
            }

            this.Fill = fill;
            this.SelectedFill = selectedFill;
        }

        public void ReleaseColorOverride()
        {
            if (_prevFill == null)
                return;

            this.Fill = _prevFill;
            this.SelectedFill = _prevSelectedFill;
            _prevFill = null;
            _prevSelectedFill = null;
        }

        protected Ellipse WPFVertexCircle { get; private set; }
        protected Ellipse WPFHighlightCircle { get; private set; }

        public Brush _Fill;
        public Brush Fill
        {
            get { return _Fill; }
            set { _Fill = value; __refresh(); }
        }

        public Brush _SelectedFill;
        public Brush SelectedFill
        {
            get { return _SelectedFill; }
            set { _SelectedFill = value; __refresh(); }
        }

        public Brush CurrentFill => this.IsSelected ? this.SelectedFill : this.Fill;

        public Brush _StrokeBrush;
        public Brush StrokeBrush
        {
            get { return _StrokeBrush; }
            set { _StrokeBrush = value; __refresh(); }
        }

        public Brush _SelectedStrokeBrush;
        public Brush SelectedStrokeBrush
        {
            get { return _SelectedStrokeBrush; }
            set { _SelectedStrokeBrush = value; __refresh(); }
        }

        public Brush CurrentStrokeBrush => this.IsSelected ? this.SelectedStrokeBrush : this.StrokeBrush;

        public Brush _HighlightBrush;
        public Brush HighlightBrush
        {
            get { return _HighlightBrush; }
            set { _HighlightBrush = value; __refresh(); }
        }

        public Brush _SelectedHighlightBrush;
        public Brush SelectedHighlightBrush
        {
            get { return _SelectedHighlightBrush; }
            set { _SelectedHighlightBrush = value; __refresh(); }
        }

        public Brush CurrentHighlightBrush => this.IsSelected ? this.SelectedHighlightBrush : this.HighlightBrush;

        public override Size RenderSize
        {
            get
            {
                Size baseSize = base.RenderSize;
                return new Size(baseSize.Width + 24, baseSize.Height + 18);
            }
        }

        protected override void OnDragStart(QGDragEventArgs e)
        {
            this.WPFVertexCircle.Opacity = 0.35;
            this.WPFHighlightCircle.Opacity = 0.1;

            base.OnDragStart(e);
        }

        protected override void OnDragStop(QGDragEventArgs e)
        {
            this.WPFVertexCircle.Opacity = 1;
            this.WPFHighlightCircle.Opacity = 1;

            base.OnDragStop(e);
        }

        protected override void OnSelectionGained(QGSelectionEventArgs e)
        {
            base.OnSelectionGained(e);
            __refresh();
        }

        protected override void OnSelectionLost(QGSelectionEventArgs e)
        {
            base.OnSelectionLost(e);
            __refresh();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if (!this.IsSelected)
                this.WPFHighlightCircle.Visibility = Visibility.Visible;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!this.IsSelected)
                this.WPFHighlightCircle.Visibility = Visibility.Collapsed;
        }

        private void __refresh()
        {
            ((FrameworkElement)this.WPFPrimaryElement).Width = ((FrameworkElement)this.WPFPrimaryElement).Height = 2 * this.TotalRadius;

            this.WPFVertexCircle.Width = 2 * this.Radius;
            this.WPFVertexCircle.Height = 2 * this.Radius;

            this.WPFHighlightCircle.Width = 2 * (this.Radius + this.CurrentHighlightThickness);
            this.WPFHighlightCircle.Height = 2 * (this.Radius + this.CurrentHighlightThickness);

            this.WPFVertexCircle.Fill = this.CurrentFill;
            this.WPFVertexCircle.Stroke = this.CurrentStrokeBrush;
            this.WPFHighlightCircle.Fill = this.CurrentHighlightBrush;

            this.WPFHighlightCircle.Visibility = this.IsSelected ? Visibility.Visible : Visibility.Hidden;
        }

        public QGVertexControlBase(Point location)
            : this()
        {
            this.SetPosition(location);
        }

        public QGVertexControlBase(double x, double y)
            : this(new Point(x, y))
        { }

        public QGVertexControlBase()
            : base()
        {
            this.WPFHighlightCircle = new Ellipse();
            this.WPFVertexCircle = new Ellipse();

            this.RegisterWPFElement(this.WPFHighlightCircle);
            this.RegisterWPFElement(this.WPFVertexCircle);
            this.SetWPFPrimaryElement(this.WPFVertexCircle);

            this.Fill = Brushes.LightGreen;
            this.SelectedFill = Brushes.Aqua;

            this.HighlightBrush = Brushes.Black;
            this.SelectedHighlightBrush = Brushes.Black;
            this.StrokeBrush = Brushes.Black;
            this.SelectedStrokeBrush = Brushes.Black;

            this.UserSelectable = true;
            this.SetDraggability(true);
            this.SetDragMobility(true);

            this.StrokeThickness = 1;

            __refresh();
        }
    }

    public class QGDummyVertexControl : QGVertexControlBase
    {
    }
}
