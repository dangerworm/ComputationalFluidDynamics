using ComputationalFluidDynamics.Nodes;

namespace ComputationalFluidDynamics
{
    public interface ISimulator
    {
        int CurrentIteration { get; set; }
        int MaxIterations { get; }
        ModelParameters ModelParameters { get; }
        NodeSpace NodeSpace { get; }
        int OutputDelay { get; }
        double SimulatorTime { get; }
    }
}