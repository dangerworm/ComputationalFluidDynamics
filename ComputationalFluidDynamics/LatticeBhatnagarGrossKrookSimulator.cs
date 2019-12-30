using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComputationalFluidDynamics.Enums;
using ComputationalFluidDynamics.Nodes;

namespace ComputationalFluidDynamics
{
    public class LatticeBhatnagarGrossKrookSimulator : ISimulator
    {
        private const NodeType FlowNodes = NodeType.Boundary | NodeType.Liquid;
        private double _l2Error;

        public LatticeBhatnagarGrossKrookSimulator(ModelParameters modelParameters, NodeSpace nodeSpace,
            IEnumerable<Shape> shapes, int iterations, int outputDelay)
        {
            ModelParameters = modelParameters;
            NodeSpace = nodeSpace;
            MaxIterations = iterations;
            OutputDelay = outputDelay;
            Shapes = shapes ?? Enumerable.Empty<Shape>();

            _l2Error = 1.0;

            Initialise();
        }

        #region Tracking Parameters

        private double _currentTime => CurrentIteration * ModelParameters.Dt;

        #endregion Tracking Parameters

        private NodeSpaceXY _nodes => (NodeSpaceXY)NodeSpace;

        public bool IsInitialised { get; private set; }
        public IEnumerable<Shape> Shapes { get; }

        public ModelParameters ModelParameters { get; }
        public NodeSpace NodeSpace { get; }

        public int CurrentIteration { get; set; }
        public int MaxIterations { get; }
        public int OutputDelay { get; }
        public double SimulatorTime => CurrentIteration * ModelParameters.TimeStep;

        private void BounceBackEastWest()
        {
            for (var y = 0; y < NodeSpace.MaxY; ++y)
            {
                /* Bounce along East boundary */
                _nodes[NodeSpace.MaxX - 1, y].FNew[2] = _nodes[NodeSpace.MaxX - 1, y].FNew[0];
                _nodes[NodeSpace.MaxX - 1, y].FNew[6] = _nodes[NodeSpace.MaxX - 1, y].FNew[4];
                _nodes[NodeSpace.MaxX - 1, y].FNew[5] = _nodes[NodeSpace.MaxX - 1, y].FNew[7];

                /* Bounce along West boundary */
                _nodes[1, y].FNew[0] = _nodes[1, y].FNew[2];
                _nodes[1, y].FNew[4] = _nodes[1, y].FNew[6];
                _nodes[1, y].FNew[7] = _nodes[1, y].FNew[5];
            }
        }

        private void BounceBackFromSolids(Node node)
        {
            var x = node.X;
            var y = node.Y;

            if (node.NodeType != NodeType.Liquid || x <= 0 || x >= NodeSpace.MaxX || y <= 0 || y >= NodeSpace.MaxY)
                return;

            var temp = new double[NodeSpace.LatticeVectors.Count];

            for (var a = 0; a < NodeSpace.LatticeVectors.Count; ++a)
                temp[a] = node.FNew[a];

            for (var a = 0; a < node.Neighbours.Length; ++a)
            {
                var vector = NodeSpace.LatticeVectors[a];
                var opposite = NodeSpace.LatticeVectors.GetOpposite(vector).Index;

                if (node.Neighbours[a].NodeType == NodeType.Solid)
                    node.FNew[opposite] = temp[a];
            }
        }

        private void BounceBackNorthSouth()
        {
            /* Bounce along South boundary  */
            for (var x = 0; x < NodeSpace.MaxX; ++x)
            {
                _nodes[x, 1].FNew[1] = _nodes[x, 1].FNew[3];
                _nodes[x, 1].FNew[4] = _nodes[x, 1].FNew[6];
                _nodes[x, 1].FNew[5] = _nodes[x, 1].FNew[7];
            }

            /* Bounce along North boundary with a known velocity */
            var y = NodeSpace.MaxY - 1;
            for (var x = 1; x < NodeSpace.MaxX - 1; ++x)
            {
                var fNew = _nodes[x, y].FNew;

                var rhoN = fNew[8] + fNew[0] + fNew[2]
                           + 2 * (fNew[1] + fNew[5] + fNew[4]);

                fNew[3] = fNew[1];
                fNew[7] = fNew[5] + rhoN * ModelParameters.U0 / 6;
                fNew[6] = fNew[4] - rhoN * ModelParameters.U0 / 6;
            }
        }

        private void CollideStream(Node node)
        {
            if ((FlowNodes & node.NodeType) == NodeType.None)
            {
                node.ResetFNew();
                return;
            }

            for (var a = 0; a < node.Neighbours.Length; ++a)
            {
                node.Neighbours[a].FNew[a] = (FlowNodes & node.Neighbours[a].NodeType) == NodeType.None
                    ? 0.0D
                    : node.FPrevious[a] - (node.FPrevious[a] - node.FEquilibrium[a]) / _relaxationTime;
            }
        }

        private void Initialise()
        {
            Parallel.ForEach(NodeSpace.NodeTypes(FlowNodes), n =>
                n.Initialise(NodeSpace.LatticeVectors.Count, ModelParameters.Rho0, ModelParameters.InitialVelocity));

            CurrentIteration = 0;
            IsInitialised = true;
        }

        public void Iterate()
        {
            if (!IsInitialised)
                throw new InvalidOperationException("The simulator has not yet been initialised.");

            if (_l2Error > 1000)
                throw new InvalidOperationException("Error greater than 1,000. Terminating program.");

            NodeSpace.ComputeFEquilibrium(_latticeVelocity);

            Parallel.ForEach(NodeSpace, CollideStream);
            Parallel.ForEach(NodeSpace, BounceBackFromSolids);

            BounceBackEastWest();
            BounceBackNorthSouth();

            Parallel.ForEach(NodeSpace, node => node.CalculateComponents(NodeSpace.LatticeVectors));
            Parallel.ForEach(NodeSpace.NodeTypes(NodeType.Solid), n => n.Velocity = new[] { 0.0, 0.0 });

            _l2Error = Math.Sqrt(NodeSpace.Sum(node => node.RhoError));
            Parallel.ForEach(NodeSpace, n => n.SetRhoPrevious());

            if (CurrentIteration % OutputDelay == 0)
                OutputProgress();

            ++CurrentIteration;
        }

        private void OutputProgress()
        {
            var padding = "";
            var max = MaxIterations.ToString().Length - 1;
            for (var s = 0; s < max; ++s)
                padding += " ";

            Console.WriteLine($"Iteration: {padding}{CurrentIteration} | Error: {_l2Error:F8}");
        }

        #region Computed Parameters

        /// <summary>
        ///     The viscosity of the fluid. Usually denoted nu.
        /// </summary>
        private double _kinematicViscosity => _latticeVelocity * NodeSpace.Dx * (2 * _relaxationTime - 1.0 / 6);

        /// <summary>
        ///     Speed at which effects travel through the lattice. Equal to dx/dt.
        /// </summary>
        private double _latticeVelocity => NodeSpace.Dx / ModelParameters.Dt;

        /// <summary>
        ///     Governs speed of convergence. Usually denoted tau.
        /// </summary>
        private double _relaxationTime
        {
            get
            {
                var yLength = (double)NodeSpace.MaxY / NodeSpace.Resolution;
                var tau =
                (6 * (ModelParameters.U0 * yLength / (_latticeVelocity * NodeSpace.Dx * ModelParameters.Re)) +
                 1) / 2;

                if (tau <= 0.5)
                    throw new ArgumentOutOfRangeException(
                        "The computed value for Tau is less than or equal to 0.5. Check model parameters.");

                return tau;
            }
        }

        #endregion Computed Parameters
    }
}