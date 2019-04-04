using ComputationalFluidDynamics.Factories;

namespace ComputationalFluidDynamics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var nodeSpace = NodeSpaceFactory.Create(0, 5, 0, 5, 20);

            bool stop = true;
        }
    }
}
