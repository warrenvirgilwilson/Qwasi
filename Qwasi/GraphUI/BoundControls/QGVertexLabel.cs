using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Qwasi.GraphUI
{
    public class QGVertexLabel : QGLabel
    {
        protected Label WPFLabel { get; private set; } = new Label();
        protected override FrameworkElement WPFInnerControl => this.WPFLabel;

        public object Content
        {
            get => this.WPFLabel.Content;
            set
            {
                // First check that the new value for the label content is not equal to its current value.
                // Changing the Content property is time consuming.
                if (this.WPFLabel.Content == null && value == null)
                    return;
                else if (this.WPFLabel.Content != null && value != null)
                    if (this.WPFLabel.Content.Equals(value))
                        return;

                this.WPFLabel.Content = value;
                this.Refresh();
            }
        }

        /*public Thickness Padding
        {
            get => this.WPFLabel.Padding;
            set => this.WPFLabel.Padding = value;
        }*/

        private QGEdgePair __findGreatestEdgeAngleSeparation(IQGVertex vertex)
        {
            if (vertex.GraphEdges.Count() == 0)
                return null;

            QGEdge startingEdge = vertex.GraphEdges.First();
            Vector startingVector = startingEdge.GetEdgeVector(vertex);
            startingVector.Negate();

            if (vertex.GraphEdges.Count() == 1)
                new QGEdgePair(startingEdge, startingEdge, startingVector, startingVector, vertex, 360);

            double totalAngle = 0;
            SortedList<double, QGEdge> sortedEdges = new SortedList<double, QGEdge>();
            foreach (QGEdge edge in vertex.GraphEdges)
            {
                Vector v = edge.GetEdgeVector(vertex);
                v.Negate();

                totalAngle = Vector.AngleBetween(startingVector, v);
                totalAngle = totalAngle <= 0 ? 360 + totalAngle : totalAngle;

                if (!sortedEdges.ContainsKey(totalAngle))
                    sortedEdges.Add(totalAngle, edge);
            }

            QGEdgePair returnPair = new QGEdgePair(startingEdge, startingEdge, startingVector, startingVector, vertex, 0);

            totalAngle = 0;
            double separationAngle = 0;
            QGEdge e1 = startingEdge;
            Vector v1 = startingVector;

            foreach (var kvp in sortedEdges)
            {
                Vector v2 = kvp.Value.GetEdgeVector(vertex);
                v2.Negate();
                separationAngle = kvp.Key - totalAngle;

                if (separationAngle > returnPair.Angle)
                {
                    returnPair.Edge1 = e1;
                    returnPair.Edge2 = kvp.Value;
                    returnPair.Vector1 = v1;
                    returnPair.Vector2 = v2;
                    returnPair.Angle = separationAngle;
                }

                e1 = kvp.Value;
                v1 = v2;
                totalAngle = kvp.Key;
            }

            return returnPair;
        }

        protected override bool UseDynamicCoordinates
        {
            get
            {
                IQGVertex parentVertex = (this.ParentLayer as QGBindingLayer)?.ParentControl as IQGVertex;
                return base.UseDynamicCoordinates && parentVertex.GraphEdges?.Count() > 0;
            }
        }

        protected override Point GetFixedLabelCoordinates()
        {
            this.HorizontalAlignment = QGHorizontalAlignment.Left;
            return base.GetFixedLabelCoordinates();
        }

        protected override Point GetDynamicLabelCoordinates()
        {
            this.HorizontalAlignment = QGHorizontalAlignment.Center;
            IQGVertex parentVertex = (this.ParentLayer as QGBindingLayer)?.ParentControl as IQGVertex;

            QGEdgePair ep = __findGreatestEdgeAngleSeparation(parentVertex);
            ep.Vector1.Normalize();
            ep.Vector2.Normalize();

            Vector avg = ep.Vector1 + ep.Vector2;

            // If the angle is obtuse, negate one last time
            if (ep.Angle > 180)
                avg.Negate();

            avg.Normalize();
            Vector labelOffset = new Vector(avg.X * this.Width / 2, avg.Y * this.Height / 2);

            return (Point)((avg * 10) + labelOffset);
        }

        protected override void OnInitialized(QGControlEventArgs e)
        {
            base.OnInitialized(e);
            this.Refresh();
        }

        public QGVertexLabel()
            : base()
        {
            this.FixedLabelDisplacement = new Vector(10, -5);

            this.WPFLabel.FontFamily = new FontFamily("Cambria Math");
            this.WPFLabel.FontSize = 11.5;
            this.WPFLabel.FontWeight = FontWeights.Bold;
            this.WPFLabel.Padding = new Thickness(2, 1, 2, 1);
        }

        private class QGEdgePair
        {
            public QGEdge Edge1;
            public QGEdge Edge2;

            public Vector Vector1;
            public Vector Vector2;

            public IQGVertex Vertex;
            public double Angle;

            public QGEdgePair(QGEdge e1, QGEdge e2, Vector v1, Vector v2, IQGVertex vertex, double angle)
            {
                this.Edge1 = e1;
                this.Edge2 = e2;
                this.Vector1 = v1;
                this.Vector2 = v2;
                this.Vertex = vertex;
                this.Angle = angle;
            }
        }
    }
}
