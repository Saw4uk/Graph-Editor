using System.Collections.Generic;

namespace GraphEditor.Runtime
{
    public interface IReadOnlyVertexNode
    {
        int Vertex { get; }
        IReadOnlyCollection<int> NeighboursVertex { get; }
    }
}