using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Qwasi.GraphUI
{
    public enum QGCoordinateOriginAlignment { TopLeft, Center }

    public interface IQGLayer
    {
        public static IQGLayer FindNearestCommonLayer(IQGControl e1, IQGControl e2)
        {
            return e1.ParentLayer.AncestorEnumeration.Intersect(e2.ParentLayer.AncestorEnumeration).FirstOrDefault();
        }

        QGControlCollection ChildControls { get; }

        public sealed IEnumerable<IQGControl> AllDescendentControls => this.ChildControls.SelectMany(c =>__selectDescendents(c));

        private IEnumerable<IQGControl> __selectDescendents(IQGControl child)
        {
            if (child is IQGLayer asLayer)
                return asLayer.AllDescendentControls;

            return new IQGControl[] { child };
        }

        internal protected void OnAddingControl(IQGControl control);
        internal protected void OnRemovingControl(IQGControl control);

        public void UpdateControlPosition(IQGControl child);
        public void UpdateControlSize(IQGControl child);

        public IQGLayer GetRootLayer()
        {
            IQGLayer root;
            for (root = this; (root as IQGControl)?.ParentLayer != null; root = ((IQGControl)root).ParentLayer) ;
            return root;
        }

        public IEnumerable<IQGLayer> AncestorEnumeration
        {
            get
            {
                for (IQGLayer layer = this; layer != null && layer is IQGControl; layer = ((IQGControl)layer).ParentLayer)
                    yield return layer;
            }
        }

        public QGCoordinateOriginAlignment OriginAlignment { get; set; }
        public Vector WPFOriginDisplacement();

        public Point GetLocalCoordinatesOf(IQGLayer descendentLayer, Point descendentPoint)
        {
            Point localPoint = descendentPoint;

            for (IQGLayer l = descendentLayer; l != this; l = (l as IQGControl)?.ParentLayer)
            {
                if (l == null)
                    throw new Exception("Cannot get local coordinates for layer: layer is not a descendent of the calling class");

                if (l is IQGControl)
                    localPoint += (Vector)((IQGControl)l).Position + l.WPFOriginDisplacement() + ((IQGControl)l).GetAlignmentOffset();
            }

            return localPoint;
        }

        public Point GetLocalCoordinatesOf(IQGControl descendentControl)
        {
            return this.GetLocalCoordinatesOf(descendentControl.ParentLayer, descendentControl.Position);
        }

        public Point GetRenderingCoordinatesOf(Point localCoordinates)
        {
            return this.WPFOriginDisplacement() + localCoordinates;
        }
    }

    public sealed class QGControlCollection : Collection<IQGControl>
    {
        private IQGLayer _ParentLayer;

        protected override void InsertItem(int index, IQGControl item)
        {
            base.InsertItem(index, item);
            _ParentLayer.OnAddingControl(item);
        }

        protected override void RemoveItem(int index)
        {
            IQGControl item = this[index];
            base.RemoveItem(index);
            _ParentLayer.OnRemovingControl(item);
        }

        protected override void SetItem(int index, IQGControl item)
        {
            IQGControl toRemove = this[index];

            base.SetItem(index, item);
            if (toRemove != null)
                _ParentLayer.OnRemovingControl(toRemove);
            _ParentLayer.OnAddingControl(item);
        }

        protected override void ClearItems()
        {
            foreach (IQGControl control in this)
                _ParentLayer.OnRemovingControl(control);
            base.ClearItems();
        }

        public QGControlCollection(IQGLayer parentLayer)
        {
            _ParentLayer = parentLayer;
        }
    }
}
