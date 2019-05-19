using System.Collections.ObjectModel;
using System.Linq;

namespace ComputationalFluidDynamics.LatticeVectors
{
    public class LatticeVectorCollection : Collection<LatticeVector>
    {
        protected int[] VectorIndices;

        public LatticeVectorCollection(int[,] vectors, double scalar, double[] weightings)
        {
            InitialiseLatticeVectors(vectors, scalar, weightings);
        }

        public LatticeVector GetOpposite(LatticeVector latticeVector)
        {
            if (!latticeVector.HasValues)
            {
                return null;
            }

            return this.FirstOrDefault(x => x.HasValues &&
                                            x.Dx == -latticeVector.Dx &&
                                            x.Dy == -latticeVector.Dy &&
                                            x.Dz == -latticeVector.Dz);
        }

        public int GetOppositeIndex(LatticeVector latticeVector)
        {
            var vector = GetOpposite(latticeVector);

            return Items.IndexOf(vector);
        }

        private void InitialiseLatticeVectors(int[,] vectors, double scalar, double[] weightings)
        {
            VectorIndices = new int[vectors.GetLength(1)];

            switch (vectors.GetLength(0))
            {
                case 2:
                    for (var i = 0; i < vectors.GetLength(1); i++)
                    {
                        Add(new LatticeVectorXY(vectors[0, i], vectors[1, i], scalar, weightings[i]));
                        VectorIndices[i] = Items.Count;
                    }

                    break;

                case 3:
                    for (var i = 0; i < vectors.GetLength(1); i++)
                    {
                        Add(new LatticeVectorXYZ(vectors[0, i], vectors[1, i], vectors[2, i], scalar, weightings[i]));
                        VectorIndices[i] = Items.Count;
                    }

                    break;
            }
        }
    }
}
