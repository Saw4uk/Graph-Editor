
using System.ComponentModel;
using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(fileName = "Cycle Task", menuName = "Tasks/Cycles")]
    public class FindSimpleCyclesTask : BaseTask<FindSimpleCyclesTask>
    {
        [SerializeField] private int simpleCyclesCount;

        [DisplayName("Количество циклов")]
        public int SimpleCyclesCount => simpleCyclesCount;

        public override bool CheckTask(MonoGraph monoGraph)
        {
            var adjacencyGraph = AdjacencyMatrixGraph.FromMonoGraph(monoGraph);
            var result = adjacencyGraph.FindNumberOfSimpleCycles();
            return result == simpleCyclesCount;
        }
    }
}
