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

        private static Func<Color, int, byte> indexToColorComponent = (c, i) =>
        {
            switch (i)
            {
                case 0:
                    return c.R;
                case 1:
                    return c.G;
                case 2:
                    return c.B;
                default:
                    return c.A;
            }
        };

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

        public static double[][] MakeEnergyMap(Pixel[,] pixels)
        {
            var width = pixels.GetLength(0);
            var height = pixels.GetLength(1);
            var result = new double[width][];

            var tasks = Enumerable.Range(0, width)
                .Select(x => Task.Factory.StartNew(() =>
                {
                    var res = new double[height];
                    for (int y = 0; y < height; y++)
                    {
                        res[y] = GetPixelEnergy(pixels, x, y);
                    }

                    return res;
                }).ContinueWith(task =>
                {
                    lock (result)
                    {
                        var r = task.Result;
                        result[x] = r;
                    }
                }));

            Task.WaitAll(tasks.ToArray());
            return result;
        }

        public static (int x, int y)[][] MakeVerticalIndexMap(double[][] energyMap)
        {
            var width = energyMap.Length;
            var height = energyMap[0].Length;

            var result = new (int x, int y)[width][];

            var tasks = Enumerable.Range(0, width)
                .Select(x => Task.Factory.StartNew(() =>
                {
                    var col = new (int x, int y)[height];
                    for (var y = 0; y < height - 1; y++)
                    {
                        col[y] = GetMinValueBottom(energyMap, x, y).position;
                    }

                    col[height - 1] = (int.MaxValue, int.MaxValue);

                    return col;
                }).ContinueWith(task =>
                {
                    lock (result) result[x] = task.Result;
                }));

            Task.WaitAll(tasks.ToArray());
            return result;
        }

        public static (int x, int y)[][] MakeHorizontalIndexMap(double[][] energyMap)
        {
            var width = energyMap.Length;
            var height = energyMap[0].Length;

            var result = new (int x, int y)[width][];

            var tasks = Enumerable.Range(0, height)
                .Select(y => Task.Factory.StartNew(() =>
                {
                    var col = new (int x, int y)[height];
                    for (var x = 0; x < width - 1; y++)
                    {
                        col[y] = GetMinValueBottom(energyMap, x, y).position;
                    }

                    col[height - 1] = (int.MaxValue, int.MaxValue);

                    return col;
                }).ContinueWith(task =>
                {
                    lock (result) result[y] = task.Result;
                }));

            Task.WaitAll(tasks.ToArray());
            return result;
        }
        
        public static double[] CalculateSeams(double[][] energyMap, (int x, int y)[][] indexMap)
        {
            var width = energyMap.Length;
            var height = energyMap[0].Length;

            var sumMap = energyMap.Copy();
            var offsetMap = indexMap.Copy();

            // Round down to nearest even & divide by 2
            var blocks = (height & ~1) / 2;

            for (var iteration = 1; iteration < height / 2; iteration *= 2)
            {
                var blockSize = height / blocks;

                var tasks = Enumerable.Range(0, blocks)
                    .Select(block => Task.Factory.StartNew(() =>
                    {
                        var y = block * blockSize;
                        for (var x = 0; x < width; x++)
                        {
                            var nextPixelPosition = offsetMap[x][y];

                            lock (offsetMap) offsetMap[x][y] = offsetMap[nextPixelPosition.x][nextPixelPosition.y];
                            lock (sumMap) sumMap[x][y] += sumMap[nextPixelPosition.x][nextPixelPosition.y];
                        }
                    }));

                Task.WaitAll(tasks.ToArray());
                blocks /= 2;
            }

            var result = new double[width];
            for (var i = 0; i < width; i++) result[i] = sumMap[i][0];

            return result;
        }

        public static List<(int x, int y)> GetBestSeam(double[] sums, (int x, int y)[][] indexMap)
        {
            var bestStartPosition = (x: 0, y: 0);
            var bestStartValue = double.MaxValue;
            for (var i = 0; i < sums.Length; i++)
            {
                if (!(sums[i] < bestStartValue)) continue;
                bestStartPosition = (i, 0);
                bestStartValue = sums[i];
            }

            var result = new List<(int, int)>();

            var nextPosition = bestStartPosition;
            while (nextPosition.y != int.MaxValue)
            {
                result.Add(nextPosition);
                nextPosition = indexMap[nextPosition.x][nextPosition.y];
            }

            return result.GetRange(0, result.Count - 1);
        }

        private static ((int x, int y) position, double value) GetMinValueBottom(double[][] sumMatrix, int x, int y)
        {
            var min = Double.MaxValue;

            var minPosition = (x: 0, y: 0);
            for (var xOffset = -1; xOffset <= 1; xOffset++)
            {
                if (x + xOffset < 0 || x + xOffset >= sumMatrix.Length)
                    continue;

                if (sumMatrix[x + xOffset][y + 1] < min)
                {
                    min = sumMatrix[x + xOffset][y + 1];
                    minPosition = (x + xOffset, y + 1);
                }
            }

            return (minPosition, min);
        }
        

        public static Pixel[,] MarkSeam(Pixel[,] image, List<(int x, int y)> seamPixels)
        {
            foreach (var pixel in seamPixels) image[pixel.x, pixel.y].Marked = true;
            return image;
        }

        public static Pixel[,] RemoveSeamsVertical(Pixel[,] image, List<(int x, int y)> marked)
        {
            var widht = image.GetLength(0);

            var height = image.GetLength(1);
            foreach (var (x, y) in marked)
            {
                image[x, y].Marked = false;

                var i = x;
                while (i < widht - 1)
                {
                    image[i, y].Color = image[i + 1, y].Color;
                    i++;
                }
            }

            image = image.Subarray(0, 0, --widht, height);
            return image;
        }
    }
}