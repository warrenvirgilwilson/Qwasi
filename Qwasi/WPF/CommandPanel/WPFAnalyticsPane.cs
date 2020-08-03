using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using Qwasi.GraphUI;
using Qwasi.HilbertSpaceMath;
using Qwasi.WPF.Controls;

namespace Qwasi.WPF
{
    public class WPFAnalyticsPane : WPFContentPane
    {
        public WPFLabeledIntField StepIncrementField { get; } = new WPFLabeledIntField();
        public WPFLabeledIntField StepRangeStartField { get; } = new WPFLabeledIntField();
        public WPFLabeledIntField StepRangeEndField { get; } = new WPFLabeledIntField();
        public WPFButton CompileAnalyticsButton { get; } = new WPFButton();

        protected QwasiEngine QwasiEngine { get; private set; }

        public void SetQwasiEngine(QwasiEngine qwasiEngine)
        {
            this.QwasiEngine = qwasiEngine;
        }

        public WPFAnalyticsPane()
            : base()
        {
            this.TitleText = "Analytics";

            this.StepIncrementField.Content = "Step Increment:";
            this.StepIncrementField.FieldText = "1";
            this.StepIncrementField.PartitionAlignment = PartitionAlignment.Right;
            this.StepIncrementField.PartitionLocation = 50;
            this.StepIncrementField.PartitionMetric = GridUnitType.Star;
            this.Items.Add(this.StepIncrementField);

            this.StepRangeStartField.Content = "Step Range Start:";
            this.StepRangeStartField.FieldText = "0";
            this.StepRangeStartField.PartitionAlignment = PartitionAlignment.Right;
            this.StepRangeStartField.PartitionLocation = 50;
            this.StepRangeStartField.PartitionMetric = GridUnitType.Star;
            this.Items.Add(this.StepRangeStartField);

            this.StepRangeEndField.Content = "Step Range End:";
            this.StepRangeEndField.FieldText = "10";
            this.StepRangeEndField.PartitionAlignment = PartitionAlignment.Right;
            this.StepRangeEndField.PartitionLocation = 50;
            this.StepRangeEndField.PartitionMetric = GridUnitType.Star;
            this.Items.Add(this.StepRangeEndField);

            this.CompileAnalyticsButton.Content = "Compile Analytics";
            this.CompileAnalyticsButton.Click += CompileAnalyticsButton_Click;
            this.Items.Add(this.CompileAnalyticsButton);
        }

        private void CompileAnalyticsButton_Click(object sender, RoutedEventArgs e)
        {
            int stepInterval = this.StepIncrementField.ParsedValue;
            int stepRangeStart = this.StepRangeStartField.ParsedValue;
            int stepRangeEnd = this.StepRangeEndField.ParsedValue;
            this.QwasiEngine.CompileAnalytics(stepInterval, stepRangeStart, stepRangeEnd);
        }
    }
}
