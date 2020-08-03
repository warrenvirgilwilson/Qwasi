using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Qwasi.GraphUI;
using Qwasi.WPF.Controls;

namespace Qwasi.WPF
{
    public class WPFUIModePane : WPFContentPane
    {
        public WPFButton EditModeButton { get; } = new WPFButton();
        public WPFButton SimulationModeButton { get; } = new WPFButton();
        public WPFButton AnalyticsModeButton { get; } = new WPFButton();

        protected QwasiEngine QwasiEngine { get; private set; }

        public void SetQwasiEngine(QwasiEngine qwasiEngine)
        {
            this.QwasiEngine = qwasiEngine;
            this.QwasiEngine.ActiveUIModeChanged += __activeUIModeChanged;
            __updateButtons(QwasiEngine.ActiveUIMode);
        }

        private void __activeUIModeChanged(object sender, QwasiUIEventArgs e)
        {
            __updateButtons(e.UIMode);
        }

        private WPFButton __getUIModeButton(QwasiUIMode? uiMode)
        {
            switch (uiMode)
            {
                case QwasiUIMode.Edit:
                    return this.EditModeButton;
                case QwasiUIMode.Simulation:
                    return this.SimulationModeButton;
                case QwasiUIMode.Analytics:
                    return this.AnalyticsModeButton;
                case null:
                    return null;
            }

            throw new Exception("UI mode not supported.");
        }

        private QwasiUIMode? _currentUIMode = null;
        private Brush _inactiveBackgroundBrush = null;
        private void __updateButtons(QwasiUIMode uiMode)
        {
            if (uiMode == _currentUIMode)
                return;

            Button buttonToActivate = __getUIModeButton(uiMode);
            Button buttonToInactivate = __getUIModeButton(_currentUIMode);
            _currentUIMode = uiMode;

            if (_inactiveBackgroundBrush != null)
            {
                buttonToInactivate.Background = _inactiveBackgroundBrush;
                buttonToInactivate.BorderThickness = new Thickness(1);
                buttonToInactivate.BorderBrush = Brushes.Transparent;
            }

            _inactiveBackgroundBrush = buttonToActivate.Background;
            buttonToActivate.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFBEE6FD"));
            buttonToActivate.BorderThickness = new Thickness(2);
            buttonToActivate.BorderBrush = Brushes.DimGray;
        }

        public WPFUIModePane()
            : base()
        {
            this.TitleText = "User Interface Mode";

            this.EditModeButton.Content = "Edit Mode";
            this.EditModeButton.Click += (o, e) => QwasiEngine.EnterEditMode();
            this.Items.Add(this.EditModeButton);

            this.SimulationModeButton.Content = "Simulation Mode";
            this.SimulationModeButton.Click += (o, e) => QwasiEngine.EnterSimulationMode();
            this.Items.Add(this.SimulationModeButton);

            this.AnalyticsModeButton.Content = "Analytics Mode";
            this.AnalyticsModeButton.Click += (o, e) => QwasiEngine.EnterAnalyticsMode();
            this.Items.Add(this.AnalyticsModeButton);
        }
    }
}
