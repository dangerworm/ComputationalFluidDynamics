using ComputationalFluidDynamics.Enums;

namespace ComputationalFluidDynamics.Nodes
{
    public class Node
    {
        public NodeType NodeType { get; set; }

        public int? X { get; }
        public int? Y { get; }
        public int? Z { get; }

        public Node(int? x, int? y, int? z)
        : this (NodeType.None, x, y, z)
        {
        }

        public Node(NodeType nodeType, int? x, int? y, int? z)
        {
            NodeType = nodeType;

            X = x;
            Y = y;
            Z = z;
        }
    }
}