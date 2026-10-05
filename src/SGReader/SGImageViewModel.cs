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

        public int LayoutWidth { get; }
        public int LayoutHeight { get; }
        public int DrawX { get; }
        public int DrawY { get; }

        public SGImageViewModel(SGImage image)
            : this(image, image.Width, image.Height, 0, 0)
        {
        }

        public SGImageViewModel(SGImage image, int layoutWidth, int layoutHeight, int drawX, int drawY)
        {
            _image = image;
            LayoutWidth = Math.Max(1, layoutWidth);
            LayoutHeight = Math.Max(1, layoutHeight);
            DrawX = drawX;
            DrawY = drawY;
        }

        private SGImageViewModel(SGImage image, BitmapImage prebuilt, int width, int height)
        {
            _image = image;
            _bitmap = prebuilt;
            _decodeAttempted = true;
            LayoutWidth = Math.Max(1, width);
            LayoutHeight = Math.Max(1, height);
            DrawX = 0;
            DrawY = 0;
        }

        public static SGImageViewModel FromRendered(SGImage frame, Bitmap rendered)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (rendered == null)
                throw new ArgumentNullException(nameof(rendered));

            return new SGImageViewModel(frame, ToBitmapImage(rendered), rendered.Width, rendered.Height);
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

                _bitmap = ToBitmapImage(source);
            }
            catch
            {
                _bitmap = null;
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
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
