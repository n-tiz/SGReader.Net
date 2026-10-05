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

        /// <summary>Shared animation stage width (hotspot-aligned), not the sprite pixel size.</summary>
        public int LayoutWidth { get; }

        /// <summary>Shared animation stage height (hotspot-aligned), not the sprite pixel size.</summary>
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

                // Always keep native sprite pixels. Alignment is done in the player via DrawX/Y.
                _bitmap = ToBitmapImage(source);
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
