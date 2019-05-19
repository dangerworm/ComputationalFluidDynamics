using System;
using System.Collections;

namespace ComputationalFluidDynamics.Nodes
{
    public class NodeEnumerator : IEnumerator
    {
        private readonly Node[] _nodes;
        private readonly int[,] _indices;
        private int _maxY;
        private int _position;

        public NodeEnumerator(Node[] nodes, int[,] indices, int maxY)
        {
            _nodes = nodes;
            _indices = indices;
            _maxY = maxY;
            _position = -1;
        }

        public bool MoveNext()
        {
            _position++;
            return _position < _nodes.Length;
        }

        public void Reset()
        {
            _position = -1;
        }

        object IEnumerator.Current => Current;

        public Node Current
        {
            get
            {
                try
                {
                    return _nodes[_indices[_position % _maxY, _position / _maxY]];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
