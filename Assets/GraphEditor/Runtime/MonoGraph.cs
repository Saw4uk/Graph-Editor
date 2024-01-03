using UnityEngine;

namespace GraphEditor
{
    public class MonoGraph : MonoBehaviour, IMonoGraph
    {
        [field: SerializeField] public GameObject NodesParent { get; set; }
        [field: SerializeField] public GameObject EdgesParent { get; set; }
    }
}
