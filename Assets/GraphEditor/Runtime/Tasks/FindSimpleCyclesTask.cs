using System.Collections.Generic;
using GraphEditor.Attributes;

namespace GraphEditor.Runtime
{
    public class FindSimpleCyclesTask : BaseTask<FindSimpleCyclesTask>
    {
        private int simpleCyclesCount;

        public FindSimpleCyclesTask(int simpleCyclesCount, TaskInfo taskInfo) : base(taskInfo)
        {
            this.simpleCyclesCount = simpleCyclesCount;
        }

        [DisplayName("Количество циклов")]
        public int SimpleCyclesCount => simpleCyclesCount;

        public override float CheckTask(MonoGraph monoGraph)
        {
            var adjacencyGraph = AdjacencyMatrixGraph.FromMonoGraph(monoGraph);
            var result = adjacencyGraph.FindNumberOfSimpleCycles();
            return GetMark(new List<bool>{result == simpleCyclesCount});
        }
    }
}
