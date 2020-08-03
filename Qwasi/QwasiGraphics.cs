using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Qwasi
{
    public static class QwasiGraphics
    {
        public static QwasiScaleValues GetSystemScaleRatios(Visual fromVisual)
        {
            Matrix matrix;
            var source = PresentationSource.FromVisual(fromVisual);
            if (source != null)
                matrix = source.CompositionTarget.TransformToDevice;
            else
            {
                using (var src = new HwndSource(new HwndSourceParameters()))
                {
                    matrix = src.CompositionTarget.TransformToDevice;
                }
            }

            return new QwasiScaleValues(matrix.M11, matrix.M22);
        }
    }

    public struct QwasiScaleValues
    {
        public double XScale { get; }
        public double YScale { get; }

        public QwasiScaleValues(double xScale, double yScale)
        {
            this.XScale = xScale;
            this.YScale = yScale;
        }
    }
}
