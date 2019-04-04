namespace ComputationalFluidDynamics
{
    public class Simulator : ISimulator
    {
        public NodeSpace NodeSpace { get; set; }

        public int CurrentIteration { get; set; }
        public int MaxIterations { get; set; }

        public Simulator(NodeSpace nodeSpace, int iterations)
        {
            NodeSpace = nodeSpace;
            MaxIterations = iterations;
        }
    }
}
