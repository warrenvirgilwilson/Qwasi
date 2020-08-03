using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using Qwasi.GraphUI;
using Qwasi.HilbertSpaceMath;
using Qwasi.WPF.Controls;

namespace Qwasi.WPF
{
    public class WPFAnalyticsExportPane : WPFContentPane
    {
        public WPFLeftLabeledCheckbox IncludeStepNumbersCheckbox { get; } = new WPFLeftLabeledCheckbox();
        public WPFButton CSVExportButton { get; } = new WPFButton();

        protected QwasiEngine QwasiEngine { get; private set; }

        public void SetQwasiEngine(QwasiEngine qwasiEngine)
        {
            this.QwasiEngine = qwasiEngine;
        }

        public WPFAnalyticsExportPane()
            : base()
        {
            this.IncludeStepNumbersCheckbox.Content = "Include Step Numbers:";
            this.IncludeStepNumbersCheckbox.IsChecked = true;
            this.Items.Add(this.IncludeStepNumbersCheckbox);

            this.TitleText = "Export";

            this.CSVExportButton.Content = "Export to CSV";
            this.CSVExportButton.Click += CSVExportButton_Click;
            this.Items.Add(this.CSVExportButton);
        }

        private void CSVExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Comma-Seperated Values File|*.csv";
            saveFileDialog.Title = "Export to CSV File";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName == "")
                return;

            TextWriter tw = new StreamWriter(saveFileDialog.FileName);

            this.QwasiEngine.ExportAnalyticsToCSV(tw, this.IncludeStepNumbersCheckbox.IsChecked ?? true);

            tw.Flush();
            tw.Close();
            tw.Dispose();
        }
    }
}
