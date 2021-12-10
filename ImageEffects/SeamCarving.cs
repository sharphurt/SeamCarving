using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Priority_Queue;

namespace ImageEffects
{
    public static class SeamCarving
    {
        private static Dictionary<(int x, int y), Pixel> _visited = new Dictionary<(int x, int y), Pixel>();

        private static Dictionary<(int x, int y), Pixel> _processed = new Dictionary<(int x, int y), Pixel>();

        private static SimplePriorityQueue<Pixel> _queue;

        public static double GetPixelEnergy(Pixel[,] pixels, int x, int y)
        {
            var xm = x;
            var ym = y;
            if (xm == 0)
                xm++;

            if (xm == pixels.GetLength(0) - 1)
                xm--;

            var dx = Math.Pow(((pixels[xm + 1, y].R - pixels[xm - 1, y].R)), 2.0) +
                     Math.Pow(((pixels[xm + 1, y].G - pixels[xm - 1, y].G)), 2.0) +
                     Math.Pow(((pixels[xm + 1, y].B - pixels[xm - 1, y].B)), 2.0);

            if (y == 0)
                ym++;

            if (y == pixels.GetLength(1) - 1)
                ym--;

            var dy = Math.Pow(((pixels[x, ym + 1].R - pixels[x, ym - 1].R)), 2.0) +
                     Math.Pow(((pixels[x, ym + 1].G - pixels[x, ym - 1].G)), 2.0) +
                     Math.Pow(((pixels[x, ym + 1].B - pixels[x, ym - 1].B)), 2.0);

            return Math.Sqrt(dx + dy);
        }

        public static double[,] MakeIntensityMatrix(Pixel[,] pixels)
        {
            var width = pixels.GetLength(0);
            var height = pixels.GetLength(1);

            var result = new double[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result[x, y] = GetPixelEnergy(pixels, x, y);
                }
            }

            return result;
        }

        public static Pixel[,] GrayScaleEnergyRepresentation(double[,] intensityMatrix)
        {
            var width = intensityMatrix.GetLength(0);
            var height = intensityMatrix.GetLength(1);

            var result = new Pixel[width, height];

            var maxEn = intensityMatrix.Cast<double>().Max();

            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                var k = (int) (255.0 * intensityMatrix[x, y] / maxEn);
                result[x, y] = new Pixel(Color.FromArgb(k, k, k));
            }

            return result;
        }


        private static void CheckPixelForMarkSeam(Pixel parent, int x, int y, double[,] intensityMatrix)
        {
            if (!_processed.ContainsKey((x, y)))
            {
                if (_visited.ContainsKey((x, y)))
                {
                    var change = _visited[(x, y)];
                    if (parent.IntensitySum + change.Intensity < change.IntensitySum)
                    {
                        _queue.Remove(change);
                        change.IntensitySum = parent.IntensitySum + change.Intensity;
                        change.Parent = parent;
                        _queue.Enqueue(change, (float) change.IntensitySum);
                    }
                }
                else
                {
                    var newPixel = new Pixel(x, y, intensityMatrix[x, y], parent);
                    newPixel.IntensitySum = parent.IntensitySum + newPixel.Intensity;
                    _visited.Add((x, y), newPixel);
                    _queue.Enqueue(newPixel, (float) newPixel.IntensitySum);
                }
            }
        }

        public static Pixel[,] MarkSeamVertical(Pixel[,] image, double[,] intensityMat)
        {
            _queue = new SimplePriorityQueue<Pixel>();
            _visited.Clear();
            _processed.Clear();
            for (var x = 0; x < image.GetLength(0); x++)
            {
                var newPixel = new Pixel(x, 0, intensityMat[x, 0], null);
                newPixel.IntensitySum = newPixel.Intensity;
                _queue.Enqueue(newPixel, (int) newPixel.IntensitySum);
                _visited.Add((newPixel.X, newPixel.Y), newPixel);
            }

            var tmp = new Pixel(0, 0, 0.00001, null);

            while (true)
            {
                if (_queue.Count == 0)
                    break;

                tmp = _queue.Dequeue();

                _processed[(tmp.X, tmp.Y)] = tmp;
                _visited.Remove((tmp.X, tmp.Y));
                //  image.setRGB(tmp.x,tmp.y,Color.BLACK.rgb) //za test

                if (tmp.Y == image.GetLength(1) - 1)
                    break;

                if (tmp.X > 0)
                    CheckPixelForMarkSeam(tmp, tmp.X - 1, tmp.Y + 1, intensityMat);

                CheckPixelForMarkSeam(tmp, tmp.X, tmp.Y + 1, intensityMat);

                if (tmp.X < image.GetLength(0) - 1)
                    CheckPixelForMarkSeam(tmp, tmp.X + 1, tmp.Y + 1, intensityMat);
            }

            while (tmp != null)
            {
                image[tmp.X, tmp.Y].Marked = true;
                tmp = tmp.Parent;
            }

            return image;
        }

        public static Pixel[,] MarkSeamHorizontally(Pixel[,] image, double[,] intensityMat)
        {
            _queue = new SimplePriorityQueue<Pixel>();
            _visited.Clear();
            _processed.Clear();
            for (var y = 0; y < image.GetLength(1); y++)
            {
                var newPixel = new Pixel(0, y, intensityMat[0, y], null);
                newPixel.IntensitySum = newPixel.Intensity;
                _queue.Enqueue(newPixel, (int) newPixel.IntensitySum);
                _visited.Add((newPixel.X, newPixel.Y), newPixel);
            }

            var tmp = new Pixel(0, 0, 0.00001, null);

            while (true)
            {
                if (_queue.Count == 0)
                    break;

                tmp = _queue.Dequeue();

                _processed[(tmp.X, tmp.Y)] = tmp;
                _visited.Remove((tmp.X, tmp.Y));
                //  image.setRGB(tmp.x,tmp.y,Color.BLACK.rgb) //za test

                if (tmp.X == image.GetLength(0) - 1)
                    break;

                if (tmp.Y > 0)
                    CheckPixelForMarkSeam(tmp, tmp.X + 1, tmp.Y - 1, intensityMat);

                CheckPixelForMarkSeam(tmp, tmp.X + 1, tmp.Y, intensityMat);

                if (tmp.Y < image.GetLength(1) - 1)
                    CheckPixelForMarkSeam(tmp, tmp.X + 1, tmp.Y + 1, intensityMat);
            }

            while (tmp != null)
            {
                image[tmp.X, tmp.Y].Marked = true;
                tmp = tmp.Parent;
            }

            return image;
        }

        public static Pixel[,] RemoveSeamsVertical(Pixel[,] image)
        {
            var widht = image.GetLength(0);
            var height = image.GetLength(1);

            for (var j = 0; j < height; j++)
            {
                var i = 0;
                while (i < widht - 1 && !image[i, j].Marked)
                {
                    i++;
                }

                image[i, j].Marked = false;

                while (i < widht - 1)
                {
                    image[i, j].Color = image[i + 1, j].Color;
                    i++;
                }
            }

            image = image.Subarray(0, 0, --widht, height);

            return image;
        }

        public static Pixel[,] RemoveSeamsHorizontally(Pixel[,] image)
        {
            var widht = image.GetLength(0);
            var height = image.GetLength(1);

            for (var i = 0; i < widht; i++)
            {
                var j = 0;
                while (j < height - 1 && !image[i, j].Marked)
                {
                    j++;
                }

                image[i, j].Marked = false;

                while (j < height - 1)
                {
                    image[i, j].Color = image[i, j + 1].Color;
                    j++;
                }
            }

            image = image.Subarray(0, 0, widht, --height);

            return image;
        }
    }
}