namespace ComputationalFluidDynamics.LatticeVectors
{
    public abstract class LatticeVector
    {
        public int Dx;
        public int Dy;
        public int Dz;
        public int Index;

        public double Scalar;
        public double Weighting;

        protected LatticeVector(int index, int dx, int dy, double scalar, double weighting)
            : this(index, dx, dy, 0, scalar, weighting)
        {
        }

        protected LatticeVector(int index, int dx, int dy, int dz, double scalar, double weighting)
        {
            Index = index;
            Dx = dx;
            Dy = dy;
            Dz = dz;
            Scalar = scalar;
            Weighting = weighting;
        }

        public abstract bool HasValues { get; }

        public double X => Dx * Scalar;
        public double Y => Dy * Scalar;
        public double Z => Dz * Scalar;
    }
}