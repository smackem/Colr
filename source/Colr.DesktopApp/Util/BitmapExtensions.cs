using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using SD = System.Drawing;

namespace Colr.DesktopApp.Util
{
    static class BitmapExtensions
    {
        public static BitmapSource ToBitmapSource(this SD.Bitmap source)
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
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitmapSource;
        }
    }
}
