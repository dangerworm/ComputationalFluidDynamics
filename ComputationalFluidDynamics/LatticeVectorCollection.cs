using System.Collections.ObjectModel;
using System.Linq;

namespace ComputationalFluidDynamics
{
    public class LatticeVectorCollection : Collection<LatticeVector>
    {
        protected int[] VectorIndices;

        public LatticeVectorCollection(int[,] vectors, double scalar)
        {
            InitialiseLatticeVectors(vectors, scalar);
        }

        public new LatticeVector this[int i] => Items[VectorIndices[i]];

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

        private void InitialiseLatticeVectors(int[,] vectors, double scalar)
        {
            VectorIndices = new int[vectors.GetLength(1)];

            switch (vectors.GetLength(0))
            {
                case 2:
                    for (var i = 0; i < vectors.GetLength(1); i++)
                    {
                        Add(new LatticeVectorXY(vectors[0, i], vectors[1, i], scalar));
                        VectorIndices[i] = Items.Count;
                    }

                    break;

                case 3:
                    for (var i = 0; i < vectors.GetLength(1); i++)
                    {
                        Add(new LatticeVectorXYZ(vectors[0, i], vectors[1, i], vectors[2, i], scalar));
                        VectorIndices[i] = Items.Count;
                    }

                    break;
            }
        }
    }
}
