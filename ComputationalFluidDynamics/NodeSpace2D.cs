namespace ComputationalFluidDynamics
{
    public abstract class NodeSpace2D : NodeSpace
    {
        protected int[,] NodeIndices;

        protected NodeSpace2D(LatticeVectorCollection latticeVectors)
            : base(latticeVectors)
        {
        }
    }
}
