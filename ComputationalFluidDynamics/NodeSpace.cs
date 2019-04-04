using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ComputationalFluidDynamics
{
    public abstract class NodeSpace : Collection<Node>
    {
        public LatticeVectorCollection LatticeVectors { get; set; }

        public int Dimensionality { get; protected set; }
        public bool IsInitialised { get; protected set; }

        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MaxZ { get; set; }

        protected bool HasXDimension;
        protected bool HasYDimension;
        protected bool HasZDimension;

        protected NodeSpace(LatticeVectorCollection latticeVectors)
        {
            IsInitialised = false;

            LatticeVectors = latticeVectors;

            Dimensionality = 0;

            HasXDimension = false;
            HasYDimension = false;
            HasZDimension = false;

            MaxX = int.MinValue;
            MaxY = int.MinValue;
            MaxZ = int.MinValue;
        }

        protected void Setup(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                AddNode(node);
            }

            Initialise();
        }

        protected abstract void Initialise();

        protected void AddNode(Node node)
        {
            SetMaxValues(node);
            Add(node);
        }

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
