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

namespace Qwasi.WPF.Controls
{
    public abstract class WPFLabeledControl : WPFContentControlPair
    {
        static WPFLabeledControl()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFLabeledControl), new FrameworkPropertyMetadata(typeof(WPFLabeledControl)));
        }

        public WPFLabel LabelControl { get; } = new WPFLabel();

        protected sealed override FrameworkElement BaseControl1 => this.LabelControl;
        protected override ContentControl ContentControl => this.LabelControl;

        public WPFLabeledControl()
            : base()
        {
            this.PartitionMetric = GridUnitType.Auto;
            this.PartitionAlignment = PartitionAlignment.Right;

            this.BaseControl2.HorizontalAlignment = HorizontalAlignment.Right;
        }
    }
}
