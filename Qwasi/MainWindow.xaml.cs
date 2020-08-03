using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Qwasi.GraphUI;
using GeneralMath.Expressions;
using GeneralMath.Expressions.HilbertSpace;
using Qwasi.HilbertSpaceMath;
using Qwasi.WPF;
using Microsoft.Win32;
using System.Xml;

namespace Qwasi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.QwasiEngine = new QwasiEngine(this.GraphControl);
            this.CommandPanel.SetWPFGraphControl(this.GraphControl);
            this.CommandPanel.SetQwasiEngine(this.QwasiEngine);
        }

        protected QwasiEngine QwasiEngine { get; private set; }

        private void MenuItem_Open(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Qwasi Graph File|*.qwg";
            openFileDialog.Title = "Open Qwasi Graph File";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName == "")
                return;

            XmlReader xr = XmlReader.Create(openFileDialog.FileName);
            xr.ReadToFollowing("QwasiGraph");

            this.QwasiEngine.LoadFromXml(xr);

            xr.Close();
            xr.Dispose();
        }

        private void MenuItem_Save(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Qwasi Graph File|*.qwg";
            saveFileDialog.Title = "Save Qwasi Graph File";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName == "")
                return;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter xw = XmlWriter.Create(saveFileDialog.FileName, settings);
            xw.WriteStartDocument();

            this.QwasiEngine.WriteXml(xw);

            xw.Flush();
            xw.Close();
            xw.Dispose();
        }
    }
}
