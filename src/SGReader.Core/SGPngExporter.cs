using System;
using System.Drawing.Imaging;
using System.IO;

namespace SGReader.Core
{
    public static class SGPngExporter
    {
        public static string ExportImage(SGImage image, string outputPath)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path is required.", nameof(outputPath));

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)) ?? ".");

            using var bitmap = image.CreateImage();
            if (bitmap == null)
                throw new InvalidOperationException($"Image {image.Id} produced no bitmap.");

            bitmap.Save(outputPath, ImageFormat.Png);
            return outputPath;
        }

        public static int ExportBitmap(SGBitmap bitmap, string outputDirectory)
        {
            if (bitmap == null)
                throw new ArgumentNullException(nameof(bitmap));

            string folder = Path.Combine(outputDirectory, Sanitize(bitmap.Name));
            Directory.CreateDirectory(folder);

            int exported = 0;
            foreach (var image in bitmap.Images)
            {
                if (image.Width <= 0 || image.Height <= 0 || image.Parent == null)
                    continue;

                try
                {
                    string path = Path.Combine(folder, $"{Sanitize(bitmap.Name)}_{image.Id:D5}.png");
                    ExportImage(image, path);
                    exported++;
                }
                catch
                {
                    // Skip undecodable frames and continue batch export.
                }
            }

            return exported;
        }

        public static int ExportFile(SGFile file, string outputDirectory)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            string root = Path.Combine(outputDirectory, Sanitize(file.Name));
            Directory.CreateDirectory(root);

            int exported = 0;
            foreach (var bitmap in file.Bitmaps)
            {
                exported += ExportBitmap(bitmap, root);
            }

            // Images without a bitmap parent are rare, but export any leftovers by id.
            foreach (var image in file.Images)
            {
                if (image.Parent != null || image.Width <= 0 || image.Height <= 0)
                    continue;

                try
                {
                    string path = Path.Combine(root, "_unassigned", $"image_{image.Id:D5}.png");
                    ExportImage(image, path);
                    exported++;
                }
                catch
                {
                    // Skip undecodable frames.
                }
            }

            return exported;
        }

        private static string Sanitize(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "unnamed";

            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return name.Trim();
        }
    }
}
