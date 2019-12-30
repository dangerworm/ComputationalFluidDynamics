using System.Collections.Generic;
using System.Collections.ObjectModel;
using ComputationalFluidDynamics.Enums;
using ComputationalFluidDynamics.LatticeVectors;

namespace ComputationalFluidDynamics.Nodes
{
    public abstract class NodeSpace2D : NodeSpace
    {
        protected int[,] NodeIndices;

        protected NodeSpace2D(LatticeVectorCollection latticeVectors, int resolution = 1)
            : base(latticeVectors, resolution)
        {
            Dimensionality = 2;

            HasXDimension = HasYDimension = true;
        }

        protected static IEnumerable<Node> GenerateNodes(LatticeVectorCollection latticeVectors,
            NodeType nodeType, int x, int y)
        {
            var nodes = new Collection<Node>();
            for (var j = 0; j < y; ++j)
            {
                for (var i = 0; i < x; ++i)
                {
                    nodes.Add(new Node(nodeType, i, j, latticeVectors.Count));
                }
            }

            return nodes;
        }
    }
}