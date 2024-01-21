using System.Collections.Generic;
using GraphEditor.Attributes;

namespace GraphEditor.Runtime
{
    public class NodeEdgesAndConnectedTask : BaseTask<NodeEdgesAndConnectedTask>
    {
        private int nodeCount;
        private int edgeCount;
        private bool shouldBeConnected;

        public NodeEdgesAndConnectedTask(int nodeCount, int edgeCount, bool isConnected, TaskInfo taskInfo) : base(taskInfo)
        {
            this.nodeCount = nodeCount;
            this.edgeCount = edgeCount;
            this.shouldBeConnected = isConnected;
        }

        [DisplayName("Количество вершин")]
        public int NodeCount => nodeCount;

        [DisplayName("Количество ребер")]
        public int EdgeCount => edgeCount;

        [DisplayName("Должен ли быть связным")]
        public bool ShouldBeConnected => shouldBeConnected;

        public override float CheckTask(MonoGraph monoGraph)
        {
            return GetMark(monoGraph.Edges.Count == edgeCount, 
                monoGraph.Nodes.Count == nodeCount, 
                shouldBeConnected == monoGraph.CheckGraphForConnectivity());
        }
    }
}
