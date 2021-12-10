﻿using System;

 namespace ImageEffects
{
    public static class SobelFilter
    {
        public static double[,] Filter(double[,] image, double[,] sx)
        {
            var width = image.GetLength(0);
            var height = image.GetLength(1);
            var sxSize = sx.GetLength(0);
            var sy = GetTransposedMatrix(sx);
            var result = new double[width, height];
            int offset = sx.GetLength(0) / 2;
            for (int x = offset; x < width - offset; x++)
            for (int y = offset; y < height - offset; y++)
            {
                var pixelsMatrix = GetMatrixFromPixels(image, x, y, sxSize);
                var gx = MultiplyMatrixByMatrix(pixelsMatrix, sx);
                var gy = MultiplyMatrixByMatrix(pixelsMatrix, sy);
                result[x, y] = Math.Sqrt(gx * gx + gy * gy);
            }

            return result;
        }

        private static double[,] GetMatrixFromPixels(double[,] pixels, int x, int y, int size)
        {
            var offset = size / 2;
            var matrix = new double[size, size];
            for (var i = -offset; i <= offset; i++)
            for (var j = -offset; j <= offset; j++)
                matrix[i + offset, j + offset] = pixels[x + i, y + j];
            return matrix;
        }

        private static double MultiplyMatrixByMatrix(double[,] matrix1, double[,] matrix2)
        {
            var result = 0.0;
            var sideLength = matrix1.GetLength(0);
            for (var i = 0; i < sideLength; i++)
            for (var j = 0; j < sideLength; j++)
                result += matrix1[i, j] * matrix2[i, j];
            return result;
        }

        private static double[,] GetTransposedMatrix(double[,] originalMatrix)
        {
            var width = originalMatrix.GetLength(0);
            var height = originalMatrix.GetLength(1);
            var result = new double[height, width];
            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
                result[j, i] = originalMatrix[i, j];
            return result;
        }
    }
}