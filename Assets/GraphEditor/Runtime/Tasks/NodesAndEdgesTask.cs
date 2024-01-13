using GraphEditor.Attributes;
using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(fileName = "New Nodes And Edges Task", menuName = "Tasks/Nodes and Edges")]
    public class NodesAndEdgesTask : BaseTask<NodesAndEdgesTask>
    {
        [SerializeField, Min(1)] private int nodeCount;
        [SerializeField, Min(1)] private int edgeCount;
        
        [DisplayName("Количество вершин")]
        public int NodeCount => nodeCount;

        [DisplayName("Количество ребер")] 
        public int EdgeCount => edgeCount;

        public override bool CheckTask(MonoGraph monoGraph)
        {
            return monoGraph.Edges.Count == edgeCount && 
                   monoGraph.Nodes.Count == nodeCount;
        }
    }
}
