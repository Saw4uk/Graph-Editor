using GraphEditor.Attributes;

namespace GraphEditor.Runtime
{
    public class NodesAndEdgesTask : BaseTask<NodesAndEdgesTask>
    {
        private int nodeCount;
        private int edgeCount;

        public NodesAndEdgesTask(int nodeCount, int edgeCount, TaskInfo taskInfo) : base(taskInfo)
        {
            this.nodeCount = nodeCount;
            this.edgeCount = edgeCount;
        }

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
