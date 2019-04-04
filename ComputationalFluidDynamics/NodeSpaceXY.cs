using System;
using System.Collections.Generic;

namespace ComputationalFluidDynamics
{
    public class NodeSpaceXY : NodeSpace2D
    {
        public NodeSpaceXY(LatticeVectorCollection latticeVectors)
        : base(latticeVectors)
        {
            Dimensionality = 2;

            HasXDimension = true;
            HasYDimension = true;
        }

        public NodeSpaceXY(LatticeVectorCollection latticeVectors, IEnumerable<Node> nodes)
            : this(latticeVectors)
        {
            Setup(nodes);
        }

        public Node this[int x, int y]
        {
            get
            {
                if (!IsInitialised)
                {
                    throw new InvalidOperationException("Cannot access node: node space has not yet been initialised.");
                }

                return Items[NodeIndices[x, y]];
            }
        }

        protected override void Initialise()
        {
            NodeIndices = new int[MaxX, MaxY];

            for (var n = 0; n < Items.Count; n++)
            {
                var node = Items[n];

                if (!node.X.HasValue || !node.Y.HasValue)
                {
                    throw new InvalidOperationException("A node was passed to X-Y node space with a null X or Y value.");
                }

                NodeIndices[node.X.Value, node.Y.Value] = n;
            }

            IsInitialised = true;
        }
    }
}
