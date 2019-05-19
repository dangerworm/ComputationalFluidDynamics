namespace ComputationalFluidDynamics
{
    public class ModelParameters
    {
        /// <summary>
        /// Time step.
        /// </summary>
        public double Dt => TimeStep;

        /// <summary>
        /// Reynolds number.
        /// </summary>
        public double Re => ReynoldsNumber;

        /// <summary>
        /// Initial density.
        /// </summary>
        public double Rho0 => InitialDensity;

        /// <summary>
        /// Initial velocity in x direction.
        /// </summary>
        public double U0 => InitialVelocity[0];

        /// <summary>
        /// Initial velocity in y direction.
        /// </summary>
        public double V0 => InitialVelocity[1];

        public double[] InitialVelocity { get; set; }
     
        public double InitialDensity { get; set; }
       
        public double ReynoldsNumber { get; set; }
        
        public double TimeStep { get; set; }

        public double ConvergenceTolerance { get; set; }

        public ModelParameters(double[] initialVelocity, double initialDensity, double reynoldsNumber, double timeStep, double convergenceTolerance)
        {
            InitialVelocity = initialVelocity;
            InitialDensity = initialDensity;
            ReynoldsNumber = reynoldsNumber;
            TimeStep = timeStep;
            ConvergenceTolerance = convergenceTolerance;
        }
    }
}
