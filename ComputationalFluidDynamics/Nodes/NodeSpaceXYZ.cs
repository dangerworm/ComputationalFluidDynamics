using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ComputationalFluidDynamics.Enums;
using ComputationalFluidDynamics.LatticeVectors;

namespace ComputationalFluidDynamics.Nodes
{
    public class NodeSpaceXYZ : NodeSpace3D, IEnumerable
    {
        public NodeSpaceXYZ(LatticeVectorCollection latticeVectors, int resolution = 1)
            : base(latticeVectors, resolution)
        {
        }

        public NodeSpaceXYZ(LatticeVectorCollection latticeVectors, int resolution, IEnumerable<Node> nodes)
            : this(latticeVectors, resolution)
        {
            Setup(nodes);
        }

        public NodeSpaceXYZ(LatticeVectorCollection latticeVectors, int resolution,
            NodeType defaultNodeType, int nodesX, int nodesY, int nodesZ)
            : this(latticeVectors, resolution, GenerateNodes(latticeVectors, defaultNodeType, nodesX, nodesY, nodesZ))
        {
        }

        public Node this[int x, int y, int z]
        {
            get
            {
                if (!IsInitialised)
                    throw new InvalidOperationException("Cannot access node: node space has not yet been initialised.");

                return Items[NodeIndices[x, y, z]];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public new NodeEnumerator GetEnumerator()
        {
            return new NodeEnumerator(Items.ToArray(), NodeIndices, MaxX, MaxY);
        }

        protected override void Initialise()
        {
            NodeIndices = new int[MaxX, MaxY, MaxZ];

            for (var n = 0; n < Items.Count; ++n)
            {
                var node = Items[n];

                NodeIndices[node.X, node.Y, node.Z] = n;
            }

            IsInitialised = true;
        }
    }
}