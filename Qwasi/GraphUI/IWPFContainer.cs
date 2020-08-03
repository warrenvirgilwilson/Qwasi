using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Qwasi.GraphUI
{
    public delegate void WPFElementActionDelegate(object caller, UIElement wpfElement);

    public interface IWPFContainer : IInterfaceStateAgent<IWPFContainer>
    {
        public static readonly VariableIdentifier<IWPFContainer> WPFPrimaryElementProperty = VariableIdentifier<IWPFContainer>.Create("WPFPrimaryElement");
        public sealed UIElement WPFPrimaryElement
        {
            get { return (UIElement)GetInstance(WPFPrimaryElementProperty); }
            private set { SetInstance(WPFPrimaryElementProperty, value); }
        }

        private static readonly VariableIdentifier<IWPFContainer> WPFElementInfoCollectionProperty =
            VariableIdentifier<IWPFContainer>.Create("_WPFElementInfoCollection");
        private WPFElementInfoCollection _WPFElementInfoCollection
        {
            get { return (WPFElementInfoCollection)GetInstance(WPFElementInfoCollectionProperty, new WPFElementInfoCollection()); }
            set { SetInstance(WPFElementInfoCollectionProperty, value); }
        }

        public static readonly EventIdentifier<IWPFContainer> WPFInitializeElementEvent = EventIdentifier<IWPFContainer>.Create("WPFInitializeElement");
        private event EventHandler<WPFInitEventArgs> WPFInitializeElement
        {
            add { AddEventHandler(WPFInitializeElementEvent, value); }
            remove { RemoveEventHandler(WPFInitializeElementEvent, value); }
        }

        public sealed void RegisterWPFElementInitMethod(WPFElementActionDelegate method)
        {
            this.WPFInitializeElement += (o, e) => method(o, e.WPFElement);
            foreach (UIElement element in this.WPFElements)
                method(this, element);
        }

        private void __registerWPFElement(WPFElementInfo elementInfo)
        {
            _WPFElementInfoCollection.Add(elementInfo);
            RaiseEvent(WPFInitializeElementEvent, new WPFInitEventArgs(elementInfo.UIElement));
        }

        public sealed void RegisterWPFElement(UIElement element) => __registerWPFElement(new WPFElementInfo(element));
        public sealed void RegisterWPFElement(UIElement element, Vector positionOffset) => __registerWPFElement(new WPFElementInfo(element, positionOffset));

        public sealed void SetWPFPrimaryElement(UIElement element)
        {
            if (!_WPFElementInfoCollection.Contains(element))
                throw new Exception("Primary element must exist amongst those already registered.");

            UIElement currentElement = this.WPFPrimaryElement;
            if (element.Equals(currentElement))
                return;

            this.WPFPrimaryElement = element;
            RaiseEvent(WPFInitializePrimaryElementEvent, new WPFInitEventArgs(element));
        }

        public static readonly EventIdentifier<IWPFContainer> WPFInitializePrimaryElementEvent =
            EventIdentifier<IWPFContainer>.Create("WPFInitializePrimaryElement");

        private event EventHandler<WPFInitEventArgs> WPFInitializePrimaryElement
        {
            add { AddEventHandler(WPFInitializePrimaryElementEvent, value); }
            remove { RemoveEventHandler(WPFInitializePrimaryElementEvent, value); }
        }

        public sealed void RegisterWPFPrimaryElementInitMethod(WPFElementActionDelegate method)
        {
            this.WPFInitializePrimaryElement += (o, e) => method(o, e.WPFElement);
            if (this.WPFPrimaryElement != null)
                method(this, this.WPFPrimaryElement);
        }

        public sealed WPFElementInfo WPFRetrieveElementInfo(UIElement element)
        {
            return _WPFElementInfoCollection[element];
        }

        public sealed IEnumerable<WPFElementInfo> WPFElementInfoEnumeration => _WPFElementInfoCollection.AsEnumerable();
        public sealed IEnumerable<UIElement> WPFElements => _WPFElementInfoCollection.Select(item => item.UIElement);
    }

    public class WPFElementInfo
    {
        public UIElement UIElement { get; }
        public Vector PositionOffset { get; set; } = new Vector(0, 0);

        public Size Size => (this.UIElement is FrameworkElement fe) ? new Size(fe.Width, fe.Height) : this.UIElement.RenderSize;

        public WPFElementInfo(UIElement element)
        {
            this.UIElement = element;
        }

        public WPFElementInfo(UIElement element, Vector positionOffset)
            : this(element)
        {
            this.PositionOffset = positionOffset;
        }
    }

    public class WPFElementInfoCollection : System.Collections.ObjectModel.KeyedCollection<UIElement, WPFElementInfo>
    {
        protected override UIElement GetKeyForItem(WPFElementInfo item) => item.UIElement;
    }

    public class WPFInitEventArgs : EventArgs
    {
        public UIElement WPFElement { get; private set; }

        public WPFInitEventArgs(UIElement wpfElement)
        {
            this.WPFElement = wpfElement;
        }
    }
}