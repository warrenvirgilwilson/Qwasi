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
    public class WPFSimulationInfoPane : WPFContentPane
    {
        public WPFControlPair<WPFLabel, WPFLabel> LabelProbabilityControlPair { get; } = new WPFControlPair<WPFLabel, WPFLabel>();
        public WPFLabel TotalProbabilityLabel { get; } = new WPFLabel();
        public WPFLabel TotalProbabilityValueControl { get; } = new WPFLabel();

        public WPFControlPair<WPFLabel, WPFLabel> LabelStepCountPair { get; } = new WPFControlPair<WPFLabel, WPFLabel>();
        public WPFLabel TotalStepCountLabel { get; } = new WPFLabel();
        public WPFLabel TotalStepCountValueControl { get; } = new WPFLabel();

        protected QwasiEngine QwasiEngine { get; private set; }

        public void SetQwasiEngine(QwasiEngine qwasiEngine)
        {
            this.QwasiEngine = qwasiEngine;
            this.QwasiEngine.SimulationStarted += __simulationStarted;
            this.QwasiEngine.SimulationStepPerformed += __simulationStepPerformed;
        }

        private void __simulationStarted(object sender, QwasiSimulationStepEventArgs e) => __loadSimulationState(e);
        private void __simulationStepPerformed(object sender, QwasiSimulationStepEventArgs e) => __loadSimulationState(e);

        private void __loadSimulationState(QwasiSimulationStepEventArgs e)
        {
            this.TotalProbabilityValueControl.Content = e.SimulationState.VectorProbabilities.Sum(t => t.Probability);
            this.TotalStepCountValueControl.Content = e.TotalStepCount;
        }

        public WPFSimulationInfoPane()
            : base()
        {
            this.TitleText = "Simulation Info";

            this.TotalProbabilityLabel.Content = "Total Probability:";
            this.TotalProbabilityLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.TotalProbabilityLabel.HorizontalContentAlignment = HorizontalAlignment.Left;
            this.TotalProbabilityLabel.Background = new SolidColorBrush(Color.FromRgb(235, 235, 235));
            this.TotalProbabilityLabel.BorderThickness = new Thickness(1);
            this.TotalProbabilityLabel.BorderBrush = Brushes.White;

            this.TotalProbabilityValueControl.Content = "0.0";
            this.TotalProbabilityValueControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.TotalProbabilityValueControl.HorizontalContentAlignment = HorizontalAlignment.Right;
            this.TotalProbabilityValueControl.VerticalAlignment = VerticalAlignment.Bottom;
            this.TotalProbabilityValueControl.Background = Brushes.White;

            this.LabelProbabilityControlPair.Control1 = this.TotalProbabilityLabel;
            this.LabelProbabilityControlPair.Control2 = this.TotalProbabilityValueControl;
            this.LabelProbabilityControlPair.PartitionAlignment = PartitionAlignment.Left;
            this.LabelProbabilityControlPair.PartitionMetric = GridUnitType.Star;
            this.LabelProbabilityControlPair.PartitionLocation = 50;
            this.Items.Add(this.LabelProbabilityControlPair);

            this.TotalStepCountLabel.Content = "Total Step Count:";
            this.TotalStepCountLabel.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.TotalStepCountLabel.HorizontalContentAlignment = HorizontalAlignment.Left;
            this.TotalStepCountLabel.Background = new SolidColorBrush(Color.FromRgb(235, 235, 235));
            this.TotalStepCountLabel.BorderThickness = new Thickness(1);
            this.TotalStepCountLabel.BorderBrush = Brushes.White;

            this.TotalStepCountValueControl.Content = "0.0";
            this.TotalStepCountValueControl.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.TotalStepCountValueControl.HorizontalContentAlignment = HorizontalAlignment.Right;
            this.TotalStepCountValueControl.Background = Brushes.White;

            this.LabelStepCountPair.Control1 = this.TotalStepCountLabel;
            this.LabelStepCountPair.Control2 = this.TotalStepCountValueControl;
            this.LabelStepCountPair.PartitionAlignment = PartitionAlignment.Left;
            this.LabelStepCountPair.PartitionMetric = GridUnitType.Star;
            this.LabelStepCountPair.PartitionLocation = 50;
            this.Items.Add(this.LabelStepCountPair);
        }
    }
}
