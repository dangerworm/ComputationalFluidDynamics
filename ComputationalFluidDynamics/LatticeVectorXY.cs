using System;

namespace ComputationalFluidDynamics
{
    public class LatticeVectorXY : LatticeVector
    {
        public LatticeVectorXY(int dx, int dy, double scalar)
        : base(dx, dy, scalar)
        {
        }

        public override bool HasValues => Dx != 0 && Dy != 0;
    }
}
