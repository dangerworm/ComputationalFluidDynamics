using System.Collections.Generic;
using System.Collections.ObjectModel;
using ComputationalFluidDynamics.Enums;
using ComputationalFluidDynamics.LatticeVectors;

namespace ComputationalFluidDynamics.Nodes
{
    public abstract class NodeSpace3D : NodeSpace
    {
        protected int[,,] NodeIndices;

        protected NodeSpace3D(LatticeVectorCollection latticeVectors, int resolution = 1)
            : base(latticeVectors, resolution)
        {
            Dimensionality = 3;

            HasXDimension = HasYDimension = HasZDimension = true;
        }

        protected static IEnumerable<Node> GenerateNodes(LatticeVectorCollection latticeVectors,
            NodeType nodeType, int x, int y, int z)
        {
            var nodes = new Collection<Node>();
            for (var k = 0; k < z; ++k)
            for (var j = 0; j < y; ++j)
            for (var i = 0; i < x; ++i)
                nodes.Add(new Node(nodeType, i, j, k, latticeVectors.Count));

            return nodes;
        }
    }
}