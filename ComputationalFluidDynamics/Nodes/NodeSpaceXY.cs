using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ComputationalFluidDynamics.Enums;
using ComputationalFluidDynamics.LatticeVectors;

namespace ComputationalFluidDynamics.Nodes
{
    public class NodeSpaceXY : NodeSpace2D, IEnumerable
    {
        public NodeSpaceXY(LatticeVectorCollection latticeVectors, int resolution = 1)
        : base(latticeVectors, resolution)
        {
            Dimensionality = 2;

            HasXDimension = true;
            HasYDimension = true;
        }

        public NodeSpaceXY(LatticeVectorCollection latticeVectors, IEnumerable<Node> nodes, int resolution)
            : this(latticeVectors, resolution)
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public new NodeEnumerator GetEnumerator()
        {
            return new NodeEnumerator(Items.ToArray(), NodeIndices, MaxY);
        }
    }
}
