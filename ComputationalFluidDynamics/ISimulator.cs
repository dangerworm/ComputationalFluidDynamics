namespace ComputationalFluidDynamics
{
    public interface ISimulator
    {
        NodeSpace NodeSpace { get; set; }

        int CurrentIteration { get; set; }
        int MaxIterations { get; set; }
    }
}