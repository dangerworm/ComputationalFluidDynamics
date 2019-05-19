namespace ComputationalFluidDynamics.LatticeVectors
{
    public abstract class LatticeVector
    {
        public int Dx;
        public int Dy;
        public int Dz;

        public double Scalar;
        public double Weighting;

        public abstract bool HasValues { get; }

        public double X => Dx * Scalar;
        public double Y => Dy * Scalar;
        public double Z => Dz * Scalar;

        protected LatticeVector(int dx, int dy, double scalar, double weighting)
        : this(dx, dy, 0, scalar, weighting)
        {
        }

        protected LatticeVector(int dx, int dy, int dz, double scalar, double weighting)
        {
            Dx = dx;
            Dy = dy;
            Dz = dz;
            Scalar = scalar;
            Weighting = weighting;
        }
    }
}