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
using SD = System.Drawing;

namespace Colr.DesktopApp.ApplicationLayer
{
    [Feather(FeatherAction.NotifyPropertyChanged)]
    class MainWindowModel : INotifyPropertyChanged
    {
        public ImageSource ImageSource { get; private set; }
        public ColrBitmap Bitmap { get; private set; }
        public int[] HueDistribution { get; private set; }
        public ColorHsv? DominantColor { get; private set; }
        public int[] SaturationDistribution { get; private set; }
        public int[] ValueDistribution { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadImage(string filePath)
        {
            Bitmap = ColrBitmap.LoadFromFile(filePath);
            ImageSource = ToBitmapSource(Bitmap.Bitmap);

            HueDistribution = null;
            SaturationDistribution = null;
            ValueDistribution = null;
            DominantColor = null;
        }

        public async void AnalyzeImage()
        {
            if (Bitmap != null)
            {
                var dist = await Bitmap.GetHueDistributionAsync(120);

                HueDistribution = dist.HueDistribution;
                DominantColor = dist.DominantColor;
                SaturationDistribution = dist.DominantHueSaturationDistribution;
                ValueDistribution = dist.DominantHueValueDistribution;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        static BitmapSource ToBitmapSource(SD.Bitmap source)
        {
            var bitmapSource = null as BitmapSource;
            var hBitmap = source.GetHbitmap();

            try
            {
                bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitmapSource = null;
            }
            finally
            {
                Util.NativeMethods.DeleteObject(hBitmap);
            }

            return bitmapSource;
        }

        void OnPropertyChanged(string propertyName)
        {
            var @event = PropertyChanged;

            if (@event != null)
                @event(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
