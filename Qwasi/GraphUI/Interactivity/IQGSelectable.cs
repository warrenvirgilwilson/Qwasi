using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qwasi.GraphUI
{
    public interface IQGSelectable : IInterfaceStateAgent<IQGSelectable>
    {
        private static ICollection<IQGSelectable> s_SelectedElements { get; } = new HashSet<IQGSelectable>();
        public static IEnumerable<IQGSelectable> SelectedElements => s_SelectedElements;

        public static event EventHandler<QGSelectionEventArgs> SelectedElementsChanged;

        private static void AddSelectedElement(IQGSelectable element)
        {
            s_SelectedElements.Add(element);
            SelectedElementsChanged?.Invoke(null, new QGSelectionEventArgs());
        }

        private static void RemoveSelectedElement(IQGSelectable element)
        {
            s_SelectedElements.Remove(element);
            SelectedElementsChanged?.Invoke(null, new QGSelectionEventArgs());
        }

        public static bool IsElementSelected(IQGSelectable element) => s_SelectedElements.Contains(element);

        public static void ClearSelectedElements() => ClearSelectedElements(Array.Empty<IQGSelectable>());
        public static void ClearSelectedElements(params IQGSelectable[] except) => ClearSelectedElements((IEnumerable<IQGSelectable>)except);
        public static void ClearSelectedElements(IEnumerable<IQGSelectable> except)
        {
            foreach (IQGSelectable item in s_SelectedElements.Except(except).ToArray())
                item.Unselect();
        }

        public sealed bool IsSelected => IQGSelectable.IsElementSelected(this);

        public static readonly VariableIdentifier<IQGSelectable> UserSelectableProperty =
            VariableIdentifier<IQGSelectable>.Create("UserSelectable");

        public sealed bool UserSelectable
        {
            get => (bool)GetInstance(UserSelectableProperty, true);
            set
            {
                if (this.UserSelectable && !value)
                    this.Unselect();

                SetInstance(UserSelectableProperty, value);
            }
        }

        public static readonly EventIdentifier<IQGSelectable> SelectionGainedEvent =
            EventIdentifier<IQGSelectable>.Create("SelectionGained");

        protected void RaiseSelectionGainedEvent(QGSelectionEventArgs e) => RaiseEvent(SelectionGainedEvent, e);
        public event EventHandler<QGSelectionEventArgs> SelectionGained
        {
            add { AddEventHandler(SelectionGainedEvent, value); }
            remove { RemoveEventHandler(SelectionGainedEvent, value); }
        }

        public static readonly EventIdentifier<IQGSelectable> SelectionLostEvent =
            EventIdentifier<IQGSelectable>.Create("SelectionLost");

        protected void RaiseSelectionLostEvent(QGSelectionEventArgs e) => RaiseEvent(SelectionLostEvent, e);
        public event EventHandler<QGSelectionEventArgs> SelectionLost
        {
            add { AddEventHandler(SelectionLostEvent, value); }
            remove { RemoveEventHandler(SelectionLostEvent, value); }
        }

        public virtual void Select()
        {
            if (!this.UserSelectable || this.IsSelected)
                return;

            IQGSelectable.AddSelectedElement(this);
            this.RaiseSelectionGainedEvent(new QGSelectionEventArgs());
        }

        public virtual void Unselect()
        {
            if (!this.IsSelected)
                return;

            IQGSelectable.RemoveSelectedElement(this);
            this.RaiseSelectionLostEvent(new QGSelectionEventArgs());
        }
    }

    public class QGSelectionEventArgs : EventArgs
    {
        public IEnumerable<IQGSelectable> AllSelectedElements => IQGSelectable.SelectedElements;
    }
}
