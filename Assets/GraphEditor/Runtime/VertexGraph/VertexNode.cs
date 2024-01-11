using System.Collections.Generic;
using System.Linq;

namespace GraphEditor.Runtime
{
    public class VertexNode : IReadOnlyVertexNode
    {
        public int Vertex { get; }
        public IReadOnlyCollection<int> NeighboursVertex => NeighboursVertexSet;
        public HashSet<int> NeighboursVertexSet { get; } = new HashSet<int>();

        public VertexNode(int vertex)
        {
            Vertex = vertex;
        }

        public override string ToString()
        {
            return $"{Vertex}: {string.Join(", ", NeighboursVertexSet)}";
        }
    }
}