using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ComputationalFluidDynamics.Enums;
using ComputationalFluidDynamics.LatticeVectors;

namespace ComputationalFluidDynamics.Nodes
{
    public abstract class NodeSpace : Collection<Node>
    {
        public LatticeVectorCollection LatticeVectors { get; set; }

        public bool IsInitialised { get; protected set; }

        public int Dimensionality { get; protected set; }
        public int Resolution { get; protected set; }

        public double Dx { get; protected set; }
        public double Dy { get; protected set; }

        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MaxZ { get; set; }

        protected bool HasXDimension { get; set; }
        protected bool HasYDimension { get; set; }
        protected bool HasZDimension { get; set; }

        protected NodeSpace(LatticeVectorCollection latticeVectors, int resolution)
        {
            IsInitialised = false;

            LatticeVectors = latticeVectors;

            Dimensionality = 0;
            Resolution = resolution;

            Dx = 1.0 / resolution;
            Dy = 1.0 / resolution;

            MaxX = int.MinValue;
            MaxY = int.MinValue;
            MaxZ = int.MinValue;

            HasXDimension = false;
            HasYDimension = false;
            HasZDimension = false;
        }

        public virtual IEnumerable<Node> NodeTypes(NodeType nodeTypes)
        {
            return this.Where(n => (nodeTypes & n.NodeType) == n.NodeType);
        }

        protected void Setup(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                AddNode(node);
            }

            Initialise();
        }

        protected void AddNode(Node node)
        {
            SetMaxValues(node);
            Add(node);
        }

        protected abstract void Initialise();

        protected virtual void SetMaxValues(Node node)
        {
            if (HasXDimension && MaxX <= node.X)
            {
                MaxX = node.X.Value + 1;
            }

            if (HasYDimension && MaxY <= node.Y)
            {
                MaxY = node.Y.Value + 1;
            }

            if (HasZDimension && MaxZ <= node.Z)
            {
                MaxZ = node.Z.Value + 1;
            }
        }

        public void ComputeFEquilibrium(double latticeVelocity)
        {
            var eSquared = Math.Pow(latticeVelocity, 2);

            Parallel.ForEach(this, n =>
            {
                var preCalc1 = 1.5 * (Math.Pow(n.Velocity[0], 2) + Math.Pow(n.Velocity[1], 2)) / eSquared;

                for (var a = 0; a < LatticeVectors.Count; a++)
                {
                    var preCalc2 = (n.Velocity[0] * LatticeVectors[a].X + 
                                    n.Velocity[1] * LatticeVectors[a].Y) / eSquared;

                    n.FEquilibrium[a] = n.Rho * LatticeVectors[a].Weighting *
                                             (1.0 + 3.0 * preCalc2 + 4.5 * Math.Pow(preCalc2, 2) - preCalc1);
                }
            });
        }
    }
}
