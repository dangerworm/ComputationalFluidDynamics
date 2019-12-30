namespace ComputationalFluidDynamics.LatticeVectors
{
    public class LatticeVectorXYZ : LatticeVector
    {
        public LatticeVectorXYZ(int index, int dx, int dy, int dz, double scalar, double weighting)
            : base(index, dx, dy, dz, scalar, weighting)
        {
        }

        public override bool HasValues => Dx != 0 && Dy != 0 && Dz != 0;
    }
}