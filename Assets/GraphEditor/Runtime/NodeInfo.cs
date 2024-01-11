using System.Collections.Generic;
using UnityEngine;

namespace GraphEditor.Runtime
{
    public class NodeInfo
    {
        public readonly int id;
        public readonly IEnumerable<int> neighborsId;
        public readonly Vector2 position;

        public NodeInfo(int id, IEnumerable<int> neighborsId, Vector2 position)
        {
            this.id = id;
            this.neighborsId = neighborsId;
            this.position = position;
        }
    }
}
