namespace ComputationalFluidDynamics.LatticeVectors
{
    public class LatticeVectorXY : LatticeVector
    {
        public LatticeVectorXY(int index, int dx, int dy, double scalar, double weighting)
            : base(index, dx, dy, scalar, weighting)
        {
        }

        public override bool HasValues => Dx != 0 && Dy != 0;
    }
}