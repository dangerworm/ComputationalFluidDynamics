namespace ComputationalFluidDynamics.LatticeVectors
{
    public class LatticeVectorXY : LatticeVector
    {
        public LatticeVectorXY(int dx, int dy, double scalar, double weighting)
        : base(dx, dy, scalar, weighting)
        {
        }

        public override bool HasValues => Dx != 0 && Dy != 0;
    }
}
