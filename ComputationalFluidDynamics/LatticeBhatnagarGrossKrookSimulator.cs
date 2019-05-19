using ComputationalFluidDynamics.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ComputationalFluidDynamics.Factories;
using ComputationalFluidDynamics.Nodes;

namespace ComputationalFluidDynamics
{
    public class LatticeBhatnagarGrossKrookSimulator : ISimulator
    {
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

        #region Variables

        private double[,,] _fPrevious;
        private double[,,] _fEquilibrium;
        private double[,,] _fCurrent;
        private double[,] _rho;
        private double[,] _rhoOld;
        private double[,] _u;
        private double[,] _v;

        #endregion Variables

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
            // Calculated at the start of each iteration. The equilibrium function.
            _fEquilibrium = ArrayFactory.Create(9, NodeSpace.MaxX, NodeSpace.MaxY, 0.0);

            // Built up each iteration. The 'new' values.
            _fCurrent = ArrayFactory.Create(9, NodeSpace.MaxX, NodeSpace.MaxY, 0.0);

            // Copied from _fCurrent at the end of each iteration. The 'previous' values.
            _fPrevious = ArrayFactory.Create(9, NodeSpace.MaxX, NodeSpace.MaxY, 0.0);

            _rho = ArrayFactory.Create(NodeSpace.MaxX, NodeSpace.MaxY, ModelParameters.Rho0);
            _rhoOld = ArrayFactory.Create(NodeSpace.MaxX, NodeSpace.MaxY, ModelParameters.Rho0);
            _u = ArrayFactory.Create(NodeSpace.MaxX, NodeSpace.MaxY, 0.0);
            _v = ArrayFactory.Create(NodeSpace.MaxX, NodeSpace.MaxY, 0.0);

            var validNodeTypes = new[] { NodeType.Boundary, NodeType.Liquid };

            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    if (!validNodeTypes.Contains(_nodes[x, y].NodeType))
                    {
                        continue;
                    }

                    _u[x, y] = ModelParameters.U0;
                    _v[x, y] = ModelParameters.V0;
                }
            }

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

            ComputeFEquilibrium(); // Can be done independent of nodes

            Parallel.ForEach(_nodes, CollideStream);
            Parallel.ForEach(_nodes, BounceBackFromSolids);

            BounceBackEastWest();
            BounceBackNorthSouth();

            CalculateComponents();
            ClearSolidCells();

            var error = 0.0;
            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    error += Math.Pow(_rho[x, y] - _rhoOld[x, y], 2);
                }
            }

            _l2Error = Math.Sqrt(error);

            Array.Copy(_rho, _rhoOld, _rho.Length);

            if (CurrentIteration % OutputDelay == 0)
            {
                OutputProgress();
            }

            CurrentIteration++;
        }

        private void ComputeFEquilibrium()
        {
            var eSquared = Math.Pow(_latticeVelocity, 2);

            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    var preCalc1 = (_u[x, y] * _u[x, y] + _v[x, y] * _v[x, y]) / eSquared;

                    for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                    {
                        var preCalc2 = (_u[x, y] * NodeSpace.LatticeVectors[a].X +
                                        _v[x, y] * NodeSpace.LatticeVectors[a].Y) / eSquared;

                        _fEquilibrium[a, x, y] = _rho[x, y] * NodeSpace.LatticeVectors[a].Weighting *
                                                 (1.0 + 3.0 * preCalc2 + 4.5 * Math.Pow(preCalc2, 2) - 1.5 * preCalc1);
                    }
                }
            }
        }

        private void CollideStream(Node node)
        {
            var validNodeTypes = new[] { NodeType.Boundary, NodeType.Liquid };

            var x = node.X ?? -1;
            var y = node.Y ?? -1;
            var a = 0;

            if (!validNodeTypes.Contains(_nodes[x, y].NodeType))
            {
                for (a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                {
                    _fCurrent[a, x, y] = 0;
                }

                return;
            }

            for (a = 0; a < NodeSpace.LatticeVectors.Count - 1; a++)
            {
                var dx = NodeSpace.LatticeVectors[a].Dx;
                var dy = NodeSpace.LatticeVectors[a].Dy;

                var newValue = GetNewCollisionValue(a, x, y, dx, dy, validNodeTypes);
                if (newValue != null)
                {
                    _fCurrent[a, x + dx, y + dy] = newValue.Value;
                }
            }

            a = NodeSpace.LatticeVectors.Count - 1;
            _fCurrent[a, x, y] = _fPrevious[a, x, y] - (_fPrevious[a, x, y] - _fEquilibrium[a, x, y]) / _relaxationTime;
        }

        private double? GetNewCollisionValue(int a, int x, int y, int dx, int dy, IEnumerable<NodeType> validNodeTypes)
        {
            if (!validNodeTypes.Contains(_nodes[x + dx, y + dy].NodeType) ||
                (dx > 0 && x + dx > NodeSpace.MaxX) ||
                (dy > 0 && y + dy > NodeSpace.MaxY) ||
                (dx < 0 && x + dx < 0) ||
                (dy < 0 && y + dy < 0))
            {
                return null;
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
                temp[a] = _fCurrent[a, x, y];
            }

            for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
            {
                var vector = NodeSpace.LatticeVectors[a];
                var opposite = NodeSpace.LatticeVectors.GetOppositeIndex(vector);

                var dx = vector.Dx;
                var dy = vector.Dy;

                if (_nodes[x + dx, y + dy].NodeType == NodeType.Solid)
                {
                    _fCurrent[opposite, x, y] = temp[a];
                }
            }
        }

        private void BounceBackEastWest()
        {
            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                /* Bounce along East boundary */
                _fCurrent[2, NodeSpace.MaxX - 1, y] = _fCurrent[0, NodeSpace.MaxX - 1, y];
                _fCurrent[6, NodeSpace.MaxX - 1, y] = _fCurrent[4, NodeSpace.MaxX - 1, y];
                _fCurrent[5, NodeSpace.MaxX - 1, y] = _fCurrent[7, NodeSpace.MaxX - 1, y];

                /* Bounce along West boundary */
                _fCurrent[0, 1, y] = _fCurrent[2, 1, y];
                _fCurrent[4, 1, y] = _fCurrent[6, 1, y];
                _fCurrent[7, 1, y] = _fCurrent[5, 1, y];
            }
        }

        private void BounceBackNorthSouth()
        {
            /* Bounce along South boundary  */
            for (var x = 0; x < NodeSpace.MaxX; x++)
            {
                _fCurrent[1, x, 1] = _fCurrent[3, x, 1];
                _fCurrent[4, x, 1] = _fCurrent[6, x, 1];
                _fCurrent[5, x, 1] = _fCurrent[7, x, 1];
            }

            /* Bounce along North boundary with a known velocity */
            for (var x = 1; x < NodeSpace.MaxX - 1; x++)
            {
                var y = NodeSpace.MaxY - 1;

                var rhoN = _fCurrent[8, x, y] + _fCurrent[0, x, y] + _fCurrent[2, x, y]
                           + 2 * (_fCurrent[1, x, y] + _fCurrent[5, x, y] + _fCurrent[4, x, y]);

                _fCurrent[3, x, y] = _fCurrent[1, x, y];
                _fCurrent[7, x, y] = _fCurrent[5, x, y] + rhoN * ModelParameters.U0 / 6;
                _fCurrent[6, x, y] = _fCurrent[4, x, y] - rhoN * ModelParameters.U0 / 6;
            }
        }

        private void CalculateComponents()
        {
            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                    {
                        _fPrevious[a, x, y] = _fCurrent[a, x, y];
                    }
                }
            }

            var sumRho = ArrayFactory.Create(NodeSpace.MaxX, NodeSpace.MaxY, 0.0);
            var sumU = ArrayFactory.Create(NodeSpace.MaxX, NodeSpace.MaxY, 0.0);
            var sumV = ArrayFactory.Create(NodeSpace.MaxX, NodeSpace.MaxY, 0.0);

            foreach (var node in _nodes)
            {
                var x = node.X ?? -1;
                var y = node.Y ?? -1;

                for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                {
                    var value = node.NodeType != NodeType.Liquid ? 0 : _fCurrent[a, x, y];

                    sumRho[x, y] += value;
                    sumU[x, y] += value * NodeSpace.LatticeVectors[a].Dx;
                    sumV[x, y] += value * NodeSpace.LatticeVectors[a].Dy;
                }
            }

            _rho = sumRho;

            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    _u[x, y] = sumU[x, y] / _rho[x, y];
                    _v[x, y] = sumV[x, y] / _rho[x, y];
                }
            }
        }

        private void ClearSolidCells()
        {
            foreach (var node in _nodes.Where(n => n.NodeType == NodeType.Solid))
            {
                var x = node.X ?? -1;
                var y = node.Y ?? -1;

                _u[x, y] = 0;
                _v[x, y] = 0;
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
