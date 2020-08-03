using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Qwasi.GraphUI
{
    public static class QGInput
    {
        private static ICollection<IQGMouseHandler> s_ClickedElementsCollection { get; } = new List<IQGMouseHandler>();
        public static IEnumerable<IQGMouseHandler> ClickedElements => s_ClickedElementsCollection.AsEnumerable();

        public static void AddClickedElement(IQGMouseHandler element) => s_ClickedElementsCollection.Add(element);
        public static void RemoveClickedElement(IQGMouseHandler element) => s_ClickedElementsCollection.Remove(element);
        public static void ClearClickedElements() => s_ClickedElementsCollection.Clear();

        public static IQGMouseHandler MouseButtonDownAsweredBy { get; private set; } = null;
        public static void AnswerMouseButtonDown(IQGMouseHandler element) => MouseButtonDownAsweredBy = element;
        public static void ClearMouseButtonDown() => MouseButtonDownAsweredBy = null;
    }

    public interface IQGMouseHandler : IWPFContainer
    {
        public void InitializeInterface()
        {
            this.RegisterWPFPrimaryElementInitMethod(__WPFInitialize);
        }

        private void __WPFInitialize(object caller, UIElement wpfElement) => this.BindInputToSurface(wpfElement);

        private void BindInputToSurface(UIElement wpfElement)
        {
            wpfElement.PreviewMouseLeftButtonDown += this.PreviewMouseLeftButtonDownHandler;
            wpfElement.MouseLeftButtonDown += this.MouseLeftButtonDownHandler;
            wpfElement.MouseLeftButtonUp += this.MouseLeftButtonUpHandler;
            wpfElement.MouseMove += this.MouseLeftButtonMoveHandler;
        }

        private void UnbindInputToSurface(UIElement wpfElement)
        {
            wpfElement.PreviewMouseLeftButtonDown -= this.PreviewMouseLeftButtonDownHandler;
            wpfElement.MouseLeftButtonDown -= this.MouseLeftButtonDownHandler;
            wpfElement.MouseLeftButtonUp -= this.MouseLeftButtonUpHandler;
            wpfElement.MouseMove -= this.MouseLeftButtonMoveHandler;
        }

        protected virtual void OnPreviewMouseLeftButtonDown(object o, MouseButtonEventArgs e) { }
        protected void PreviewMouseLeftButtonDownHandler(object o, MouseButtonEventArgs e)
        {
            QGInput.AddClickedElement(this);

            this.OnPreviewMouseLeftButtonDown(o, e);
        }

        protected virtual bool ShouldCaptureMouse() => true;
        protected abstract void OnMouseLeftButtonDown(object o, MouseButtonEventArgs e);
        protected void MouseLeftButtonDownHandler(object o, MouseButtonEventArgs e)
        {
            if (QGInput.MouseButtonDownAsweredBy != null)
                return;

            QGInput.AnswerMouseButtonDown(this);
            if (!this.ShouldCaptureMouse())
            {
                QGInput.ClearClickedElements();
                QGInput.ClearMouseButtonDown();
                return;
            }

            ((FrameworkElement)o).CaptureMouse();
            this.OnMouseLeftButtonDown(o, e);
        }

        protected abstract void OnMouseLeftButtonUp(object o, MouseButtonEventArgs e);
        protected void MouseLeftButtonUpHandler(object o, MouseButtonEventArgs e)
        {
            if (QGInput.MouseButtonDownAsweredBy != this)
                return;

            QGInput.ClearClickedElements();
            QGInput.ClearMouseButtonDown();
            ((FrameworkElement)o).ReleaseMouseCapture();

            this.OnMouseLeftButtonUp(o, e);
        }

        protected abstract void OnMouseLeftButtonMove(object o, MouseEventArgs e);
        protected void MouseLeftButtonMoveHandler(object o, MouseEventArgs e)
        {
            if (QGInput.MouseButtonDownAsweredBy != this)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
                this.OnMouseLeftButtonMove(o, e);
        }
    }
}
