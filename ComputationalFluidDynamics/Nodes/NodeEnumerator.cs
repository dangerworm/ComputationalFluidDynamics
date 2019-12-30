using System;
using System.Collections;

namespace ComputationalFluidDynamics.Nodes
{
    public class NodeEnumerator : IEnumerator
    {
        private readonly int[,] _2dIndices;
        private readonly int[,,] _3dIndices;

        private readonly int _maxX;
        private readonly int _maxY;
        private readonly Node[] _nodes;

        private int _position;

        public NodeEnumerator(Node[] nodes, int[,] indices, int maxX)
        {
            _nodes = nodes;
            _2dIndices = indices;

            _maxX = maxX;

            _position = -1;
        }

        public NodeEnumerator(Node[] nodes, int[,,] indices, int maxX, int maxY)
        {
            _nodes = nodes;
            _3dIndices = indices;

            _maxX = maxX;
            _maxY = maxY;

            _position = -1;
        }

        public Node Current
        {
            get
            {
                try
                {
                    var x = _position % _maxX;

                    if (_3dIndices != null)
                    {
                        var y = _position / _maxX % _maxY;
                        var z = _position / (_maxX * _maxY);

                        return _nodes[_3dIndices[x, y, z]];
                    }

                    if (_2dIndices != null)
                    {
                        var y = _position / _maxX;

                        return _nodes[_2dIndices[x, y]];
                    }

                    return null;
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public bool MoveNext()
        {
            return ++_position < _nodes.Length;
        }

        public void Reset()
        {
            _position = -1;
        }

        object IEnumerator.Current => Current;
    }
}