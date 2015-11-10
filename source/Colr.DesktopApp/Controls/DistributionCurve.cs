using Colr.Imaging;
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

namespace Colr.DesktopApp.Controls
{
    [TemplatePart(Name = CurvePanePartName, Type = typeof(DrawingPane))]
    public class DistributionCurve : Control
    {
        public const string CurvePanePartName = "PART_CurvePane";
        DrawingPane curvePane;

        public static readonly DependencyProperty DistributionDataProperty =
            DependencyProperty.Register("DistributionData", typeof(int[]), typeof(DistributionCurve),
                new PropertyMetadata(null,
                    (sender, e) =>
                    {
                        var control = (DistributionCurve)sender;

                        if (control.IsLoaded)
                            control.Render();
                    }));

        public DistributionCurve()
        {
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        public int[] DistributionData
        {
            get { return (int[])GetValue(DistributionDataProperty); }
            set { SetValue(DistributionDataProperty, value); }
        }

        /// <summary>
        /// Called when a new template has been applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.curvePane = Template.FindName(CurvePanePartName, this) as DrawingPane;

            if (IsLoaded)
                Render();
        }

        ///////////////////////////////////////////////////////////////////////

        static DistributionCurve()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DistributionCurve), new FrameworkPropertyMetadata(typeof(DistributionCurve)));
        }

        void Render()
        {
            RenderCurve();
        }

        void RenderCurve()
        {
            if (this.curvePane == null)
                return;

            using (var dc = this.curvePane.RenderOpen())
            {
                var data = DistributionData;

                if (data == null || data.Length < 2)
                    return;

                var width = this.curvePane.ActualWidth;
                var height = this.curvePane.ActualHeight;
                var maxValue = data.Max();

                var dx = width / data.Length;
                var dy = height / maxValue;

                var pen = new Pen(Foreground, 1.0);

                var x = 0.0;
                var point0 = new Point(x, height - data[0] * dy);

                for (var i = 1; i < data.Length; i++)
                {
                    x += dx;
                    var point1 = new Point(x, height - data[i] * dy);

                    dc.DrawLine(pen, point0, point1);

                    point0 = point1;
                }
            }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Render();
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Render();
        }
    }
}
