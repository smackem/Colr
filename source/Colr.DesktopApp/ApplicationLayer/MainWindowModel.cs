using Colr.DesktopApp.Util;
using Colr.Imaging;
using FeatherSharp.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Colr.DesktopApp.ApplicationLayer
{
    [Feather(FeatherAction.NotifyPropertyChanged)]
    class MainWindowModel : NotifyPropertyChanged
    {
        public ImageSource ImageSource { get; private set; }
        public ColrBitmap Bitmap { get; private set; }
        public ColorHsv? DominantColor { get; private set; }
        public IEnumerable<int> HueDistribution { get; private set; }
        public IEnumerable<int> DominantHueSaturationDistribution { get; private set; }
        public IEnumerable<int> DominantHueValueDistribution { get; private set; }

        public void LoadImage(string filePath)
        {
            Bitmap = ColrBitmap.LoadFromFile(filePath);
            ImageSource = Bitmap.Bitmap.ToBitmapSource();
            DominantColor = null;
            HueDistribution = null;
            DominantHueSaturationDistribution = null;
            DominantHueValueDistribution = null;
        }

        public async void AnalyzeImage()
        {
            if (Bitmap != null)
            {
                var distribution = await Bitmap.GetColorDistributionAsync(120);
                var dominantColor = distribution.GetDominantColor();
                HueDistribution = distribution.GetHueDistribution();
                DominantHueSaturationDistribution = distribution.GetSaturationDistribution(dominantColor.H);
                DominantHueValueDistribution = distribution.GetValueDistribution(dominantColor.H);
                DominantColor = dominantColor;
            }
        }
    }
}
