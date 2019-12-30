using System.Collections.Generic;
using ComputationalFluidDynamics.Factories;

namespace ComputationalFluidDynamics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Define domain
            var nodeSpace = NodeSpaceFactory.Create(0.0, 1.0, 0.0, 1.0, 300);
            var modelParameters = new ModelParameters(new[] {0.1, 0}, 1.0, 3200.0, 0.01, 0.00001);
            List<Shape> shapes = null;

            var simulation = new LatticeBhatnagarGrossKrookSimulator(modelParameters, nodeSpace, shapes, 300, 5);

            while (simulation.CurrentIteration < simulation.MaxIterations)
                simulation.Iterate();

            var stop = true;
        }
    }
}