using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using Microsoft.Win32;
using Qwasi.GraphUI;
using Qwasi.HilbertSpaceMath;
using Qwasi.WPF.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace Qwasi.WPF
{
    public delegate BitmapSource WPFBitmapGeneratorFunction();

    public class WPFPlotActionsPane : Grid
    {
        protected WPFBitmapGeneratorFunction BitmapGeneratorFunction { get; private set; }
        public void SetBitmapGeneratorFunction(WPFBitmapGeneratorFunction generatorFunction) => this.BitmapGeneratorFunction = generatorFunction;

        protected QwasiEngine QwasiEngine { get; private set; }
        public void SetQwasiEngine(QwasiEngine qwasiEngine) => this.QwasiEngine = qwasiEngine;

        private void __FormatButton(Button button, string labelText, Grid grid, int gridRow, int gridColumn)
        {
            button.Content = labelText;
            button.HorizontalAlignment = HorizontalAlignment.Stretch;
            button.HorizontalContentAlignment = HorizontalAlignment.Center;
            button.Margin = new Thickness(4);
            button.Padding = new Thickness(4, 2, 4, 2);
            Grid.SetRow(button, gridRow);
            Grid.SetColumn(button, gridColumn);
            grid.Children.Add(button);
        }

        protected Button SavePlotButton { get; } = new Button();
        protected Button DoneButton { get; } = new Button();

        private void __initializeButtonsPane()
        {
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
            this.HorizontalAlignment = HorizontalAlignment.Stretch;

            __FormatButton(this.SavePlotButton, "Save Plot Image", this, 0, 1);
            __FormatButton(this.DoneButton, "Done", this, 0, 2);

            this.SavePlotButton.Click += SavePlotButton_Click;
            this.DoneButton.Click += DoneButton_Click;
        }

        private void __savePngFile(string filePath, BitmapSource image)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(fs);
            }
        }

        private void SavePlotButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG File|*.png";
            saveFileDialog.Title = "Save Plot Image";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName == "")
                return;

            __savePngFile(saveFileDialog.FileName, this.BitmapGeneratorFunction());
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            this.QwasiEngine.ClosePlotFrame();
        }

        public WPFPlotActionsPane()
            : base()
        {
            __initializeButtonsPane();
        }

        public WPFPlotActionsPane(QwasiEngine qwasiEngine, WPFBitmapGeneratorFunction generatorFunction)
            : this()
        {
            this.SetQwasiEngine(qwasiEngine);
            this.SetBitmapGeneratorFunction(generatorFunction);
        }
    }
}
