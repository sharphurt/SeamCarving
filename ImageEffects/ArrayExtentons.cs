namespace ImageEffects
{
    public static class ArrayExtentons
    {
        public static T[,] Subarray<T>(this T[,] arr, int x, int y, int width, int height)
        {
            var result = new T[width, height];
            for (int _x = 0; _x < width; _x++)
            for (int _y = 0; _y < height; _y++)
            {
                result[_x, _y] = arr[_x + x, _y + y];
            }

            return result;
        }
    }
}