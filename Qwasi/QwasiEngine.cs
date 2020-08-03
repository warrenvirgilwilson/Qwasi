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
    public enum QwasiUIMode { Edit, Simulation, Analytics }

    public partial class QwasiEngine
    {
        IQGGraphLayer RootLayer => this.WPFGraphControl.RootLayer;

        public IEnumerable<IQGControl> AllGraphControls => this.RootLayer.AllDescendentControls;
        public IEnumerable<IQGVertex> AllVertices => this.AllGraphControls.OfType<IQGVertex>();
        public IEnumerable<QGEdge> AllEdges => this.AllGraphControls.OfType<QGEdge>();

        public WPFGraphControl WPFGraphControl { get; }

        public QwasiUIMode ActiveUIMode { get; private set; } = QwasiUIMode.Edit;
        public event EventHandler<QwasiUIEventArgs> ActiveUIModeChanged;
        protected virtual void OnActiveUIModeChanged(QwasiUIEventArgs e) => this.ActiveUIModeChanged?.Invoke(this, e);
        private void RaiseActiveUIModeChangedEvent(QwasiUIEventArgs e) => this.OnActiveUIModeChanged(e);

        public void EnterEditMode()
        {
            if (this.ActiveUIMode == QwasiUIMode.Edit)
                return;

            __exitCurrentMode();

            foreach (IQGUserDeletable control in this.AllGraphControls.OfType<IQGUserDeletable>())
                control.UserDeletable = true;

            foreach (QGEdge edge in this.AllEdges)
                edge.UserSelectable = true;

            this.ActiveUIMode = QwasiUIMode.Edit;

            this.RaiseActiveUIModeChangedEvent(new QwasiUIEventArgs(this.ActiveUIMode));
        }

        private void __exitCurrentMode()
        {
            if (this.ActiveUIMode == QwasiUIMode.Edit)
                __exitEditMode();
            if (this.ActiveUIMode == QwasiUIMode.Simulation)
                __exitSimulationMode();
            if (this.ActiveUIMode == QwasiUIMode.Analytics)
                __exitAnalyticsMode();
        }

        private void __exitEditMode()
        {
        }

        public IEnumerable<HSEdgeState> CompileHilbertSpace()
        {
            List<HSEdgeState> edgeStates = new List<HSEdgeState>();
            foreach (IQGVertex v in this.AllVertices)
            {
                foreach (QGEdge e in v.GraphEdges)
                {
                    if (e.Vertex1 == v)
                        edgeStates.Add(new HSEdgeState(e.Vertex1, e.Vertex2));
                    else if (e.Vertex2 == v)
                        edgeStates.Add(new HSEdgeState(e.Vertex2, e.Vertex1));
                }
            }

            return edgeStates;
        }

        private IEnumerable<HSEdgeState> _hs = null;
        private IHSExplicitVectorValue<HSEdgeState> _recurrenceVector = null;
        private IHSExplicitVectorValue<HSEdgeState> _currentStep = null;
        private int _totalStepCount = 0;

        private void __initializeMath()
        {
            _hs = this.CompileHilbertSpace();
            var hsVector = IHSExplicitVector<HSEdgeState>.ArbitraryVector(_hs, "hs");
            var op = IHSExplicitVector<HSEdgeState>.CreateLinearOperator(__qwStepFunction, "step");
            _recurrenceVector = (IHSExplicitVectorValue<HSEdgeState>)hsVector.ApplyOperator(op);
        }

        private IHSExplicitVectorValue<HSEdgeState> __performStepOperation(int stepCount)
        {
            for (int i = 0; i < stepCount; i++)
                _currentStep = __step(_currentStep, _recurrenceVector, _hs);

            return _currentStep;
        }

        private IHSExplicitVectorValue<HSEdgeState> __step(
            IHSExplicitVectorValue<HSEdgeState> stateVector,
            IHSExplicitVectorValue<HSEdgeState> walkedVector,
            IEnumerable<HSEdgeState> hs)
        {
            MathContext context = new MathContext();
            foreach (var state in hs)
                context["hs", state] = stateVector[state];

            return (IHSExplicitVectorValue<HSEdgeState>)walkedVector.Evaluate(context);
        }

        private IHSExplicitVectorValue<HSEdgeState> __qwGetStartVector(IQGVertex startVertex)
        {
            List<HSEdgeState> edgeStates = new List<HSEdgeState>();

            double n = startVertex.GraphEdges.Count();
            IAlgebraicExpression coefficient = IAlgebraicExpression.RealNumber(1.0d / Math.Sqrt(n));

            foreach (QGEdge edge in startVertex.GraphEdges)
            {
                HSEdgeState edgeState;
                if (edge.Vertex1 == startVertex)
                    edgeState = new HSEdgeState(edge.Vertex1, edge.Vertex2);
                else
                    edgeState = new HSEdgeState(edge.Vertex2, edge.Vertex1);

                edgeStates.Add(edgeState);
            }

            var vectorTerms = edgeStates.Select(s => new VectorTerm<HSEdgeState>(s, coefficient));
            return IHSExplicitVector<HSEdgeState>.VectorValue(vectorTerms);
        }

        private IHSExplicitVector<HSEdgeState> __qwStepFunction(HSEdgeState basisState)
        {
            List<HSEdgeState> v2edgeStates = new List<HSEdgeState>();

            double n = basisState.Vertex2.GraphEdges.Count();
            IAlgebraicExpression r = IAlgebraicExpression.RealNumber((n - 2.0d) / n);
            IAlgebraicExpression t = IAlgebraicExpression.RealNumber(2.0d / n);

            foreach (QGEdge edge in basisState.Vertex2.GraphEdges)
            {
                HSEdgeState edgeState;
                if (edge.Vertex1 == basisState.Vertex2)
                    edgeState = new HSEdgeState(edge.Vertex1, edge.Vertex2);
                else
                    edgeState = new HSEdgeState(edge.Vertex2, edge.Vertex1);

                // Skip the reflected state
                if (edgeState.Vertex2 != basisState.Vertex1)
                    v2edgeStates.Add(edgeState);
            }

            var vectorTerms = v2edgeStates.Select(s => new VectorTerm<HSEdgeState>(s, !basisState.Vertex2.IsMarked() ? t : -t));
            vectorTerms = vectorTerms.Prepend((new HSEdgeState(basisState.Vertex2, basisState.Vertex1),
                !basisState.Vertex2.IsMarked() ? -r : r));

            return IHSExplicitVector<HSEdgeState>.VectorValue(vectorTerms);
        }

        QXRootLayerSerializer _serializer = new QXRootLayerSerializer();
        public void WriteXml(XmlWriter xmlWriter)
        {
            _serializer.Serialize(this.RootLayer, xmlWriter);
        }

        public void LoadFromXml(XmlReader xmlReader)
        {
            _serializer.IgnoreUnregisteredElements = true;
            this.WPFGraphControl.RootLayer = (QGRootLayer)_serializer.Deserialize(xmlReader);
            this.EnterEditMode();
        }

        public QwasiEngine(WPFGraphControl graphControl)
        {
            this.WPFGraphControl = graphControl;
        }
    }

    public class QwasiEvaluationState
    {
        protected IHSExplicitVectorValue<HSEdgeState> EvaluatedHSStateVector { get; }
        public int StepNumber { get; }

        public IEnumerable<HSEdgeState> BasisStates => this.EvaluatedHSStateVector.Select(v => v.BasisVector);

        public IEnumerable<(HSEdgeState BasisVector, double Coefficient)> VectorTerms
            => this.EvaluatedHSStateVector.Select(v => (v.BasisVector, ((RealNumber)v.Coefficient).Value));

        public double GetCoefficient(HSEdgeState basisState) => ((RealNumber)this.EvaluatedHSStateVector[basisState]).Value;

        private Dictionary<HSEdgeState, double> _VectorProbabilitiesDictionary = null;
        protected Dictionary<HSEdgeState, double> VectorProbabilitiesDictionary
        {
            get
            {
                if (_VectorProbabilitiesDictionary == null)
                {
                    _VectorProbabilitiesDictionary = new Dictionary<HSEdgeState, double>(
                        this.VectorTerms.Select(t => new KeyValuePair<HSEdgeState, double>(t.BasisVector, t.Coefficient * t.Coefficient)));
                }

                return _VectorProbabilitiesDictionary;
            }
        }

        public IEnumerable<(HSEdgeState BasisVector, double Probability)> VectorProbabilities =>
            this.VectorProbabilitiesDictionary.Select(kvp => (kvp.Key, kvp.Value));

        public double GetProbability(HSEdgeState basisState) => this.VectorProbabilitiesDictionary[basisState];

        private Dictionary<QGEdge, double> _EdgeProbabilitiesDictionary = null;
        protected Dictionary<QGEdge, double> EdgeProbabilitiesDictionary
        {
            get
            {
                if (_EdgeProbabilitiesDictionary == null)
                {
                    _EdgeProbabilitiesDictionary = new Dictionary<QGEdge, double>();
                    foreach (var term in this.VectorProbabilities)
                    {
                        QGEdge edge = term.BasisVector.Vertex1.GetEdge(term.BasisVector.Vertex2);
                        _EdgeProbabilitiesDictionary[edge] = _EdgeProbabilitiesDictionary.ContainsKey(edge) ?
                            (_EdgeProbabilitiesDictionary[edge] + term.Probability) : term.Probability;
                    }
                }

                return _EdgeProbabilitiesDictionary;
            }
        }

        public IEnumerable<(QGEdge Edge, double Probability)> EdgeProbabilities => this.EdgeProbabilitiesDictionary.Select(kvp => (kvp.Key, kvp.Value));

        public double GetProbability(QGEdge edge) => this.EdgeProbabilitiesDictionary[edge];

        public QwasiEvaluationState(IHSExplicitVectorValue<HSEdgeState> evaluatedHSStateVector, int stepNumber)
        {
            this.EvaluatedHSStateVector = evaluatedHSStateVector;
            this.StepNumber = stepNumber;
        }
    }

    public class QwasiUIEventArgs : EventArgs
    {
        public QwasiUIMode UIMode { get; }

        public QwasiUIEventArgs(QwasiUIMode uiMode)
        {
            this.UIMode = uiMode;
        }
    }
}
