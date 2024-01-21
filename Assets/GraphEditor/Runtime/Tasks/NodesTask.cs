using System.Collections.Generic;
using GraphEditor.Attributes;

namespace GraphEditor.Runtime
{
    public class NodesTask : BaseTask<NodesTask>
    {
        private int nodeCount;

        public NodesTask(int nodeCount, TaskInfo taskInfo) : base(taskInfo)
        {
            this.nodeCount = nodeCount;
        }

        [DisplayName("Количество вершин")]
        public int NodeCount => nodeCount;

        public override float CheckTask(MonoGraph monoGraph)
        {
            return GetMark(new List<bool> {monoGraph.Nodes.Count == nodeCount });
        }
    }
}
