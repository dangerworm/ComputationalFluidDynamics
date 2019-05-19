using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    }
}
