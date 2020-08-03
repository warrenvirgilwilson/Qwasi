using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using Qwasi.GraphUI;
using Qwasi.WPF.Controls;

namespace Qwasi.WPF
{
    public class WPFSimulationPane : WPFContentPane
    {
        public WPFButtonIntField StepFB { get; } = new WPFButtonIntField();
        public WPFButton ResetButton { get; } = new WPFButton();

        protected QwasiEngine QwasiEngine { get; private set; }

        public void SetQwasiEngine(QwasiEngine qwasiEngine)
        {
            this.QwasiEngine = qwasiEngine;
        }

        public WPFSimulationPane()
            : base()
        {
            this.TitleText = "Simulation";

            this.StepFB.Content = "Step (# of times):";
            this.StepFB.FieldText = "1";

            this.StepFB.PartitionAlignment = PartitionAlignment.Right;
            this.StepFB.PartitionLocation = 40;
            this.StepFB.PartitionMetric = GridUnitType.Pixel;
            this.StepFB.Click += StepFB_Click;
            this.Items.Add(this.StepFB);

            this.ResetButton.Content = "Reset Simulation";
            this.ResetButton.Click += (o, e) => this.QwasiEngine.ResetSimulation();
            this.Items.Add(this.ResetButton);
        }

        private void StepFB_Click(object sender, RoutedEventArgs e)
        {
            this.QwasiEngine.EvaluateSimulationStep(this.StepFB.ParsedValue);
        }
    }
}
