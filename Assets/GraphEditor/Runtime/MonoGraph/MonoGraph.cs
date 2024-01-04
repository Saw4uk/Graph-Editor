using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphEditor
{
    public class MonoGraph : MonoBehaviour
    {
        [field: SerializeField] public GameObject NodesParent { get; set; }
        [field: SerializeField] public GameObject EdgesParent { get; set; }

        private readonly Dictionary<int, MonoNode> idToNode = new();
        private readonly Dictionary<int, MonoEdge> idToEdge = new();

        public IDictionary<int, MonoNode> IdToNode => idToNode;
        public IDictionary<int, MonoEdge> IdToEdge => idToEdge;
        
        public IReadOnlyList<MonoNode> Nodes => idToNode.Values.ToList();
        public IReadOnlyList<MonoEdge> Edges => idToEdge.Values.ToList();
    }
}
