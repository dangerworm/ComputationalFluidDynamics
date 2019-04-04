namespace ComputationalFluidDynamics
{
    public abstract class LatticeVector
    {
        public int Dx;
        public int Dy;
        public int Dz;

        public double Scalar;

        public abstract bool HasValues { get; }

        protected LatticeVector(int dx, int dy, double scalar)
        : this(dx, dy, 0, scalar)
        {
        }

        protected LatticeVector(int dx, int dy, int dz, double scalar)
        {
            Dx = dx;
            Dy = dy;
            Dz = dz;
            Scalar = scalar;
        }
    }
}