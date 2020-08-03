using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Qwasi.GraphUI
{
    public interface IQGMouseDragHandler : IQGMouseHandler, IInterfaceStateAgent<IQGMouseDragHandler>
    {
        public static readonly EventIdentifier<IQGMouseDragHandler> PreviewMouseLeftButtonDownEvent =
            EventIdentifier<IQGMouseDragHandler>.Create("PreviewMouseLeftButtonDown");

        protected void RaisePreviewMouseLeftButtonDownEvent(MouseButtonEventArgs e) => RaiseEvent(PreviewMouseLeftButtonDownEvent, e);
        public event EventHandler<MouseButtonEventArgs> PreviewMouseLeftButtonDown
        {
            add { AddEventHandler(PreviewMouseLeftButtonDownEvent, value); }
            remove { RemoveEventHandler(PreviewMouseLeftButtonDownEvent, value); }
        }

        public static readonly EventIdentifier<IQGMouseDragHandler> MouseLeftButtonDownEvent =
            EventIdentifier<IQGMouseDragHandler>.Create("MouseLeftButtonDown");

        protected void RaiseMouseLeftButtonDownEvent(MouseButtonEventArgs e) => RaiseEvent(MouseLeftButtonDownEvent, e);
        public event EventHandler<MouseButtonEventArgs> MouseLeftButtonDown
        {
            add { AddEventHandler(MouseLeftButtonDownEvent, value); }
            remove { RemoveEventHandler(MouseLeftButtonDownEvent, value); }
        }

        public static readonly EventIdentifier<IQGMouseDragHandler> MouseLeftButtonUpEvent =
            EventIdentifier<IQGMouseDragHandler>.Create("MouseLeftButtonUp");

        protected void RaiseMouseLeftButtonUpEvent(MouseButtonEventArgs e) => RaiseEvent(MouseLeftButtonUpEvent, e);
        public event EventHandler<MouseButtonEventArgs> MouseLeftButtonUp
        {
            add { AddEventHandler(MouseLeftButtonUpEvent, value); }
            remove { RemoveEventHandler(MouseLeftButtonUpEvent, value); }
        }

        public static readonly EventIdentifier<IQGMouseDragHandler> MouseMoveEvent =
            EventIdentifier<IQGMouseDragHandler>.Create("MouseMove");

        protected void RaiseMouseMoveEvent(MouseEventArgs e) => RaiseEvent(MouseMoveEvent, e);
        public event EventHandler<MouseEventArgs> MouseMove
        {
            add { AddEventHandler(MouseMoveEvent, value); }
            remove { RemoveEventHandler(MouseMoveEvent, value); }
        }

        bool IQGMouseHandler.ShouldCaptureMouse() => this is IQGDraggable;

        private static Point s_mouseDownLoc;
        void IQGMouseHandler.OnMouseLeftButtonDown(object o, MouseButtonEventArgs e)
        {
            s_mouseDownLoc = e.GetPosition(null);

            IQGSelectable asSelectable = this as IQGSelectable;
            if (asSelectable != null && asSelectable.UserSelectable)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    if (!asSelectable.IsSelected)
                        asSelectable.Select();
                    else
                        asSelectable.Unselect();
                }
                else
                {
                    if (!asSelectable.IsSelected)
                    {
                        IQGSelectable.ClearSelectedElements();
                        asSelectable.Select();
                    }
                }
            }
            else if (!Keyboard.IsKeyDown(Key.LeftShift))
                IQGSelectable.ClearSelectedElements();

            this.RaiseMouseLeftButtonDownEvent(e);
        }

        void IQGMouseHandler.OnMouseLeftButtonUp(object o, MouseButtonEventArgs e)
        {
            if (!(this is IQGDraggable asDraggable))
                return;

            Vector displacement = e.GetPosition(null) - s_mouseDownLoc;
            asDraggable.StopDrag(displacement, QGDragActionType.Active);

            this.RaiseMouseLeftButtonUpEvent(e);
        }

        public static double DragBuffer { get; set; } = 14;
        void IQGMouseHandler.OnMouseLeftButtonMove(object o, MouseEventArgs e)
        {
            if (!(this is IQGDraggable asDraggable))
                return;

            Vector displacement = e.GetPosition(null) - s_mouseDownLoc;
            if (asDraggable.IsDraggable(QGDragParameterType.Active))
            {
                if (!asDraggable.IsBeingDragged)
                {
                    if (displacement.Length > DragBuffer)
                        asDraggable.StartDrag(QGDragActionType.Active);
                }
                else
                {
                    asDraggable.ContinueDrag(displacement, QGDragActionType.Active);
                }
            }

            this.RaiseMouseMoveEvent(e);
        }
    }
}
