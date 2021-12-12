using System;
using System.Drawing;
using System.Linq;

namespace ImageEffects
{
    public static class Utils
    {
        public static bool IsValidCoordinates(int x, int y, int width, int height) =>
            x >= 0 && x < width && y >= 0 && y < height;


        private static Bitmap ConvertToBitmap(int width, int height, Func<int, int, Color> getPixelColor)
        {
            var bmp = new Bitmap(width, height);

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                bmp.SetPixel(x, y, getPixelColor(x, y));

            return bmp;
        }

        public static Bitmap ConvertToBitmap(Pixel[,] array)
        {
            return ConvertToBitmap(array.GetLength(0), array.GetLength(1),
                (x, y) => array[x, y].Marked ? Color.White : array[x, y].Color);
        }

        public static Bitmap ConvertToBitmap(double[,] array)
        {
            var min = array.Cast<double>().Min();
            var max = array.Cast<double>().Max();

            return ConvertToBitmap(array.GetLength(0), array.GetLength(1), (x, y) =>
            {
                var gray = (int) array[x, y];
                gray = Math.Min(gray, 255);
                gray = Math.Max(gray, 0);
                return Color.FromArgb(gray, gray, gray);
            });
        }

        public static Pixel[,] LoadPixels(Bitmap bmp)
        {
            var pixels = new Pixel[bmp.Width, bmp.Height];
            for (var x = 0; x < bmp.Width; x++)
            for (var y = 0; y < bmp.Height; y++)
                pixels[x, y] = new Pixel(bmp.GetPixel(x, y));
            return pixels;
        }

        public static Pixel[,] ConvertToPixels(double[,] pixels)
        {
            var width = pixels.GetLength(0);
            var height = pixels.GetLength(1);

            var result = new Pixel[width, height];

            for (var x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var gray = (int) (255 * pixels[x, y]);
                gray = Math.Min(gray, 255);
                gray = Math.Max(gray, 0);
                result[x, y] = new Pixel(Color.FromArgb(gray, gray, gray));
            }

            return result;
        }

        public static double ScaleBetween(double v, double min, double max, double newMin, double newMax)
        {
            return (((v - min) * (newMax - newMin)) / (max - min)) + newMin;
        }
    }
}