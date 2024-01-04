using System.Collections.Generic;

namespace GraphEditor
{
    public interface IReadOnlyVertexNode
    {
        public int Vertex { get; }
        public IReadOnlyCollection<int> NeighboursVertex { get; }
    }
}