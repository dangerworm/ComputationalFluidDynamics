using ComputationalFluidDynamics.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ComputationalFluidDynamics.Nodes;

namespace ComputationalFluidDynamics.Factories
{
    public class NodeSpaceFactory
    {
        public static NodeSpaceXY Create(double minX, double maxX, double minY, double maxY, int resolution)
        {
            return (NodeSpaceXY)Create(minX, maxX, minY, maxY, 0, 0, resolution, NodeType.None);
        }

        public static NodeSpaceXYZ Create(double minX, double maxX, double minY, double maxY, double minZ, double maxZ, int resolution)
        {
            return (NodeSpaceXYZ)Create(minX, maxX, minY, maxY, minZ, maxZ, resolution, NodeType.None);
        }

        private static NodeSpace Create(double minX, double maxX, double minY, double maxY, double minZ, double maxZ, int resolution, NodeType defaultNodeType)
        {
            var x = Math.Abs(maxX - minX);
            var y = Math.Abs(maxY - minY);
            var z = Math.Abs(maxZ - minZ);

            var nodesX = (int)Math.Floor(x * resolution);
            var nodesY = (int)Math.Floor(y * resolution);
            var nodesZ = (int)Math.Floor(z * resolution);

            if (nodesX > 0 && nodesY > 0 && nodesZ > 0)
            {
                var nodes = GenerateNodes(defaultNodeType, nodesX, nodesY, nodesZ);
                var latticeVectors = LatticeVectorCollectionFactory.Create(LatticeArrangement.D3Q19, 1);

                return new NodeSpaceXYZ(latticeVectors, nodes, resolution);
            }

            if (nodesX > 0 && nodesY > 0)
            {
                var nodes = GenerateNodes(defaultNodeType, nodesX, nodesY);
                var latticeVectors = LatticeVectorCollectionFactory.Create(LatticeArrangement.D2Q9, 1);

                return new NodeSpaceXY(latticeVectors, nodes, resolution);
            }

            return null;
        }

        private static IEnumerable<Node> GenerateNodes(NodeType nodeType, int x, int y)
        {
            var nodes = new Collection<Node>();
            for (var j = 0; j < y; j++)
            {
                for (var i = 0; i < x; i++)
                {
                    nodes.Add(new Node(nodeType, i, j, null));
                }
            }

            return nodes;
        }

        private static IEnumerable<Node> GenerateNodes(NodeType nodeType, int x, int y, int z)
        {
            var nodes = new Collection<Node>();
            for (var k = 0; k < z; k++)
            {
                for (var j = 0; j < y; j++)
                {
                    for (var i = 0; i < x; i++)
                    {
                        nodes.Add(new Node(nodeType, i, j, k));
                    }
                }
            }

            return nodes;
        }
    }
}
