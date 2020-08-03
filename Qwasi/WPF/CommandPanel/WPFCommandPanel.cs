using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using Qwasi.GraphUI;
using Qwasi.WPF.Controls;

namespace Qwasi.WPF
{
    public class WPFCommandPanel : WPFSidePanel
    {
        public IEnumerable<IQGVertex> LoadedVertices { get; private set; } = new IQGVertex[0];

        public WPFUIModePane UIModePane { get; } = new WPFUIModePane();

        public WPFGraphBuilderPane GraphBuilderPane { get; } = new WPFGraphBuilderPane();
        public WPFVertexPropertiesPane VertexPropertiesPane { get; } = new WPFVertexPropertiesPane();
        public WPFSubgraphPropertiesPane SubgraphPropertiesPane { get; } = new WPFSubgraphPropertiesPane();

        public WPFSimulationPane SimulationPane { get; } = new WPFSimulationPane();
        public WPFSimulationInfoPane SimulationInfoPane { get; } = new WPFSimulationInfoPane();

        public WPFAnalyticsPane AnalyticsPane { get; } = new WPFAnalyticsPane();
        public WPFAnalyticsExportPane AnalyticsExportPane { get; } = new WPFAnalyticsExportPane();

        protected WPFGraphControl WPFGraphControl { get; private set; }
        protected QwasiEngine QwasiEngine { get; private set; }

        public void SetQwasiEngine(QwasiEngine qwasiEngine)
        {
            this.QwasiEngine = qwasiEngine;

            this.UIModePane.SetQwasiEngine(this.QwasiEngine);
            qwasiEngine.ActiveUIModeChanged += (o, e) => __setUIMode(e.UIMode);
            __setUIMode(this.QwasiEngine.ActiveUIMode);

            this.SimulationPane.SetQwasiEngine(this.QwasiEngine);
            this.SimulationInfoPane.SetQwasiEngine(this.QwasiEngine);

            this.AnalyticsPane.SetQwasiEngine(this.QwasiEngine);
            this.AnalyticsExportPane.SetQwasiEngine(this.QwasiEngine);
        }

        private void __setUIMode(QwasiUIMode uiMode)
        {
            if (uiMode == QwasiUIMode.Edit)
                __enterUIEditMode();
            else if (uiMode == QwasiUIMode.Simulation)
                __enterUISimulationMode();
            else if (uiMode == QwasiUIMode.Analytics)
                __enterUIAnalyticsMode();
        }

        private void __enterUIEditMode()
        {
            this.GraphBuilderPane.Show();
            this.VertexPropertiesPane.Show();
            this.SubgraphPropertiesPane.Show();

            this.SimulationPane.Hide();
            this.SimulationInfoPane.Hide();

            this.AnalyticsPane.Hide();
            this.AnalyticsExportPane.Hide();
        }

        private void __enterUISimulationMode()
        {
            this.GraphBuilderPane.Hide();
            this.VertexPropertiesPane.Hide();
            this.SubgraphPropertiesPane.Hide();

            this.SimulationPane.Show();
            this.SimulationInfoPane.Show();

            this.AnalyticsPane.Hide();
            this.AnalyticsExportPane.Hide();
        }

        private void __enterUIAnalyticsMode()
        {
            this.GraphBuilderPane.Hide();
            this.VertexPropertiesPane.Hide();
            this.SubgraphPropertiesPane.Hide();

            this.SimulationPane.Hide();
            this.SimulationInfoPane.Hide();

            this.AnalyticsPane.Show();
            this.AnalyticsExportPane.Show();
        }

        public void SetWPFGraphControl(WPFGraphControl graphControl)
        {
            this.GraphBuilderPane.SetWPFGraphControl(graphControl);
            this.WPFGraphControl = graphControl;
        }

        public void LoadGraphControls()
        {
            IEnumerable<IQGControl> selectedElements = IQGSelectable.SelectedElements.OfType<IQGControl>();

            this.GraphBuilderPane.LoadGraphControls(selectedElements);
            this.VertexPropertiesPane.LoadVertices(selectedElements.OfType<IQGVertex>());
            this.SubgraphPropertiesPane.LoadGraphLayers(selectedElements.OfType<IQGGraphLayer>());
        }

        public WPFCommandPanel()
            : base()
        {
            this.Children.Add(this.UIModePane);

            this.Children.Add(this.GraphBuilderPane);
            this.Children.Add(this.VertexPropertiesPane);
            this.Children.Add(this.SubgraphPropertiesPane);

            this.Children.Add(this.SimulationPane);
            this.Children.Add(this.SimulationInfoPane);

            this.Children.Add(this.AnalyticsPane);
            this.Children.Add(this.AnalyticsExportPane);

            IQGSelectable.SelectedElementsChanged += __selectedElementsChanged;
        }

        private void __selectedElementsChanged(object sender, QGSelectionEventArgs e)
        {
            LoadGraphControls();
        }
    }
}
