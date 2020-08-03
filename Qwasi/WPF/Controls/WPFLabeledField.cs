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
    public class WPFLabeledField<TTextbox, TValue> : WPFControlFieldPair<TTextbox, TValue>
        where TTextbox : WPFTextbox<TValue>, new()
    {
        static WPFLabeledField()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WPFLabeledField<TTextbox, TValue>), new FrameworkPropertyMetadata(typeof(WPFLabeledField<TTextbox, TValue>)));
        }

        public WPFLabel LabelControl { get; } = new WPFLabel();

        protected override FrameworkElement BaseControl1 => this.LabelControl;
        protected override ContentControl ContentControl => this.LabelControl;

        public WPFLabeledField()
            : base()
        {
            this.Margin = new Thickness(0, 1, 0, 0);
            this.PartitionMetric = GridUnitType.Star;
            this.PartitionAlignment = PartitionAlignment.Left;
            this.PartitionLocation = 50;
        }
    }

    public class WPFLabeledField : WPFLabeledField<WPFTextbox1, string> { }
    public class WPFLabeledIntField : WPFLabeledField<WPFIntValuedTextbox, int> { }
}
