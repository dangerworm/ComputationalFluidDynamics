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
        }

        public NodeSpaceXY(LatticeVectorCollection latticeVectors, int resolution, IEnumerable<Node> nodes)
            : this(latticeVectors, resolution)
        {
            Setup(nodes);
        }

        public NodeSpaceXY(LatticeVectorCollection latticeVectors, int resolution,
            NodeType defaultNodeType, int nodesX, int nodesY)
            : this(latticeVectors, resolution, GenerateNodes(latticeVectors, defaultNodeType, nodesX, nodesY))
        {
        }

        public Node this[int x, int y]
        {
            get
            {
                if (!IsInitialised)
                    throw new InvalidOperationException("Cannot access node: node space has not yet been initialised.");

                return Items[NodeIndices[x, y]];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public new NodeEnumerator GetEnumerator()
        {
            return new NodeEnumerator(Items.ToArray(), NodeIndices, MaxX);
        }

        protected override void Initialise()
        {
            NodeIndices = new int[MaxX, MaxY];

            for (var n = 0; n < Items.Count; ++n)
            {
                var node = Items[n];

                NodeIndices[node.X, node.Y] = n;
            }

            IsInitialised = true;
        }
    }
}