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
    class MainWindowModel : INotifyPropertyChanged
    {
        public ImageSource ImageSource { get; private set; }
        public ColrBitmap Bitmap { get; private set; }
        public ColorDistribution ColorDistribution { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadImage(string filePath)
        {
            Bitmap = ColrBitmap.LoadFromFile(filePath);
            ImageSource = Bitmap.Bitmap.ToBitmapSource();
            ColorDistribution = null;
        }

        public async void AnalyzeImage()
        {
            if (Bitmap != null)
                ColorDistribution = await Bitmap.GetHueDistributionAsync(120);
        }

        ///////////////////////////////////////////////////////////////////////

        void OnPropertyChanged(string propertyName)
        {
            var @event = PropertyChanged;

            if (@event != null)
                @event(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
