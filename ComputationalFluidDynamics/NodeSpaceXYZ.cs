using System;
using System.Collections.Generic;

namespace ComputationalFluidDynamics
{
    public class NodeSpaceXYZ : NodeSpace3D
    {
        public NodeSpaceXYZ(LatticeVectorCollection latticeVectors)
        : base(latticeVectors)
        {
            Dimensionality = 3;

            HasXDimension = true;
            HasYDimension = true;
            HasZDimension = true;
        }

        public NodeSpaceXYZ(LatticeVectorCollection latticeVectors, IEnumerable<Node> nodes)
            : this(latticeVectors)
        {
            Setup(nodes);
        }

        public Node this[int x, int y, int z]
        {
            get
            {
                if (!IsInitialised)
                {
                    throw new InvalidOperationException("Cannot access node: node space has not yet been initialised.");
                }

                return Items[NodeIndices[x, y, z]];
            }
        }

        protected override void Initialise()
        {
            NodeIndices = new int[MaxX, MaxY, MaxZ];

            for (var n = 0; n < Items.Count; n++)
            {
                var node = Items[n];

                if (!node.X.HasValue || !node.Y.HasValue || !node.Z.HasValue)
                {
                    throw new InvalidOperationException("A node was passed to X-Y-Z node space with a null X, Y or Z value.");
                }

                NodeIndices[node.X.Value, node.Y.Value, node.Z.Value] = n;
            }

            IsInitialised = true;
        }
    }
}
