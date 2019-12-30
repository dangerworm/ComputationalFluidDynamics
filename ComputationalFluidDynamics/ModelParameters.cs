namespace ComputationalFluidDynamics
{
    public class ModelParameters
    {
        public ModelParameters(double[] initialVelocity, double initialDensity, double reynoldsNumber, double timeStep,
            double convergenceTolerance)
        {
            InitialVelocity = initialVelocity;
            InitialDensity = initialDensity;
            ReynoldsNumber = reynoldsNumber;
            TimeStep = timeStep;
            ConvergenceTolerance = convergenceTolerance;
        }

        public double ConvergenceTolerance { get; set; }

        /// <summary>
        ///     Time step.
        /// </summary>
        public double Dt => TimeStep;

        public double InitialDensity { get; set; }

        public double[] InitialVelocity { get; set; }

        /// <summary>
        ///     Reynolds number.
        /// </summary>
        public double Re => ReynoldsNumber;

        public double ReynoldsNumber { get; set; }

        /// <summary>
        ///     Initial density.
        /// </summary>
        public double Rho0 => InitialDensity;

        public double TimeStep { get; set; }

        /// <summary>
        ///     Initial velocity in x direction.
        /// </summary>
        public double U0 => InitialVelocity[0];

        /// <summary>
        ///     Initial velocity in y direction.
        /// </summary>
        public double V0 => InitialVelocity[1];
    }
}