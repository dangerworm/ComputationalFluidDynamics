using System;

namespace ComputationalFluidDynamics.Enums
{
    [Flags]
    public enum NodeType
    {
        None = 0,
        Vacuum = 1 << 1,
        Gas = 1 << 2,
        Liquid = 1 << 3,
        Solid = 1 << 4,
        Boundary = 1 << 5
    }
}