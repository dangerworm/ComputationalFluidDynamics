using ComputationalFluidDynamics.Enums;

namespace ComputationalFluidDynamics.Factories
{
    public class LatticeVectorCollectionFactory
    {
        private static readonly int[,] d2q9 = {
            { 1,  0, -1,  0,  1, -1, -1,  1,  0},
            { 0,  1,  0, -1,  1,  1, -1, -1,  0}
        };

        private static readonly double[] d2q9Weights = {
            1.0 / 9.0, 1.0 / 9.0, 1.0 / 9.0, 1.0 / 9.0, 1.0 / 36.0, 1.0 / 36.0, 1.0 / 36.0, 1.0 / 36.0, 4.0 / 9.0
        };

        private static readonly int[,] d3q19 =
        {
            {  1,  0, -1,  0,  0,  0,  1, -1, -1,  1,  1, -1, -1,  1,  0,  0,  0,  0,  0},
            {  0,  1,  0, -1,  0,  0,  1,  1, -1, -1,  0,  0,  0,  0,  1, -1, -1,  1,  0},
            {  0,  0,  0,  0,  1, -1,  0,  0,  0,  0,  1,  1, -1, -1,  1,  1, -1, -1,  0}
        };

        private static readonly double[] d3q19Weights = {
             1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0,1.0
        };

        public static LatticeVectorCollection Create(LatticeArrangement arrangement, double scalar)
        {
            switch (arrangement)
            {
                case LatticeArrangement.D2Q9:
                    return new LatticeVectorCollection(d2q9, scalar, d2q9Weights);
                case LatticeArrangement.D3Q19:
                    return new LatticeVectorCollection(d3q19, scalar, d3q19Weights);
            }

            return null;
        }
    }
}
