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
            DependencyProperty.Register("DistributionData", typeof(IReadOnlyList<int>), typeof(DistributionCurve),
                new PropertyMetadata(null, TriggerRender));

        public static readonly DependencyProperty CurveFillProperty =
            DependencyProperty.Register("CurveFill", typeof(Brush), typeof(DistributionCurve),
                new PropertyMetadata(null, TriggerRender));

        public DistributionCurve()
        {
            Loaded += OnLoaded;
            SizeChanged += OnSizeChanged;
        }

        public IReadOnlyList<int> DistributionData
        {
            get { return (IReadOnlyList<int>)GetValue(DistributionDataProperty); }
            set { SetValue(DistributionDataProperty, value); }
        }

        public Brush CurveFill
        {
            get { return (Brush)GetValue(CurveFillProperty); }
            set { SetValue(CurveFillProperty, value); }
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

                if (data == null || data.Count < 2)
                    return;

                var width = this.curvePane.ActualWidth;
                var height = this.curvePane.ActualHeight;
                var maxValue = data.Max();

                var dx = width / data.Count;
                var dy = height / maxValue;

                var geometry = new StreamGeometry();

                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(new Point(0, height), true, true);

                    var x = 0.0;

                    foreach (var hueCount in data)
                    {
                        ctx.LineTo(new Point(x, height - hueCount * dy), true, true);
                        x += dx;
                    }

                    ctx.LineTo(new Point(width, height), true, true);
                }

                var pen = new Pen(Foreground, 1.0);
                dc.DrawGeometry(CurveFill, pen, geometry);
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

        static void TriggerRender(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (DistributionCurve)sender;

            if (control.IsLoaded)
                control.Render();
        }
    }
}
