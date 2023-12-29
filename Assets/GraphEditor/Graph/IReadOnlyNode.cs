using System.Collections.Generic;

namespace GraphEditor
{
    public interface IReadOnlyNode
    {
        public int Vertex { get; }
        public IReadOnlyCollection<int> NeighboursVertex { get; }
    }
}