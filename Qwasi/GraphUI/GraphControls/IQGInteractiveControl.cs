using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Qwasi.GraphUI
{
    public interface IQGInteractiveControl: IQGControl, IQGSelectable, IQGMouseDragHandler, IQGDraggable, IQGUserDeletable
    {
    }

    public abstract class QGInteractiveControl : QGControl, IQGInteractiveControl
    {
        InterfaceStateCatalog<IQGSelectable> IInterfaceStateAgent<IQGSelectable>.InterfaceStateCatalog { get; set; }
        InterfaceStateCatalog<IQGMouseDragHandler> IInterfaceStateAgent<IQGMouseDragHandler>.InterfaceStateCatalog { get; set; }
        InterfaceStateCatalog<IQGDraggable> IInterfaceStateAgent<IQGDraggable>.InterfaceStateCatalog { get; set; }
        InterfaceStateCatalog<IQGUserDeletable> IInterfaceStateAgent<IQGUserDeletable>.InterfaceStateCatalog { get; set; }

        public event EventHandler<MouseButtonEventArgs> PreviewMouseLeftButtonDown;
        public event EventHandler<MouseButtonEventArgs> MouseLeftButtonDown;
        public event EventHandler<MouseButtonEventArgs> MouseLeftButtonUp;
        public event EventHandler<MouseEventArgs> MouseMove;

        protected virtual void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) => this.PreviewMouseLeftButtonDown?.Invoke(this, e);
        protected virtual void OnMouseLeftButtonDown(MouseButtonEventArgs e) => this.MouseLeftButtonDown?.Invoke(this, e);
        protected virtual void OnMouseLeftButtonUp(MouseButtonEventArgs e) => this.MouseLeftButtonUp?.Invoke(this, e);
        protected virtual void OnMouseMove(MouseEventArgs e) => this.MouseMove?.Invoke(this, e);

        void IQGMouseDragHandler.RaisePreviewMouseLeftButtonDownEvent(MouseButtonEventArgs e) => this.OnPreviewMouseLeftButtonDown(e);
        void IQGMouseDragHandler.RaiseMouseLeftButtonDownEvent(MouseButtonEventArgs e) => this.OnMouseLeftButtonDown(e);
        void IQGMouseDragHandler.RaiseMouseLeftButtonUpEvent(MouseButtonEventArgs e) => this.OnMouseLeftButtonUp(e);
        void IQGMouseDragHandler.RaiseMouseMoveEvent(MouseEventArgs e) => this.OnMouseMove(e);

        public bool IsSelected => ((IQGSelectable)this).IsSelected;
        public bool UserSelectable
        {
            get => ((IQGSelectable)this).UserSelectable;
            set => ((IQGSelectable)this).UserSelectable = value;
        }

        public event EventHandler<QGSelectionEventArgs> SelectionGained;
        public event EventHandler<QGSelectionEventArgs> SelectionLost;

        public bool SelectParentToo { get; set; } = true;
        protected virtual void OnSelectionGained(QGSelectionEventArgs e)
        {
            this.SelectionGained?.Invoke(this, e);

            if (this.SelectParentToo && this.ParentLayer is IQGSelectable asSelectable)
                asSelectable.Select();
        }

        protected virtual void OnSelectionLost(QGSelectionEventArgs e)
        {
            this.SelectionLost?.Invoke(this, e);

            if (this.SelectParentToo && this.ParentLayer is IQGSelectable asSelectable)
                if (!this.ParentLayer.ChildControls.OfType<IQGSelectable>().Any(c => c.IsSelected))
                    asSelectable.Unselect();
        }

        void IQGSelectable.RaiseSelectionGainedEvent(QGSelectionEventArgs e) => this.OnSelectionGained(e);
        void IQGSelectable.RaiseSelectionLostEvent(QGSelectionEventArgs e) => this.OnSelectionLost(e);

        public void SetDraggability(bool isDraggable) => ((IQGDraggable)this).SetDraggability(isDraggable);
        public void SetDraggability(bool isDraggable, QGDragParameterType actionType)
            => ((IQGDraggable)this).SetDraggability(isDraggable, actionType);
        public bool IsDraggable() => ((IQGDraggable)this).IsDraggable();
        public bool IsDraggable(QGDragParameterType actionType) => ((IQGDraggable)this).IsDraggable(actionType);

        public void SetDragMobility(bool isDragMobile) => ((IQGDraggable)this).SetDragMobility(isDragMobile);
        public void SetDragMobility(bool isDragMobile, QGDragParameterType actionType)
            => ((IQGDraggable)this).SetDragMobility(isDragMobile, actionType);
        public bool IsDragMobile() => ((IQGDraggable)this).IsDragMobile();
        public bool IsDragMobile(QGDragParameterType actionType) => ((IQGDraggable)this).IsDragMobile(actionType);

        public void SetProxyTargets(IEnumerable<IQGControl> controls, QGDragParameterType actionType)
            => ((IQGDraggable)this).SetProxyTargets(controls, actionType);
        public IEnumerable<IQGControl> GetProxyTargets(QGDragParameterType actionType)
            => ((IQGDraggable)this).GetProxyTargets(actionType);

        public void AddDragStartEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => ((IQGDraggable)this).AddDragStartEventHandler(eventHandler, actionType);
        public void RemoveDragStartEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => ((IQGDraggable)this).RemoveDragStartEventHandler(eventHandler, actionType);

        public void AddDragMoveEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => ((IQGDraggable)this).AddDragMoveEventHandler(eventHandler, actionType);
        public void RemoveDragMoveEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => ((IQGDraggable)this).RemoveDragMoveEventHandler(eventHandler, actionType);

        private static readonly VariableIdentifier<IQGDraggable> _DragStopEventHandlerProperty =
            VariableIdentifier<IQGDraggable>.Create("_DragStopEventHandler");

        public void AddDragStopEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => ((IQGDraggable)this).AddDragStopEventHandler(eventHandler, actionType);
        public void RemoveDragStopEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => ((IQGDraggable)this).RemoveDragStopEventHandler(eventHandler, actionType);

        public QGDragActionType? CurrentDragAction => ((IQGDraggable)this).CurrentDragAction;
        public bool IsBeingDragged => ((IQGDraggable)this).IsBeingDragged;

        protected virtual void OnDragStart(QGDragEventArgs e) { }
        protected virtual void OnDragMove(QGDragEventArgs e) { }
        protected virtual void OnDragStop(QGDragEventArgs e) { }

        public bool UserDeletable { get; set; } = true;

        public QGInteractiveControl()
            : base()
        {
            ((IQGMouseHandler)this).InitializeInterface();

            this.AddDragStartEventHandler((o, e) => this.OnDragStart(e), QGDragParameterType.All);
            this.AddDragMoveEventHandler((o, e) => this.OnDragMove(e), QGDragParameterType.All);
            this.AddDragStopEventHandler((o, e) => this.OnDragStop(e), QGDragParameterType.All);
        }
    }
}
