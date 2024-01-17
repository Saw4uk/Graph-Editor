using System.Collections.Generic;

namespace GraphEditor.Runtime
{
    public interface IReadOnlyVertex
    {
        int Value { get; }
        IReadOnlyCollection<int> NeighboursVertex { get; }
    }
}