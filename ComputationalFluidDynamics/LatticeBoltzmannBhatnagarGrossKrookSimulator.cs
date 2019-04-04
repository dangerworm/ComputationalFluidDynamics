using ComputationalFluidDynamics.Enums;
using System;
using System.Linq;

namespace ComputationalFluidDynamics
{
    public class LatticeBoltzmannBhatnagarGrossKrookSimulator : ISimulator
    {
        public NodeSpace NodeSpace { get; set; }

        public bool IsInitialised { get; private set; }

        public int MaxIterations { get; set; }
        public int CurrentIteration { get; set; }
        public double SimulatorTime => CurrentIteration * _dt;

        #region Parameters

        private readonly double _u0 = 1;
        private readonly double _rho0 = 100;
        private readonly double _dt = 0.01;
        private readonly double _tau = 0.8;

        private NodeSpaceXY Nodes => (NodeSpaceXY) NodeSpace;
        private double _dx => 1.0 / NodeSpace.Resolution;
        private double _e => _dx / _dt;
        private double _nu => Math.Pow(_e, 2) * _dx * (2 * _tau - 1) / 6;
        private double _Re => _u0 * NodeSpace.MaxY / _nu;

        #endregion Parameters

        #region Variables

        private double[,,] _f;
        private double[,,] _feq;
        private double[,,] _fTemp;
        private double[,] _rho;
        private double[,] _u;
        private double[,] _v;

        #endregion Variables


        public LatticeBoltzmannBhatnagarGrossKrookSimulator(NodeSpace nodeSpace, int iterations)
        {
            NodeSpace = nodeSpace;
            MaxIterations = iterations;

            Initialise();
        }

        private void Initialise()
        {
            _f = DataHelper.GetNew3DArray(9, NodeSpace.MaxX, NodeSpace.MaxY, 0.0);
            _feq = DataHelper.GetNew3DArray(9, NodeSpace.MaxX, NodeSpace.MaxY, 0.0);
            _fTemp = DataHelper.GetNew3DArray(9, NodeSpace.MaxX, NodeSpace.MaxY, 0.0);
            _rho = DataHelper.GetNew2DArray(NodeSpace.MaxX, NodeSpace.MaxY, _rho0);
            _u = DataHelper.GetNew2DArray(NodeSpace.MaxX, NodeSpace.MaxY, 0.0);
            _v = DataHelper.GetNew2DArray(NodeSpace.MaxX, NodeSpace.MaxY, 0.0);

            var validNodeTypes = new[] {NodeType.Boundary, NodeType.Flow};

            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    if (!validNodeTypes.Contains(Nodes[x, y].NodeType))
                    {
                        continue;
                    }

                    _u[x, y] = 0.0;
                    _v[x, y] = -_u0;
                }
            }

            CurrentIteration = 0;

            IsInitialised = true;
        }

        public void Step()
        {
            if (!IsInitialised)
            {
                throw new InvalidOperationException("The simulator has not yet been initialised.");
            }

            CurrentIteration++;

            ComputeFeq();
            CollideStream();
            BounceBack();

            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                    {
                        _f[a, x, y] = _fTemp[a, x, y];
                    }
                }
            }
        }

        private void ComputeFeq()
        {
            var eSquared = Math.Pow(_e, 2);

            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    var part1 = (_u[x, y] * _u[x, y] + _v[x, y] * _v[x, y]) / eSquared;

                    for (var a = 0; a < NodeSpace.LatticeVectors.Count; x++)
                    {
                        var ex = NodeSpace.LatticeVectors[a].X;
                        var ey = NodeSpace.LatticeVectors[a].Y;

                        var part2 = (_u[x, y] * ex + _v[x, y] * ey) / eSquared;

                        _feq[a, x, y] = _rho[x, y] * NodeSpace.LatticeVectors[a].Weighting *
                                        (1.0 + 3.0 * part2 + 4.5 * Math.Pow(part2, 2) - 1.5 * part1);
                    }
                }
            }
        }

        private void CollideStream()
        {
            var validNodeTypes = new[] {NodeType.Boundary, NodeType.Flow};

            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                for (var x = 0; x < NodeSpace.MaxX; x++)
                {
                    var a = 0;
                    if (!validNodeTypes.Contains(Nodes[x, y].NodeType))
                    {
                        for (a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                        {
                            _fTemp[a, x, y] = 0;
                        }

                        continue;
                    }

                    for (a = 0; a < NodeSpace.LatticeVectors.Count - 1; a++)
                    {
                        var newFTempValue = GetNewFTempValue(a, x, y, validNodeTypes);
                        if (newFTempValue != null)
                        {
                            _fTemp[a, x, y] = newFTempValue.Value;
                        }
                    }

                    a = NodeSpace.LatticeVectors.Count - 1;
                    _fTemp[a, x, y] = _f[a, x, y] - (_f[a, x, y] - _feq[a, x, y]) / _tau;
                }
            }
        }

        private double? GetNewFTempValue(int a, int x, int y, NodeType[] validNodeTypes)
        {
            var dx = NodeSpace.LatticeVectors[a].Dx;
            var dy = NodeSpace.LatticeVectors[a].Dy;

            if (!validNodeTypes.Contains(Nodes[x + dx, y + dy].NodeType) ||
                (dx > 0 && x + dx > NodeSpace.MaxX) ||
                (dy > 0 && y + dy > NodeSpace.MaxY) ||
                (dx < 0 && x - dx < 0) ||
                (dy < 0 && y - dy < 0))
            {
                return null;
            }

            return _f[a, x, y] - (_f[a, x, y] - _feq[a, x, y]) / _tau;
        }

        private void BounceBack()
        {
            for (var y = 0; y < NodeSpace.MaxY; y++)
            {
                /* Left */
                _fTemp[0, 1, y] = _fTemp[2, 1, y];
                _fTemp[4, 1, y] = _fTemp[6, 1, y];
                _fTemp[7, 1, y] = _fTemp[5, 1, y];

                /* Right */
                _fTemp[2, NodeSpace.MaxX, y] = _fTemp[0, NodeSpace.MaxX, y];
                _fTemp[6, NodeSpace.MaxX, y] = _fTemp[4, NodeSpace.MaxX, y];
                _fTemp[5, NodeSpace.MaxX, y] = _fTemp[7, NodeSpace.MaxX, y];
            }

            /* Bottom */
            for (var x = 0; x < NodeSpace.MaxX; x++)
            {
                for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                {
                    _fTemp[a, x, 0] = _fTemp[a, x, 1];

                    _fTemp[a, x, NodeSpace.MaxY] = _feq[a, x, NodeSpace.MaxY];
                }
            }

            var temp = new double[NodeSpace.LatticeVectors.Count];
            for (var y = 1; y < NodeSpace.MaxY - 1; y++)
            {
                for (var x = 1; x < NodeSpace.MaxX - 1; x++)
                {
                    if (Nodes[x, y].NodeType != NodeType.Flow)
                    {
                        continue;
                    }

                    for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                    {
                        temp[a] = _fTemp[a, x, y];
                    }

                    for (var a = 0; a < NodeSpace.LatticeVectors.Count; a++)
                    {
                        var vector = NodeSpace.LatticeVectors[a];
                        var opposite = NodeSpace.LatticeVectors.GetOppositeIndex(vector);

                        var dx = vector.Dx;
                        var dy = vector.Dy;

                        if (Nodes[x + dx, y + dy].NodeType == NodeType.Solid)
                        {
                            _fTemp[opposite, x, y] = temp[a];
                        }
                    }
                }
            }
        }
    }
}
