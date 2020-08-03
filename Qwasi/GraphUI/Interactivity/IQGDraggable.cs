using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Qwasi.GraphUI
{
    public enum QGDragActionType { Active = 0, Passive = 1, Proxy = 2 }
    public enum QGDragParameterType { Active = 0, Passive = 1, Proxy = 2, All = 3 }

    public interface IQGDraggable : IInterfaceStateAgent<IQGDraggable>
    {
        private static readonly VariableIdentifier<IQGDraggable> _StartPositionDictionaryProperty =
            VariableIdentifier<IQGDraggable>.Create("_StartPositionDictionary");
        private IDictionary<IQGControl, Point> _StartPositionDictionary
        {
            get => (IDictionary<IQGControl, Point>)GetInstance(_StartPositionDictionaryProperty);
            set => SetInstance(_StartPositionDictionaryProperty, value);
        }

        private static readonly VariableIdentifier<IQGDraggable> _ActionDraggabilityProperty =
            VariableIdentifier<IQGDraggable>.Create("_ActionDraggability");

        private bool[] _ActionDraggability
        {
            get => (bool[])(GetInstance(_ActionDraggabilityProperty));
            set => SetInstance(_ActionDraggabilityProperty, value);
        }

        public sealed void SetDraggability(bool isDraggable) => this.SetDraggability(isDraggable, QGDragParameterType.All);
        public sealed void SetDraggability(bool isDraggable, QGDragParameterType actionType)
        {
            if (actionType == QGDragParameterType.All)
                _ActionDraggability[0] = _ActionDraggability[1] = _ActionDraggability[2] = isDraggable;
            else
                _ActionDraggability[(int)actionType] = isDraggable;
        }

        public sealed bool IsDraggable() => this.IsDraggable(QGDragParameterType.All);
        public sealed bool IsDraggable(QGDragParameterType actionType)
        {
            if (actionType == QGDragParameterType.All)
                return _ActionDraggability[0] && _ActionDraggability[1] && _ActionDraggability[2];
            else
                return _ActionDraggability[(int)actionType];
        }

        private static readonly VariableIdentifier<IQGDraggable> _ActionDragMobilityProperty =
            VariableIdentifier<IQGDraggable>.Create("_ActionDragMobility");

        private bool[] _ActionDragMobility
        {
            get => (bool[])(GetInstance(_ActionDragMobilityProperty));
            set => SetInstance(_ActionDragMobilityProperty, value);
        }

        public sealed void SetDragMobility(bool isDragMobile) => this.SetDragMobility(isDragMobile, QGDragParameterType.All);
        public sealed void SetDragMobility(bool isDragMobile, QGDragParameterType actionType)
        {
            if (actionType == QGDragParameterType.All)
                _ActionDragMobility[0] = _ActionDragMobility[1] = _ActionDragMobility[2] = isDragMobile;
            else
                _ActionDragMobility[(int)actionType] = isDragMobile;
        }

        public sealed bool IsDragMobile() => this.IsDragMobile(QGDragParameterType.All);
        public sealed bool IsDragMobile(QGDragParameterType actionType)
        {
            if (actionType == QGDragParameterType.All)
                return _ActionDragMobility[0] && _ActionDragMobility[1] && _ActionDragMobility[2];
            else
                return _ActionDragMobility[(int)actionType];
        }

        private static readonly VariableIdentifier<IQGDraggable> _ActionProxyTargetListsProperty =
            VariableIdentifier<IQGDraggable>.Create("_ActionProxyTargetLists");

        private IEnumerable<IQGControl>[] _ActionProxyTargetLists
        {
            get => (IEnumerable<IQGControl>[])(GetInstance(_ActionProxyTargetListsProperty));
            set => SetInstance(_ActionProxyTargetListsProperty, value);
        }

        public sealed void SetProxyTargets(IEnumerable<IQGControl> controls, QGDragParameterType actionType)
            => _ActionProxyTargetLists[(int)actionType] = controls.ToArray();
        public sealed IEnumerable<IQGControl> GetProxyTargets(QGDragParameterType actionType)
            => _ActionProxyTargetLists[(int)actionType];

        private static readonly VariableIdentifier<IQGDraggable> _DragStartEventHandlerProperty =
            VariableIdentifier<IQGDraggable>.Create("_DragStartEventHandler");

        private EventHandler<QGDragEventArgs>[] _DragStartEventHandler
        {
            get => (EventHandler<QGDragEventArgs>[])(GetInstance(_DragStartEventHandlerProperty));
            set => SetInstance(_DragStartEventHandlerProperty, value);
        }

        public sealed void AddDragStartEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => _DragStartEventHandler[(int)actionType] += eventHandler;
        public sealed void RemoveDragStartEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => _DragStartEventHandler[(int)actionType] -= eventHandler;
        protected sealed void RaiseDragStartEvent(QGDragEventArgs e, QGDragParameterType actionType)
            => _DragStartEventHandler[(int)actionType]?.Invoke(this, e);

        private static readonly VariableIdentifier<IQGDraggable> _DragMoveEventHandlerProperty =
            VariableIdentifier<IQGDraggable>.Create("_DragMoveEventHandler");

        private EventHandler<QGDragEventArgs>[] _DragMoveEventHandler
        {
            get => (EventHandler<QGDragEventArgs>[])(GetInstance(_DragMoveEventHandlerProperty));
            set => SetInstance(_DragMoveEventHandlerProperty, value);
        }

        public sealed void AddDragMoveEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => _DragMoveEventHandler[(int)actionType] += eventHandler;
        public sealed void RemoveDragMoveEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => _DragMoveEventHandler[(int)actionType] -= eventHandler;
        protected sealed void RaiseDragMoveEvent(QGDragEventArgs e, QGDragParameterType actionType)
            => _DragMoveEventHandler[(int)actionType]?.Invoke(this, e);

        private static readonly VariableIdentifier<IQGDraggable> _DragStopEventHandlerProperty =
            VariableIdentifier<IQGDraggable>.Create("_DragStopEventHandler");

        private EventHandler<QGDragEventArgs>[] _DragStopEventHandler
        {
            get => (EventHandler<QGDragEventArgs>[])(GetInstance(_DragStopEventHandlerProperty));
            set => SetInstance(_DragStopEventHandlerProperty, value);
        }

        public sealed void AddDragStopEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => _DragStopEventHandler[(int)actionType] += eventHandler;
        public sealed void RemoveDragStopEventHandler(EventHandler<QGDragEventArgs> eventHandler, QGDragParameterType actionType)
            => _DragStopEventHandler[(int)actionType] -= eventHandler;
        protected sealed void RaiseDragStopEvent(QGDragEventArgs e, QGDragParameterType actionType)
            => _DragStopEventHandler[(int)actionType]?.Invoke(this, e);

        public static readonly VariableIdentifier<IQGDraggable> CurrentDragActionProperty =
            VariableIdentifier<IQGDraggable>.Create("CurrentDragAction");

        public sealed QGDragActionType? CurrentDragAction
        {
            get => (QGDragActionType?)(GetInstance(CurrentDragActionProperty));
            protected set => SetInstance(CurrentDragActionProperty, value);
        }

        public sealed bool IsBeingDragged => this.CurrentDragAction != null;

        protected static readonly VariableIdentifier<IQGDraggable> StartPositionProperty =
            VariableIdentifier<IQGDraggable>.Create("StartPosition");

        protected sealed Point StartPosition
        {
            get => (Point)GetInstance(StartPositionProperty);
            set => SetInstance(StartPositionProperty, value);
        }

        public void StartDrag(QGDragActionType dragType)
        {
            QGDragParameterType dragParam = (QGDragParameterType)dragType;
            if (!this.IsDraggable(dragParam))
                return;

            this.CurrentDragAction = dragType;
            this.StartPosition = (this as IQGControl)?.Position ?? new Point(0, 0);

            QGDragEventArgs args = new QGDragEventArgs(this.StartPosition, this.StartPosition, dragType);
            this.RaiseDragStartEvent(args, QGDragParameterType.All);
            this.RaiseDragStartEvent(args, dragParam);

            _StartPositionDictionary.Clear();
            foreach (IQGControl c in this.GetProxyTargets(QGDragParameterType.All).Concat(this.GetProxyTargets(dragParam)))
            {
                _StartPositionDictionary.Add(c, c.Position);
                if (c is IQGDraggable cDraggable)
                {
                    if (!cDraggable.IsDraggable(QGDragParameterType.Proxy))
                        throw new Exception("Cannot drag control as a proxy.");

                    cDraggable.StartDrag(QGDragActionType.Proxy);
                }
            }

            if (this.CurrentDragAction == QGDragActionType.Active)
            {
                foreach (IQGDraggable item in IQGSelectable.SelectedElements.OfType<IQGDraggable>().Where(el => el != this))
                {
                    if (!item.IsDraggable(QGDragParameterType.Passive) || !(item is IQGControl c) || _StartPositionDictionary.ContainsKey(c))
                        continue;

                    _StartPositionDictionary.Add(c, c.Position);
                    if (item.IsDraggable(QGDragParameterType.Passive))
                        item.StartDrag(QGDragActionType.Passive);
                }
            }
        }

        public void ContinueDrag(Vector displacement, QGDragActionType dragType)
        {
            QGDragParameterType dragParam = (QGDragParameterType)dragType;
            if (this.CurrentDragAction != dragType)
                return;

            Point thisPosition = this.StartPosition;
            if (this.IsDragMobile(dragParam))
            {
                thisPosition += displacement;
                (this as IQGControl)?.SetPosition(thisPosition);
            }

            QGDragEventArgs args = new QGDragEventArgs(this.StartPosition, thisPosition, dragType);
            this.RaiseDragMoveEvent(args, QGDragParameterType.All);
            this.RaiseDragMoveEvent(args, dragParam);

            foreach (var kvpair in _StartPositionDictionary)
            {
                (IQGControl c, Point startPosition) = (kvpair.Key, kvpair.Value);

                if (c is IQGDraggable cDraggable)
                    cDraggable.ContinueDrag(displacement, (QGDragActionType)cDraggable.CurrentDragAction);
                else
                    c.SetPosition(startPosition + displacement);
            }
        }

        public void StopDrag(Vector displacement, QGDragActionType dragType)
        {
            QGDragParameterType dragParam = (QGDragParameterType)dragType;
            if (this.CurrentDragAction != dragType)
                return;

            this.CurrentDragAction = null;

            Point thisPosition = this.StartPosition;
            if (this.IsDragMobile(dragParam))
            {
                thisPosition += displacement;
                (this as IQGControl)?.SetPosition(thisPosition);
            }

            QGDragEventArgs args = new QGDragEventArgs(this.StartPosition, thisPosition, dragType);
            this.RaiseDragStopEvent(args, QGDragParameterType.All);
            this.RaiseDragStopEvent(args, dragParam);

            foreach (var kvpair in _StartPositionDictionary)
            {
                (IQGControl c, Point startPosition) = (kvpair.Key, kvpair.Value);

                if (c is IQGDraggable cDraggable)
                    cDraggable.StopDrag(displacement, (QGDragActionType)cDraggable.CurrentDragAction);
                else
                    c.SetPosition(startPosition + displacement);
            }
        }

        void IInterfaceStateAgent<IQGDraggable>.Constructor()
        {
            _StartPositionDictionary = new Dictionary<IQGControl, Point>();
            _ActionDraggability = new bool[3] { true, true, true };
            _ActionDragMobility = new bool[3] { true, true, true };
            IEnumerable<IQGControl> empty = Enumerable.Empty<IQGControl>();
            _ActionProxyTargetLists = new IEnumerable<IQGControl>[4] { empty, empty, empty, empty };
            _DragStartEventHandler = new EventHandler<QGDragEventArgs>[4] { null, null, null, null };
            _DragMoveEventHandler = new EventHandler<QGDragEventArgs>[4] { null, null, null, null };
            _DragStopEventHandler = new EventHandler<QGDragEventArgs>[4] { null, null, null, null };
            this.CurrentDragAction = null;
        }
    }

    public class QGDragEventArgs : QGControlEventArgs
    {
        public Point DragOrigin { get; private set; }
        public Point CurrentPosition { get; private set; }
        public QGDragActionType DragType { get; private set; }

        public Vector Displacement => this.CurrentPosition - this.DragOrigin;

        public QGDragEventArgs(Point origin, Point current, QGDragActionType type)
        {
            this.DragOrigin = origin;
            this.CurrentPosition = current;
            this.DragType = type;
        }
    }
}
