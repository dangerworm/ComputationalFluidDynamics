namespace ComputationalFluidDynamics
{
    public abstract class NodeSpace3D : NodeSpace
    {
        protected int[,,] NodeIndices;

        protected NodeSpace3D(LatticeVectorCollection latticeVectors)
            : base(latticeVectors)
        {
        }
    }
}
