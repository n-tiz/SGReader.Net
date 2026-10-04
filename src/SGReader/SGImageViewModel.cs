using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SGReader.Core;
using SGReader.Helpers;

namespace SGReader
{
    public class SGImageViewModel : ObservableObject
    {
        private readonly SGImage _image;
        private readonly int _canvasWidth;
        private readonly int _canvasHeight;
        private readonly int _drawX;
        private readonly int _drawY;
        private readonly bool _alignToCanvas;
        private BitmapImage _bitmap;
        private bool _decodeAttempted;

        public string Group => _image.Parent?.FileName;
        public string Description => _image.Description;
        public string FullDescription => _image.FullDescription;
        public int Id => _image.Id;
        public byte Type => _image.Type;
        public string TypeLabel => SGImageTypeHelper.FormatType(_image.Type);
        public short XOffset => _image.XOffset;
        public short YOffset => _image.YOffset;

        public SGImageViewModel(SGImage image)
        {
            _image = image;
        }

        public SGImageViewModel(SGImage image, int canvasWidth, int canvasHeight, int drawX, int drawY)
        {
            _image = image;
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
            _drawX = drawX;
            _drawY = drawY;
            _alignToCanvas = true;
        }

        public BitmapImage Bitmap
        {
            get
            {
                EnsureDecoded();
                return _bitmap;
            }
        }

        public bool HasBitmap
        {
            get
            {
                EnsureDecoded();
                return _bitmap != null;
            }
        }

        private void EnsureDecoded()
        {
            if (_decodeAttempted)
                return;

            _decodeAttempted = true;

            if (_image.Width <= 0 || _image.Height <= 0 || _image.Parent == null)
                return;

            try
            {
                using var source = _image.CreateImage();
                if (source == null)
                    return;

                if (_alignToCanvas && _canvasWidth > 0 && _canvasHeight > 0)
                {
                    using var canvas = new Bitmap(_canvasWidth, _canvasHeight, PixelFormat.Format32bppArgb);
                    using (var graphics = Graphics.FromImage(canvas))
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.DrawImageUnscaled(source, _drawX, _drawY);
                    }
                    _bitmap = ToBitmapImage(canvas);
                }
                else
                {
                    _bitmap = ToBitmapImage(source);
                }
            }
            catch
            {
                _bitmap = null;
            }
        }

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
