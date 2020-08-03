using GeneralMath.Expressions;
using GeneralMath.Expressions.HilbertSpace;
using GeneralMath.Expressions.Vectors;
using Microsoft.Win32;
using Qwasi.GraphUI;
using Qwasi.HilbertSpaceMath;
using Qwasi.Serialization;
using Qwasi.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace Qwasi
{
    public partial class QwasiEngine
    {
        protected IDictionary<QGEdge, QGEdgeAnalyticsLabel> AnalyticsEdgeLabels { get; } = new Dictionary<QGEdge, QGEdgeAnalyticsLabel>();

        public event EventHandler<QwasiAnalyticsEventArgs> AnalyticsDataCompiled;
        protected virtual void OnAnalyticsDataCompiled(QwasiAnalyticsEventArgs e) => this.AnalyticsDataCompiled?.Invoke(this, e);
        private void RaiseAnalyticsDataCompiledEvent(QwasiAnalyticsEventArgs e) => this.OnAnalyticsDataCompiled(e);

        protected WPFPlotFrame PlotFrame { get; private set; }
        public Thickness OverlayMargin { get; set; } = new Thickness(30);

        public QwasiAnalyticsData CurrentAnalyticsData { get; private set; } = null;

        public void EnterAnalyticsMode()
        {
            if (this.ActiveUIMode == QwasiUIMode.Analytics)
                return;

            if (IQGVertex.StartVertex == null)
            {
                MessageBox.Show("Cannot enter analytics mode: no start vertex has been chosen. Please designate a start vertex to continue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            __exitCurrentMode();
            if (this.PlotFrame == null)
                __initializePlotFrame();

            foreach (IQGUserDeletable control in this.AllGraphControls.OfType<IQGUserDeletable>())
                control.UserDeletable = false;

            foreach (QGEdge edge in this.AllEdges)
                edge.UserSelectable = false;

            this.ActiveUIMode = QwasiUIMode.Analytics;
            this.GenerateAnalyticsEdgeLabels();

            this.RaiseActiveUIModeChangedEvent(new QwasiUIEventArgs(this.ActiveUIMode));
        }

        private void __exitAnalyticsMode()
        {
            foreach (var kvp in this.AnalyticsEdgeLabels)
            {
                kvp.Value.Unpin();
                __unbindAnalyticsEdgeLabelEvents(kvp.Key);
            }

            this.ClosePlotFrame();
        }

        protected void GenerateAnalyticsEdgeLabels()
        {
            foreach (QGEdge edge in this.AllEdges)
            {
                QGEdgeAnalyticsLabel edgeLabel;
                if (!this.AnalyticsEdgeLabels.ContainsKey(edge))
                {
                    edgeLabel = new QGEdgeAnalyticsLabel();
                    this.AnalyticsEdgeLabels.Add(edge, edgeLabel);
                    edgeLabel.ThumbnailPlotClicked += (o, e) => this.LoadPlotFrame(e.Edge);
                    edge.BindingLayer.ChildControls.Add(edgeLabel);
                    edge.Deleting += (o, e) => this.AnalyticsEdgeLabels.Remove(edge);
                }
                else
                    edgeLabel = this.AnalyticsEdgeLabels[edge];

                __bindAnalyticsEdgeLabelEvents(edge);
                edgeLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void __bindAnalyticsEdgeLabelEvents(QGEdge edge)
        {
            edge.Refreshed += __edgeAnalytics_Refreshed;
            edge.MouseEnter += __edgeAnalytics_MouseEnter;
            edge.MouseLeave += __edgeAnalytics_MouseLeave;
        }

        private void __unbindAnalyticsEdgeLabelEvents(QGEdge edge)
        {
            edge.Refreshed -= __edgeAnalytics_Refreshed;
            edge.MouseEnter -= __edgeAnalytics_MouseEnter;
            edge.MouseLeave -= __edgeAnalytics_MouseLeave;
        }

        private void __edgeAnalytics_Refreshed(object sender, QGEdgeEventArgs e) => this.AnalyticsEdgeLabels[(QGEdge)sender].Refresh();
        private void __edgeAnalytics_MouseEnter(object sender, MouseEventArgs e) => this.AnalyticsEdgeLabels[(QGEdge)sender].Show();
        private void __edgeAnalytics_MouseLeave(object sender, MouseEventArgs e) => this.AnalyticsEdgeLabels[(QGEdge)sender].TimedHide();

        public QwasiAnalyticsData CompileAnalytics(int stepInterval, int stepRangeStart, int stepRangeEnd)
        {
            if (stepRangeEnd < stepRangeStart)
                return new QwasiAnalyticsData();

            __initializeMath();
            _currentStep = (IHSExplicitVectorValue<HSEdgeState>)(IHSExplicitVector<HSEdgeState>.ZeroVector(_hs) + __qwGetStartVector(IQGVertex.StartVertex));
            QwasiAnalyticsData analyticsData = new QwasiAnalyticsData() { StepInterval = stepInterval, MaxStepValue = 0 };
            QwasiEvaluationState currentState = new QwasiEvaluationState(__performStepOperation(stepRangeStart), stepRangeStart);
            analyticsData.StateEvaluationList.Add(currentState);

            int stepCount;
            for (stepCount = stepRangeStart; stepCount + stepInterval <= stepRangeEnd; stepCount += stepInterval)
            {
                currentState = new QwasiEvaluationState(__performStepOperation(stepInterval), stepCount + stepInterval);
                analyticsData.StateEvaluationList.Add(currentState);
            }
            analyticsData.MaxStepValue = stepCount;

            List<QGEdge> updatedEdges = new List<QGEdge>();
            foreach (HSEdgeState edgeState in analyticsData.BasisStates)
            {
                QGEdge edge = edgeState.Vertex1.GetEdge(edgeState.Vertex2);
                if (updatedEdges.Contains(edge))
                    continue;

                this.AnalyticsEdgeLabels[edge].LoadAnalyticsData(analyticsData);

                updatedEdges.Add(edge);
            }

            this.CurrentAnalyticsData = analyticsData;
            this.RaiseAnalyticsDataCompiledEvent(new QwasiAnalyticsEventArgs(analyticsData));

            return analyticsData;
        }

        public void ExportAnalyticsToCSV(TextWriter textWriter, bool includeStepNumbers)
        {
            if (this.CurrentAnalyticsData == null || this.CurrentAnalyticsData.StateEvaluationCount == 0)
            {
                MessageBox.Show("Cannot export analytics to CSV file because no analytics data has been compiled. Please compile data first.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (includeStepNumbers)
                textWriter.Write("Step #,");
            HSEdgeState[] basisStates = this.CurrentAnalyticsData.BasisStates.ToArray();
            for (int i = 0; i < basisStates.Length; i++)
                textWriter.Write("\"" + basisStates[i].ToString() + "\"" + (i < (basisStates.Length - 1) ? "," : ""));
            textWriter.WriteLine();

            foreach (QwasiEvaluationState evalState in this.CurrentAnalyticsData.StateEvaluationList)
            {
                if (includeStepNumbers)
                    textWriter.Write(evalState.StepNumber.ToString() + ",");
                for (int i = 0; i < basisStates.Length; i++)
                    textWriter.Write("\"" + evalState.GetCoefficient(basisStates[i]).ToString() + "\"" + (i < (basisStates.Length - 1) ? "," : ""));

                textWriter.WriteLine();
            }
        }

        private void __initializePlotFrame()
        {
            this.PlotFrame = new WPFPlotFrame(this);

            this.WPFGraphControl.OverlayCanvas.Children.Add(this.PlotFrame);
            this.WPFGraphControl.OverlayCanvas.SizeChanged += (o, e) => __updatePlotFrameDimensions();

            this.AnalyticsDataCompiled += __updatePlotFrameData;

            this.PlotFrame.Hide();
            __updatePlotFrameDimensions();
        }

        private void __updatePlotFrameData(object sender, QwasiAnalyticsEventArgs e)
        {
            if (this.PlotFrame.Visible)
                this.PlotFrame.ReloadAnalyticsData(this.CurrentAnalyticsData);
        }

        private void __updatePlotFrameDimensions()
        {
            Canvas.SetLeft(this.PlotFrame, this.OverlayMargin.Left);
            Canvas.SetTop(this.PlotFrame, this.OverlayMargin.Top);

            double pfWidth = this.WPFGraphControl.OverlayCanvas.ActualWidth - this.OverlayMargin.Left - this.OverlayMargin.Right;
            double pfHeight = this.WPFGraphControl.OverlayCanvas.ActualHeight - this.OverlayMargin.Top - this.OverlayMargin.Bottom;

            this.PlotFrame.Width = pfWidth > 0 ? pfWidth : 0;
            this.PlotFrame.Height = pfHeight > 0 ? pfHeight : 0;
        }

        public void LoadPlotFrame(QGEdge edge)
        {
            this.WPFGraphControl.OverlayCanvas.Background = Brushes.Gray.Clone();
            this.WPFGraphControl.OverlayCanvas.Background.Opacity = 0.2;
            this.WPFGraphControl.OverlayCanvas.Visibility = Visibility.Visible;
            this.PlotFrame.Show(this.CurrentAnalyticsData, edge);
        }

        public void ClosePlotFrame()
        {
            this.PlotFrame.Hide();
            this.WPFGraphControl.OverlayCanvas.Visibility = Visibility.Collapsed;
        }
    }

    public class QwasiAnalyticsData
    {
        public int StepInterval { get; set; }
        public int MaxStepValue { get; set; }
        public List<QwasiEvaluationState> StateEvaluationList { get; } = new List<QwasiEvaluationState>();

        public int StateEvaluationCount => StateEvaluationList?.Count ?? 0;

        public IEnumerable<HSEdgeState> BasisStates => this.StateEvaluationList.FirstOrDefault()?.BasisStates;

        public IEnumerable<double> GetEvaluatedStepValues(HSEdgeState basisState)
        {
            return this.StateEvaluationList.Select(v => v.GetCoefficient(basisState));
        }

        public IEnumerable<double> this[HSEdgeState basisState] => this.GetEvaluatedStepValues(basisState);
    }

    public class QwasiAnalyticsEventArgs : EventArgs
    {
        public QwasiAnalyticsData AnalyticsData { get; }

        public QwasiAnalyticsEventArgs(QwasiAnalyticsData analyticsData)
        {
            this.AnalyticsData = analyticsData;
        }
    }
}
