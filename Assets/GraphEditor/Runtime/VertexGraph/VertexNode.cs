using System.Collections.Generic;
using System.Linq;

namespace GraphEditor
{
    public class VertexNode : IReadOnlyVertexNode
    {
        public int Vertex { get; }
        public IReadOnlyCollection<int> NeighboursVertex => NeighboursVertexSet;
        public HashSet<int> NeighboursVertexSet { get; } = new();

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