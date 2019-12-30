namespace ComputationalFluidDynamics.Factories
{
    public static class ArrayFactory
    {
        public static T[] Create<T>(int a, T initialValue = default(T))
        {
            var array = new T[a];

            if (initialValue.Equals(default(T)))
                return array;

            for (var i = 0; i < a; ++i)
                array[a] = initialValue;

            return array;
        }

        public static T[,] Create<T>(int x, int y, T initialValue = default(T))
        {
            var array = new T[x, y];

            if (initialValue.Equals(default(T)))
                return array;

            for (var i = 0; i < x * y; ++i)
                array[i % x, i / x] = initialValue;

            return array;
        }

        public static T[,,] Create<T>(int a, int x, int y, T initialValue = default(T))
        {
            var array = new T[a, x, y];

            if (initialValue.Equals(default(T)))
                return array;

            for (var i = 0; i < x * y; ++i)
            for (var j = 0; j < a; ++j)
                array[j, i % x, i / x] = initialValue;

            return array;
        }

        public static T[,,,] Create<T>(int a, int b, int c, int d, T initialValue = default(T))
        {
            var array = new T[a, b, c, d];

            if (initialValue.Equals(default(T)))
                return array;

            for (var i = 0; i < a; ++i)
            for (var j = 0; j < b; ++j)
            for (var k = 0; k < c; ++k)
            for (var l = 0; l < d; ++l)
                array[i, j, k, l] = initialValue;

            return array;
        }
    }
}