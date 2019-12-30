using System;
using System.Xml.Schema;
using ComputationalFluidDynamics.Enums;
using ComputationalFluidDynamics.Nodes;

namespace ComputationalFluidDynamics.Factories
{
    public class NodeSpaceFactory
    {
        public static NodeSpaceXY Create(double minX, double maxX, double minY, double maxY, int resolution)
        {
            var nodesX = GetNodeCount(minX, maxX, resolution);
            var nodesY = GetNodeCount(minY, maxY, resolution);

            var latticeVectors = LatticeVectorCollectionFactory.Create(LatticeArrangement.D2Q9, 1);
            var nodeSpace = new NodeSpaceXY(latticeVectors, resolution, NodeType.None, nodesX, nodesY);

            foreach (var node in nodeSpace)
            {
                for (var a = 0; a < latticeVectors.Count; ++a)
                {
                    var x = node.X + latticeVectors[a].Dx;
                    var y = node.Y + latticeVectors[a].Dy;

                    if (x < 0 || y < 0 || x >= nodeSpace.MaxX || y >= nodeSpace.MaxY)
                        continue;

                    node.Neighbours[a] = nodeSpace[x, y];
                }
            }

            return nodeSpace;
        }

        public static NodeSpaceXYZ Create(double minX, double maxX, double minY, double maxY, double minZ, double maxZ,
            int resolution)
        {
            var nodesX = GetNodeCount(minX, maxX, resolution);
            var nodesY = GetNodeCount(minY, maxY, resolution);
            var nodesZ = GetNodeCount(minZ, maxZ, resolution);

            var latticeVectors = LatticeVectorCollectionFactory.Create(LatticeArrangement.D3Q19, 1);
            var nodeSpace = new NodeSpaceXYZ(latticeVectors, resolution, NodeType.None, nodesX, nodesY, nodesZ);

            foreach (var node in nodeSpace)
            {
                for (var a = 0; a < latticeVectors.Count; ++a)
                {
                    var x = node.X + latticeVectors[a].Dx;
                    var y = node.Y + latticeVectors[a].Dy;
                    var z = node.Z + latticeVectors[a].Dz;

                    if (x < 0 || y < 0 || z < 0 || x > nodeSpace.MaxX || y > nodeSpace.MaxY || z > nodeSpace.MaxZ)
                        continue;

                    node.Neighbours[a] = nodeSpace[x, y, z];
                }
            }

            return nodeSpace;
        }

        private static int GetNodeCount(double min, double max, int resolution)
        {
            return Convert.ToInt32(Math.Floor(Math.Abs(max - min) * resolution));
        }
    }
}