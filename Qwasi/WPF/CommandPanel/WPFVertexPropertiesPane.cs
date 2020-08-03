using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Qwasi.GraphUI;
using Qwasi.WPF.Controls;

namespace Qwasi.WPF
{
    public class WPFVertexPropertiesPane : WPFContentPane
    {
        public WPFLabeledCombobox VertexIDTypeCB { get; } = new WPFLabeledCombobox();
        public WPFLabeledField VertexIDField { get; } = new WPFLabeledField();
        public WPFLeftLabeledCheckbox IsMarkedCheckbox { get; } = new WPFLeftLabeledCheckbox();
        public WPFLeftLabeledCheckbox IsStartVertexCheckbox { get; } = new WPFLeftLabeledCheckbox();

        protected IEnumerable<IQGVertex> LoadedVertices { get; private set; }

        private bool _IsLoading = false;

        public void LoadVertices(IEnumerable<IQGVertex> loadVertices)
        {
            if (loadVertices.Count() == 0)
            {
                this.Inactivate();
                return;
            }

            this.Activate();
            this.LoadedVertices = loadVertices.ToArray();
            __reloadControl();
        }

        private void __reloadControl()
        {
            _IsLoading = true;

            __loadVertexIDTypeCB();
            __loadIsMarkedCheckbox();
            __loadIsStartVertexCheckbox();

            _IsLoading = false;
        }

        private void __loadVertexIDTypeCB()
        {
            if (this.LoadedVertices.Count() != 1)
            {
                this.VertexIDTypeCB.SelectedIndex = -1;
                this.VertexIDTypeCB.IsEnabled = false;
                this.VertexIDField.FieldText = "";
                this.VertexIDField.IsEnabled = false;

                return;
            }

            IQGVertex loadedVertex = this.LoadedVertices.First();
            this.VertexIDTypeCB.SelectedIndex = (int)loadedVertex.VertexIDType;

            if (loadedVertex.VertexIDType == QGVertexIDType.Sequential)
            {
                this.VertexIDField.FieldText = "";
                this.VertexIDField.IsEnabled = false;
            }
            else
            {
                this.VertexIDField.FieldText = loadedVertex.ID as string;
                this.VertexIDField.IsEnabled = true;
            }
        }

        private void __loadIsMarkedCheckbox()
        {
            if (this.LoadedVertices.All(v => v.IsMarked()))
                this.IsMarkedCheckbox.IsChecked = true;
            else if (this.LoadedVertices.Any(v => v.IsMarked()))
                this.IsMarkedCheckbox.IsChecked = null;
            else
                this.IsMarkedCheckbox.IsChecked = false;
        }

        private void __loadIsStartVertexCheckbox()
        {
            this.IsStartVertexCheckbox.IsEnabled = this.LoadedVertices.Count() == 1;
            this.IsStartVertexCheckbox.IsChecked = this.LoadedVertices.Count() == 1 && this.LoadedVertices.First().IsStartVertex();

            if (this.LoadedVertices.Count() != 1)
                this.IsStartVertexCheckbox.IsChecked = this.LoadedVertices.Any(v => v.IsStartVertex()) ? (bool?)null : false;
        }

        public WPFVertexPropertiesPane()
            : base()
        {
            this.TitleText = "Vertex Properties";

            this.VertexIDTypeCB.Content = "Vertex ID Type:";
            this.VertexIDTypeCB.Items.Add("Sequential");
            this.VertexIDTypeCB.Items.Add("Custom Local");
            this.VertexIDTypeCB.Items.Add("Custom Global");
            this.VertexIDTypeCB.PartitionMetric = GridUnitType.Pixel;
            this.VertexIDTypeCB.PartitionLocation = 105;
            this.VertexIDTypeCB.SelectedIndex = -1;
            this.VertexIDTypeCB.SelectionChanged += __VertexIDTypeCB_SelectionChanged;
            this.Items.Add(this.VertexIDTypeCB);

            this.VertexIDField.Content = "Vertex ID:";
            this.VertexIDField.PartitionMetric = GridUnitType.Pixel;
            this.VertexIDField.PartitionLocation = 105;
            this.VertexIDField.TextChanged += __VertexIDField_TextChanged;
            this.Items.Add(this.VertexIDField);

            this.IsMarkedCheckbox.Content = "Mark Vertex:";
            this.IsMarkedCheckbox.Checked += __IsMarkedCheckbox_ValueChanged;
            this.IsMarkedCheckbox.Unchecked += __IsMarkedCheckbox_ValueChanged;
            this.Items.Add(this.IsMarkedCheckbox);

            this.IsStartVertexCheckbox.Content = "Make Start Vertex:";
            this.IsStartVertexCheckbox.Checked += __IsStartVertexCheckbox_ValueChanged;
            this.IsStartVertexCheckbox.Unchecked += __IsStartVertexCheckbox_ValueChanged;
            this.Items.Add(this.IsStartVertexCheckbox);

            this.LoadVertices(Enumerable.Empty<IQGVertex>());
        }

        private void __VertexIDTypeCB_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_IsLoading)
                return;

            if (this.LoadedVertices.Count() != 1)
                throw new Exception("Setting vertex ID type can only be done to one vertex at a time.");

            IQGVertex loadedVertex = this.LoadedVertices.First();
            loadedVertex.VertexIDType = (QGVertexIDType)this.VertexIDTypeCB.SelectedIndex;
            if (loadedVertex.VertexIDType == QGVertexIDType.Local || loadedVertex.VertexIDType == QGVertexIDType.Global && !(loadedVertex.ID is string))
                loadedVertex.ID = "";

            __reloadControl();
        }

        private void __VertexIDField_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_IsLoading)
                return;

            if (this.LoadedVertices.Count() != 1)
                throw new Exception("Setting vertex ID can only be done to one vertex at a time.");

            IQGVertex loadedVertex = this.LoadedVertices.First();
            loadedVertex.ID = this.VertexIDField.FieldText;

            __reloadControl();
        }

        private void __IsStartVertexCheckbox_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (_IsLoading)
                return;

            if (this.LoadedVertices.Count() != 1)
                throw new Exception("Setting the start vertex status can only be done to one vertex.");

            IQGVertex loadedVertex = this.LoadedVertices.First();
            if (this.IsStartVertexCheckbox.IsChecked == true)
                loadedVertex.SetAsStart();
            else
                loadedVertex.ClearVertexStatus();

            __reloadControl();
        }

        private void __IsMarkedCheckbox_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (_IsLoading)
                return;

            if (this.IsMarkedCheckbox.IsChecked == true)
                foreach (IQGVertex v in this.LoadedVertices)
                    v.MarkVertex();
            else if (this.IsMarkedCheckbox.IsChecked == false)
                foreach (IQGVertex v in this.LoadedVertices)
                    v.ClearVertexStatus();

            __reloadControl();
        }
    }
}
