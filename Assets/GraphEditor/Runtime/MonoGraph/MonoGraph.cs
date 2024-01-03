using System.Collections.Generic;
using UnityEngine;

namespace GraphEditor
{
    public class MonoGraph : MonoBehaviour, IMonoGraph<MonoNode, MonoEdge>
    {
        [field: SerializeField] public GameObject NodesParent { get; set; }
        [field: SerializeField] public GameObject EdgesParent { get; set; }

        private readonly Dictionary<int, MonoNode> idToNode = new();
        private readonly Dictionary<int, MonoEdge> idToEdge = new();

        public IDictionary<int, MonoNode> IdToNode => idToNode;
        public IDictionary<int, MonoEdge> IdToEdge => idToEdge;
        public IEnumerable<MonoNode> Nodes => idToNode.Values;
        public IEnumerable<MonoEdge> Edges => idToEdge.Values;
        
        public int NodesCount => idToNode.Count;
        public int EdgesCount => idToEdge.Count;
    }
}
