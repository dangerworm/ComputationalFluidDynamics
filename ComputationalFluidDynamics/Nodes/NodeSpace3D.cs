using ComputationalFluidDynamics.LatticeVectors;

namespace ComputationalFluidDynamics.Nodes
{
    public abstract class NodeSpace3D : NodeSpace
    {
        protected int[,,] NodeIndices;

        protected NodeSpace3D(LatticeVectorCollection latticeVectors, int resolution = 1)
            : base(latticeVectors, resolution)
        {
        }
    }
}
