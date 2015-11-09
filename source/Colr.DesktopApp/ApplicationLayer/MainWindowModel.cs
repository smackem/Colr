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
        public SD.Bitmap Bitmap { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadImage(string filePath)
        {
            Bitmap = Bitmaps.LoadFromFile(filePath);
            ImageSource = ToBitmapSource(Bitmap);
        }

        ///////////////////////////////////////////////////////////////////////

        void OnPropertyChanged(string propertyName)
        {
            var propertyChanged = PropertyChanged;

            if (propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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
    }
}
