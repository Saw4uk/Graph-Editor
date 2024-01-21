using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(fileName = "New Node Task", menuName = "Tasks/Nodes")]
    public class NodesTaskData : TaskData
    {
        [Header("Node Settings")]
        [SerializeField, Min(1)] private int minNodeCount;
        [SerializeField, Min(1)] private int maxNodeCount;

        private void OnValidate()
        {
            if (minNodeCount > maxNodeCount)
                Debug.LogWarning($"Min Node Count ({minNodeCount}) " +
                    $"is greater than Max Node Count ({maxNodeCount})!");
        }

        public override ITask GetNewTask()
        {
            var nodeCount = Random.Range(minNodeCount, maxNodeCount);
            return new NodesTask(nodeCount, taskInfo);
        }
    }
}
