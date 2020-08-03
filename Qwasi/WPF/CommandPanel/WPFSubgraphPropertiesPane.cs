using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Qwasi.GraphUI;
using Qwasi.WPF.Controls;

namespace Qwasi.WPF
{
    public class WPFSubgraphPropertiesPane : WPFContentPane
    {
        public WPFLabeledCombobox VertexEnumerationTypeCB { get; } = new WPFLabeledCombobox();
        public WPFLabeledField LayerIDField { get; } = new WPFLabeledField();
        public WPFButtonIntField AddPerimeterVerticesFB { get; } = new WPFButtonIntField();

        protected IQGGraphLayer LoadedGraphLayer { get; private set; } = null;

        public void LoadGraphLayers(IEnumerable<IQGGraphLayer> loadLayers)
        {
            if (loadLayers.Count() != 1)
            {
                this.Inactivate();
                AddPerimeterVerticesFB.Visibility = Visibility.Collapsed;
                return;
            }

            this.Activate();
            this.LoadedGraphLayer = loadLayers.First();
            __loadControl();
        }

        private bool _IsLoading = false;
        private void __loadControl()
        {
            _IsLoading = true;

            this.VertexEnumerationTypeCB.SelectedIndex = (int)this.LoadedGraphLayer.VertexEnumerationType;
            this.LayerIDField.FieldText = this.LoadedGraphLayer.LayerID;
            if (this.LoadedGraphLayer.VertexEnumerationType == QGVertexEnumerationType.Continuous)
            {
                this.LayerIDField.FieldText = "";
                this.LayerIDField.IsEnabled = false;
            }
            else if (this.LoadedGraphLayer.VertexEnumerationType == QGVertexEnumerationType.Subgraph)
            {
                this.LayerIDField.FieldText = this.LoadedGraphLayer.LayerID;
                this.LayerIDField.IsEnabled = true;
            }

            AddPerimeterVerticesFB.Visibility = this.LoadedGraphLayer is QGPerimeterGraph ? Visibility.Visible : Visibility.Collapsed;

            _IsLoading = false;
        }

        public WPFSubgraphPropertiesPane()
            : base()
        {
            this.TitleText = "Subgraph Properties";

            this.VertexEnumerationTypeCB.Content = "Vertex Enumeration:";
            this.VertexEnumerationTypeCB.Items.Add("Continuous");
            this.VertexEnumerationTypeCB.Items.Add("Subgraph");
            this.VertexEnumerationTypeCB.PartitionMetric = GridUnitType.Pixel;
            this.VertexEnumerationTypeCB.PartitionLocation = 105;
            this.VertexEnumerationTypeCB.SelectedIndex = -1;
            this.VertexEnumerationTypeCB.SelectionChanged += __VertexEnumerationTypeCB_SelectionChanged;
            this.Items.Add(this.VertexEnumerationTypeCB);

            this.LayerIDField.Content = "Vertex ID:";
            this.LayerIDField.PartitionMetric = GridUnitType.Pixel;
            this.LayerIDField.PartitionLocation = 105;
            this.LayerIDField.TextChanged += __LayerIDField_TextChanged;
            this.Items.Add(this.LayerIDField);

            this.AddPerimeterVerticesFB.Content = "Add Perimeter Vertices:";
            this.AddPerimeterVerticesFB.FieldText = "1";
            this.AddPerimeterVerticesFB.PartitionAlignment = PartitionAlignment.Right;
            this.AddPerimeterVerticesFB.PartitionLocation = 40;
            this.AddPerimeterVerticesFB.PartitionMetric = GridUnitType.Pixel;
            this.AddPerimeterVerticesFB.Click += __AddPerimeterVerticesFB_Click;
            this.Items.Add(this.AddPerimeterVerticesFB);

            this.LoadGraphLayers(Enumerable.Empty<IQGGraphLayer>());
        }

        private void __VertexEnumerationTypeCB_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_IsLoading)
                return;

            this.LoadedGraphLayer.VertexEnumerationType = (QGVertexEnumerationType)this.VertexEnumerationTypeCB.SelectedIndex;
            if (this.LoadedGraphLayer.VertexEnumerationType == QGVertexEnumerationType.Continuous)
                this.LoadedGraphLayer.LayerID = "";
            __loadControl();
        }

        private void __LayerIDField_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_IsLoading)
                return;

            this.LoadedGraphLayer.LayerID = this.LayerIDField.FieldText;
        }

        private void __AddPerimeterVerticesFB_Click(object sender, RoutedEventArgs e)
        {
            (this.LoadedGraphLayer as QGPerimeterGraph)?.AddPerimeterVertices(this.AddPerimeterVerticesFB.ParsedValue);
        }
    }
}
