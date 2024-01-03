using UnityEditor.EditorTools;
using UnityEngine;


namespace GraphEditor
{
    [EditorTool("CreateGraph")]
    public class GraphTool : GraphToolBase<MonoNode, MonoEdge, MonoGraph>
    {
        protected override MonoNode GetNodePrefab()
        {
            return Resources.Load<MonoNode>("Prefabs/Node");
        }

        protected override MonoEdge GetEdgePrefab()
        {
            return Resources.Load<MonoEdge>("Prefabs/Edge");
        }
    }
}