using GraphEditor.Attributes;

namespace GraphEditor.Runtime
{
    public class BridgesTask : BaseTask<BridgesTask>
    {
        private int bridgesCount;

        public BridgesTask(int bridgesCount, TaskInfo taskInfo) : base(taskInfo)
        {
            this.bridgesCount = bridgesCount;
        }

        //[DisplayName("Количество мостов")]
        public int BridgesCount => bridgesCount;
        
        public override bool CheckTask(MonoGraph monoGraph)
        {
            var graph = new UndirectedVertexGraph();
            foreach(var node in monoGraph.Nodes)
            {
                graph.AddNode(node.Id);
            }

            foreach(var edges in monoGraph.Edges)
            {
                graph.ConnectNodes(edges.FirstNode.Id, edges.SecondNode.Id);
            }

            if(!graph.IsConnectedGraph())
            {
                return false;
            }

            var currentBridgesCount = 0;
            foreach(var edge in monoGraph.Edges)
            {
                graph.DisconnectNodes(edge.FirstNode.Id, edge.SecondNode.Id);
                if(!graph.IsConnectedGraph())
                {
                    currentBridgesCount++;
                }
                graph.Undo();
            }

            return bridgesCount == currentBridgesCount;
        }
    }
}
