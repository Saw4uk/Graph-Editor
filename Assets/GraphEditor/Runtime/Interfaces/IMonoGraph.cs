using UnityEngine;

namespace GraphEditor
{
    public interface IMonoGraph
    {
        GameObject NodesParent { get; set; }
        GameObject EdgesParent { get; set; }
    }
}