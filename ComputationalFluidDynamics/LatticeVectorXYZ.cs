using System;

namespace ComputationalFluidDynamics
{
    public class LatticeVectorXYZ : LatticeVector
    {
        public LatticeVectorXYZ(int dx, int dy, int dz, double scalar)
        : base(dx, dy, dz, scalar)
        {
        }

        public override bool HasValues => Dx != 0 && Dy != 0 && Dz != 0;
    }
}
