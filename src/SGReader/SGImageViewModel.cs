using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using SGReader.Core;

namespace SGReader
{
    public class SGImageViewModel : ViewModelBase
    {
        private readonly SGImage _image;

        public string Group => _image.Parent.FileName;
        public string Description => _image.Description;
        public string FullDescription => _image.FullDescription;

        public SGImageViewModel(SGImage image)
        {
            _image = image;
            var bitmap = image.CreateImage();
            if (bitmap != null)
                Bitmap = ToBitmapImage(bitmap);
        }

        public BitmapImage Bitmap { get; }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }


}