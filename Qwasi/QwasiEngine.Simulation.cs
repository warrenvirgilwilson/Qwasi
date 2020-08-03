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
        protected IDictionary<QGEdge, QGEdgeSimulationLabel> SimulationEdgeLabels { get; } = new Dictionary<QGEdge, QGEdgeSimulationLabel>();

        public event EventHandler<QwasiSimulationStepEventArgs> SimulationStarted;
        protected virtual void OnSimulationStarted(QwasiSimulationStepEventArgs e) => this.SimulationStarted?.Invoke(this, e);
        private void RaiseSimulationStartedEvent(QwasiSimulationStepEventArgs e) => this.OnSimulationStarted(e);

        public event EventHandler<QwasiSimulationStepEventArgs> SimulationStepPerformed;
        protected virtual void OnSimulationStepPerformed(QwasiSimulationStepEventArgs e) => this.SimulationStepPerformed?.Invoke(this, e);
        private void RaiseSimulationStepPerformedEvent(QwasiSimulationStepEventArgs e) => this.OnSimulationStepPerformed(e);

        public QwasiEvaluationState CurrentSimulationState { get; private set; } = null;

        public void EnterSimulationMode()
        {
            if (this.ActiveUIMode == QwasiUIMode.Simulation)
                return;

            if (IQGVertex.StartVertex == null)
            {
                MessageBox.Show("Cannot enter simulation mode: no start vertex has been chosen. Please designate a start vertex to continue.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            __exitCurrentMode();

            foreach (IQGUserDeletable control in this.AllGraphControls.OfType<IQGUserDeletable>())
                control.UserDeletable = false;

            foreach (QGEdge edge in this.AllEdges)
                edge.UserSelectable = false;

            this.ActiveUIMode = QwasiUIMode.Simulation;
            this.InitializeSimulation();

            this.RaiseActiveUIModeChangedEvent(new QwasiUIEventArgs(this.ActiveUIMode));
        }

        private void __exitSimulationMode()
        {
            foreach (var kvp in this.SimulationEdgeLabels)
            {
                kvp.Value.Unpin();
                __unbindSimulationEdgeLabelEvents(kvp.Key);
            }

            foreach (QGEdge edge in this.AllEdges)
                edge.ReleaseColorOverride();

            _hs = null;
            _recurrenceVector = null;
            _currentStep = null;
            this.CurrentSimulationState = null;
        }

        protected void GenerateSimulationEdgeLabels()
        {
            foreach (QGEdge edge in this.AllEdges)
            {
                QGEdgeSimulationLabel edgeLabel;
                if (!this.SimulationEdgeLabels.ContainsKey(edge))
                {
                    edgeLabel = new QGEdgeSimulationLabel();
                    this.SimulationEdgeLabels.Add(edge, edgeLabel);
                    edge.BindingLayer.ChildControls.Add(edgeLabel);
                    edge.Deleting += (o, e) => this.SimulationEdgeLabels.Remove(edge);
                }
                else
                    edgeLabel = this.SimulationEdgeLabels[edge];

                __bindSimulationEdgeLabelEvents(edge);
                edgeLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void __bindSimulationEdgeLabelEvents(QGEdge edge)
        {
            edge.Refreshed += __edgeSimulation_Refreshed;
            edge.MouseEnter += __edgeSimulation_MouseEnter;
            edge.MouseLeave += __edgeSimulation_MouseLeave;
        }

        private void __unbindSimulationEdgeLabelEvents(QGEdge edge)
        {
            edge.Refreshed -= __edgeSimulation_Refreshed;
            edge.MouseEnter -= __edgeSimulation_MouseEnter;
            edge.MouseLeave -= __edgeSimulation_MouseLeave;
        }

        private void __edgeSimulation_Refreshed(object sender, QGEdgeEventArgs e) => this.SimulationEdgeLabels[(QGEdge)sender].Refresh();
        private void __edgeSimulation_MouseEnter(object sender, MouseEventArgs e) => this.SimulationEdgeLabels[(QGEdge)sender].Show();
        private void __edgeSimulation_MouseLeave(object sender, MouseEventArgs e) => this.SimulationEdgeLabels[(QGEdge)sender].TimedHide();

        protected void InitializeSimulation()
        {
            this.GenerateSimulationEdgeLabels();
            __initializeMath();

            this.ResetSimulation();
        }

        public void ResetSimulation()
        {
            if (_hs == null || _recurrenceVector == null)
                throw new Exception("Cannot reset simulation because it has not yet been initialized.");

            _totalStepCount = 0;
            _currentStep = (IHSExplicitVectorValue<HSEdgeState>)(IHSExplicitVector<HSEdgeState>.ZeroVector(_hs) + __qwGetStartVector(IQGVertex.StartVertex));
            this.CurrentSimulationState = new QwasiEvaluationState(_currentStep, _totalStepCount);
            
            this.RaiseSimulationStartedEvent(new QwasiSimulationStepEventArgs(this.CurrentSimulationState, 0, _totalStepCount));
            this.ApplySimulationGraphVisualizations();
        }

        public void EvaluateSimulationStep(int stepCount)
        {
            if (_recurrenceVector == null)
                throw new Exception("The recurrence relation for the step function has not been created.");

            if (_currentStep == null)
                throw new Exception("The start vector for the compiled Hilbert space has not been determined.");

            _totalStepCount += stepCount;
            this.CurrentSimulationState = new QwasiEvaluationState(__performStepOperation(stepCount), _totalStepCount);

            this.RaiseSimulationStepPerformedEvent(new QwasiSimulationStepEventArgs(this.CurrentSimulationState, stepCount, _totalStepCount));
            this.ApplySimulationGraphVisualizations();
        }

        protected void ApplySimulationGraphVisualizations()
        {
            Dictionary<QGEdge, double> edges = new Dictionary<QGEdge, double>();
            foreach (var term in this.CurrentSimulationState.VectorTerms)
            {
                QGEdge edge = term.BasisVector.Vertex1.GetEdge(term.BasisVector.Vertex2);
                this.SimulationEdgeLabels[edge].SetEdgeStateCoefficient(term.BasisVector, term.Coefficient.ToString());
            }

            foreach (var term in this.CurrentSimulationState.EdgeProbabilities)
            {
                var color = __generateColor(term.Probability);
                Brush lineBrush = new SolidColorBrush(Color.FromRgb((byte)(255 * color.R), (byte)(255 * color.G), (byte)(255 * color.B)));
                term.Edge.SetColorOverride(lineBrush, Brushes.Red, null, null);
            }
        }

        private (double R, double G, double B) __generateColor(double prob)
        {
            double adjustedProb = 1.0d - Math.Sqrt(prob);
            double r = adjustedProb >= 0.5d ? 1.0d : adjustedProb / 0.5d;
            double gb = adjustedProb >= 0.5d ? (adjustedProb - 0.5d) / 0.5d : 0.0d;

            return (r, gb, gb);
        }
    }

    public class QwasiSimulationStepEventArgs : EventArgs
    {
        public QwasiEvaluationState SimulationState { get; }
        public int StepCountOfOperation { get; }
        public int TotalStepCount { get; }

        public QwasiSimulationStepEventArgs(QwasiEvaluationState simulationState, int opStepCount, int totalStepCount)
        {
            this.SimulationState = simulationState;
            this.StepCountOfOperation = opStepCount;
            this.TotalStepCount = totalStepCount;
        }
    }
}
