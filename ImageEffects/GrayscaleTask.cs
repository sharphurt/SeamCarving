﻿ namespace ImageEffects
{
    public static class GrayscaleTask
    {
        public static double[,] ToGrayscale(Pixel[,] original)
        {
            var grayscaleImage = new double[original.GetLength(0), original.GetLength(1)];
            var xLength = original.GetLength(0);
            var yLength = original.GetLength(1);
            for (var x = 0; x < xLength; x++)
            {
                for (var y = 0; y < yLength; y++)
                {
                    var currentPixel = original[x, y];
				
                    var rColor = currentPixel.R;
                    var gColor = currentPixel.G;
                    var bColor = currentPixel.B;
                    grayscaleImage[x, y] = (0.299 * rColor + 0.587 * gColor + 0.114 * bColor) / 255;
                }				
            }
            return grayscaleImage;
        }
    }
}