using System.Collections.Generic;
using System.Linq;

namespace GraphEditor.Runtime
{
    public class Vertex : IReadOnlyVertex
    {
        public int Value { get; }
        public HashSet<int> NeighboursVertexSet { get; } = new HashSet<int>();
        public IReadOnlyCollection<int> NeighboursVertex => NeighboursVertexSet;

        public Vertex(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{Value}: {string.Join(", ", NeighboursVertexSet)}";
        }
    }
}