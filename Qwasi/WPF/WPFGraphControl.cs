using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using Qwasi.GraphUI;

namespace Qwasi.WPF
{
    public class WPFGraphControl : Grid
    {
        public ScrollViewer ScrollViewer { get; private set; } = new ScrollViewer();
        public Canvas OverlayCanvas { get; private set; } = new Canvas();
        public Canvas RootCanvas { get; private set; } = null;

        private QGRootLayer _RootLayer = null;
        public QGRootLayer RootLayer
        {
            get => _RootLayer;
            set
            {
                if (value == _RootLayer)
                    return;

                _RootLayer = value;
                Canvas rootCanvas = (Canvas)((IWPFContainer)this.RootLayer).WPFPrimaryElement;
                if (rootCanvas == this.RootCanvas)
                    return;

                this.ScrollViewer.Content = this.RootCanvas = rootCanvas;

                rootCanvas.HorizontalAlignment = HorizontalAlignment.Left;
                rootCanvas.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        public WPFGraphControl()
            : base()
        {
            this.RootLayer = new QGRootLayer();

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;

            RowDefinition row1 = new RowDefinition();
            ColumnDefinition col1 = new ColumnDefinition();
            this.RowDefinitions.Add(row1);
            this.ColumnDefinitions.Add(col1);

            this.ScrollViewer.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.ScrollViewer.VerticalAlignment = VerticalAlignment.Stretch;
            this.ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Grid.SetRow(this.ScrollViewer, 0);
            Grid.SetColumn(this.ScrollViewer, 0);
            this.Children.Add(this.ScrollViewer);

            this.OverlayCanvas.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.OverlayCanvas.VerticalAlignment = VerticalAlignment.Stretch;
            Grid.SetRow(this.OverlayCanvas, 0);
            Grid.SetColumn(this.OverlayCanvas, 0);
            this.Children.Add(this.OverlayCanvas);

            this.OverlayCanvas.Background = new SolidColorBrush(Color.FromArgb(50, 0, 255, 0));
            this.OverlayCanvas.Visibility = Visibility.Collapsed;

            this.ScrollViewer.SizeChanged += (o, e) => __setMinimumCanvasSize();
            this.ScrollViewer.Loaded += (o, e) => __setMinimumCanvasSize();
        }

        private void __setMinimumCanvasSize()
        {
            Canvas rootCanvas = (Canvas)((IWPFContainer)this.RootLayer).WPFPrimaryElement;
            rootCanvas.MinWidth = this.ActualWidth - 18 > 0 ? this.ActualWidth - 18 : 0;
            rootCanvas.MinHeight = this.ActualHeight - 18 > 0 ? this.ActualHeight - 18 : 0;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                QGUserInterface.LockGlobalRefresh(this);

                foreach (IQGUserDeletable udcontrol in IQGSelectable.SelectedElements.OfType<IQGUserDeletable>().ToArray())
                    udcontrol.UserIssuedDelete();

                QGUserInterface.UnlockGlobalRefresh(this);
            }

            base.OnKeyUp(e);
        }
    }
}
