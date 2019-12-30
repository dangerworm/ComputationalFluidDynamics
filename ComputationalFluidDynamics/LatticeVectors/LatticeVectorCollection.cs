using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ComputationalFluidDynamics.LatticeVectors
{
    public class LatticeVectorCollection : Collection<LatticeVector>
    {
        private readonly int _vectorCount;
        private Dictionary<int, LatticeVector> _opposites;

        public int Dimensionality;

        public LatticeVectorCollection(int[,] vectors, double scalar, double[] weightings)
        {
            _vectorCount = vectors.GetLength(1);

            Dimensionality = vectors.GetLength(0);
            InitialiseLatticeVectors(vectors, scalar, weightings);
        }

        private LatticeVector CalculateOpposite(LatticeVector latticeVector)
        {
            if (!latticeVector.HasValues)
                return null;

            return this.FirstOrDefault(x => x.HasValues &&
                                            x.Dx == -latticeVector.Dx &&
                                            x.Dy == -latticeVector.Dy &&
                                            x.Dz == -latticeVector.Dz);
        }

        public LatticeVector GetOpposite(LatticeVector latticeVector)
        {
            return _opposites[latticeVector.Index];
        }

        private void InitialiseLatticeVectors(int[,] vectors, double scalar, IReadOnlyList<double> weightings)
        {
            for (var i = 0; i < _vectorCount; ++i)
            {
                if (Dimensionality == 3)
                    Add(new LatticeVectorXYZ(i, vectors[0, i], vectors[1, i], vectors[2, i], scalar, weightings[i]));
                else
                    Add(new LatticeVectorXY(i, vectors[0, i], vectors[1, i], scalar, weightings[i]));
            }

            _opposites = new Dictionary<int, LatticeVector>();
            foreach (var latticeVector in this)
                _opposites.Add(latticeVector.Index, CalculateOpposite(latticeVector));
        }
    }
}