namespace ComputationalFluidDynamics
{
    public static class DataHelper
    {
        public static T[,] GetNew2DArray<T>(int x, int y, T initialValue)
        {
            var array = new T[x, y];

            for (var i = 0; i < x * y; i++)
            {
                array[i % x, i / x] = initialValue;
            }

            return array;
        }

        public static T[,,] GetNew3DArray<T>(int a, int x, int y, T initialValue)
        {
            var array = new T[a, x, y];

            for (var i = 0; i < x * y; i++)
            {
                for (var j = 0; j < a; j++)
                {
                    array[j, i % x, i / x] = initialValue;
                }
            }

            return array;
        }

        public static T[,,,] GetNew4DArray<T>(int a, int b, int c, int d, T initialValue)
        {
            var array = new T[a, b, c, d];

            for (var i = 0; i < a; i++)
            {
                for (var j = 0; j < b; j++)
                {
                    for (var k = 0; k < c; k++)
                    {
                        for (var l = 0; l < d; l++)
                        {
                            array[i, j, k, l] = initialValue;
                        }
                    }
                }
            }

            return array;
        }
    }
}
