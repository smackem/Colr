using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Colr.DesktopApp.Controls
{
    public class DrawingPane : FrameworkElement
    {
        readonly DrawingVisual drawing;
        readonly VisualCollection children;

        public DrawingPane()
        {
            this.children = new VisualCollection(this);
            this.drawing = new DrawingVisual();

            this.children.Add(this.drawing);
        }

        public DrawingContext RenderOpen()
        {
            return this.drawing.RenderOpen();
        }

        protected override int VisualChildrenCount
        {
            get { return this.children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.children[index];
        }
    }
}
