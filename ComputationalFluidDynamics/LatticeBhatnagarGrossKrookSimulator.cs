using ComputationalFluidDynamics.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComputationalFluidDynamics.Nodes;

namespace ComputationalFluidDynamics
{
    public class LatticeBhatnagarGrossKrookSimulator : ISimulator
    {
        private const NodeType FlowNodes = NodeType.Boundary | NodeType.Liquid;

        public ModelParameters ModelParameters { get; }
        public NodeSpace NodeSpace { get; }
        public IEnumerable<Shape> Shapes { get; }

        public int CurrentIteration { get; set; }
        public int MaxIterations { get; }
        public int OutputDelay { get; }
        public double SimulatorTime => CurrentIteration * ModelParameters.TimeStep;

        public bool IsInitialised { get; private set; }

        private NodeSpaceXY _nodes => (NodeSpaceXY)NodeSpace;
        private double _l2Error;

        #region Computed Parameters

        /// <summary>
        /// The viscosity of the fluid. Usually denoted nu.
        /// </summary>
        private double _kinematicViscosity => _latticeVelocity * NodeSpace.Dx * (2 * _relaxationTime - 1.0 / 6);

        /// <summary>
        /// Speed at which effects travel through the lattice. Equal to dx/dt.
        /// </summary>
        private double _latticeVelocity => NodeSpace.Dx / ModelParameters.Dt;

        /// <summary>
        /// Governs speed of convergence. Usually denoted tau.
        /// </summary>
        private double _relaxationTime
        {
            get
            {
                var yLength = (double)NodeSpace.MaxY / NodeSpace.Resolution;
                var tau =
                (6 * ((ModelParameters.U0 * yLength) / (_latticeVelocity * NodeSpace.Dx * ModelParameters.Re)) +
                 1) / 2;

                if (tau <= 0.5)
                {
                    throw new ArgumentOutOfRangeException(
                        "The computed value for Tau is less than or equal to 0.5. Check model parameters.");
                }

                return tau;
            }
        }

        #endregion Computed Parameters

        #region Tracking Parameters

        private double _currentTime => CurrentIteration * ModelParameters.Dt;

        #endregion Tracking Parameters

<<<<<<< Updated upstream
=======
        #region Variables

        private double[,,] _fPrevious;
        private double[,,] _fEquilibrium;
        private double[,,] _fNew;

        #endregion Variables

>>>>>>> Stashed changes
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
            {
                throw new InvalidOperationException("The simulator has not yet been initialised.");
            }

            if (_l2Error > 1000)
            {
                throw new InvalidOperationException("Error greater than 1,000. Terminating program.");
            }

            NodeSpace.ComputeFEquilibrium(_latticeVelocity);

            Parallel.ForEach(NodeSpace, CollideStream);
            Parallel.ForEach(NodeSpace, BounceBackFromSolids);

            BounceBackEastWest();
            BounceBackNorthSouth();

            Parallel.ForEach(NodeSpace, node => node.CalculateComponents(NodeSpace.LatticeVectors));
            Parallel.ForEach(NodeSpace.NodeTypes(NodeType.Solid), n => n.Velocity = new[] { 0.0 }); 

            _l2Error = Math.Sqrt(NodeSpace.Sum(node => node.RhoError));
            Parallel.ForEach(NodeSpace, n => n.SetRhoPrevious());

            if (CurrentIteration % OutputDelay == 0)
            {
                OutputProgress();
            }

            CurrentIteration++;
        }

        private void CollideStream(Node node)
        {
            var x = node.X.Value;
            var y = node.Y.Value;

            if ((FlowNodes & node.NodeType) == 0)
            {
                node.ResetFNew();
                return;
            }

            for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
            {
                var dx = NodeSpace.LatticeVectors[a].Dx;
                var dy = NodeSpace.LatticeVectors[a].Dy;

<<<<<<< Updated upstream
                node.FNew[a, x + dx, y + dy] = GetNewCollisionValue(a, x, y, dx, dy, FlowNodes);
=======
                _fNew[a, x + dx, y + dy] = GetNewCollisionValue(a, x, y, dx, dy, FlowNodes);
>>>>>>> Stashed changes
            }
        }

        private double GetNewCollisionValue(int a, int x, int y, int dx, int dy, NodeType validNodeTypes)
        {
            if ((validNodeTypes & _nodes[x, y].NodeType) == 0 ||
                (validNodeTypes & _nodes[x + dx, y + dy].NodeType) == 0 ||
                (dx > 0 && x + dx > NodeSpace.MaxX) ||
                (dy > 0 && y + dy > NodeSpace.MaxY) ||
                (dx < 0 && x + dx < 0) ||
                (dy < 0 && y + dy < 0))
            {
                return 0.0;
            }

            return _fPrevious[a, x, y] - (_fPrevious[a, x, y] - _fEquilibrium[a, x, y]) / _relaxationTime;
        }

        private void BounceBackFromSolids(Node node)
        {
            var x = node.X ?? -1;
            var y = node.Y ?? -1;

            if (node.NodeType != NodeType.Liquid || x <= 0 || x >= NodeSpace.MaxX || y <= 0 || y >= NodeSpace.MaxY)
                return;

            var temp = new double[NodeSpace.LatticeVectors.Count];

            for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
            {
                temp[a] = _fNew[a, x, y];
            }

            for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
            {
                var vector = NodeSpace.LatticeVectors[a];
                var opposite = NodeSpace.LatticeVectors.GetOppositeIndex(vector);

                var dx = vector.Dx;
                var dy = vector.Dy;

                if (_nodes[x + dx, y + dy].NodeType == NodeType.Solid)
                {
                    _fNew[opposite, x, y] = temp[a];
                }
            }
        }

        private void BounceBackEastWest()
        {
            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                /* Bounce along East boundary */
                _fNew[2, NodeSpace.MaxX - 1, y] = _fNew[0, NodeSpace.MaxX - 1, y];
                _fNew[6, NodeSpace.MaxX - 1, y] = _fNew[4, NodeSpace.MaxX - 1, y];
                _fNew[5, NodeSpace.MaxX - 1, y] = _fNew[7, NodeSpace.MaxX - 1, y];

                /* Bounce along West boundary */
                _fNew[0, 1, y] = _fNew[2, 1, y];
                _fNew[4, 1, y] = _fNew[6, 1, y];
                _fNew[7, 1, y] = _fNew[5, 1, y];
            }
        }

        private void BounceBackNorthSouth()
        {
            /* Bounce along South boundary  */
            for (var x = 0; x < NodeSpace.MaxX; x++)
            {
                _fNew[1, x, 1] = _fNew[3, x, 1];
                _fNew[4, x, 1] = _fNew[6, x, 1];
                _fNew[5, x, 1] = _fNew[7, x, 1];
            }

            /* Bounce along North boundary with a known velocity */
            for (var x = 1; x < NodeSpace.MaxX - 1; x++)
            {
                var y = NodeSpace.MaxY - 1;

                var rhoN = _fNew[8, x, y] + _fNew[0, x, y] + _fNew[2, x, y]
                           + 2 * (_fNew[1, x, y] + _fNew[5, x, y] + _fNew[4, x, y]);

                _fNew[3, x, y] = _fNew[1, x, y];
                _fNew[7, x, y] = _fNew[5, x, y] + rhoN * ModelParameters.U0 / 6;
                _fNew[6, x, y] = _fNew[4, x, y] - rhoN * ModelParameters.U0 / 6;
            }
        }

        private void OutputProgress()
        {
            var padding = "";
            for (var s = 0; s < MaxIterations.ToString().Length - 1; s++)
            {
                padding += " ";
            }

            Console.WriteLine($"Iteration: {padding}{CurrentIteration} | Error: {_l2Error:F8}");
        }
    }
}
