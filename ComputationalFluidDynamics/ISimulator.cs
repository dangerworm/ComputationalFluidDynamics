using ComputationalFluidDynamics.Nodes;

namespace ComputationalFluidDynamics
{
    public interface ISimulator
    {
        ModelParameters ModelParameters { get; }
        NodeSpace NodeSpace { get; }

        int CurrentIteration { get; set; }
        int MaxIterations { get; }
        int OutputDelay { get; }
        double SimulatorTime { get; }
    }
}