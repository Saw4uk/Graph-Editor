using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(fileName = "New Node Edges And Connected Task", menuName = "Tasks/Node Edges And Connected")]
    public class NodeEdgesAndConnectedTaskData : TaskData
    {
        [SerializeField] private bool shouldBeConnected;
        [Header("Node Settings")]
        [SerializeField, Min(1)] private int minNodeCount;
        [SerializeField, Min(1)] private int maxNodeCount;

        [Header("Edge Settings")]
        [SerializeField, Min(0.1f)] private float minEdgeToNodeRatio;
        [SerializeField, Min(0.1f)] private float maxEdgeToNodeRatio;



        private void OnValidate()
        {
            if (minNodeCount > maxNodeCount)
                Debug.LogWarning($"Min Node Count ({minNodeCount}) " +
                    $"is greater than Max Node Count ({maxNodeCount})!");
            if (minEdgeToNodeRatio > maxEdgeToNodeRatio)
                Debug.LogWarning($"Min Edge To Node Ration ({minEdgeToNodeRatio}) " +
                    $"is greater than Max Edge To Node Ratio ({maxEdgeToNodeRatio})!");

            var edgesLimit = (maxNodeCount * (maxNodeCount - 1)) / 2;
            var maxEdgeCount = maxNodeCount * maxEdgeToNodeRatio;
            if (edgesLimit < maxNodeCount * maxEdgeToNodeRatio)
                Debug.LogWarning($"Max Edge Count is greater than Edges Limit! " +
                    $"Max Edge Count: {maxEdgeCount}; Edges Limit: {edgesLimit}");
        }

        public override ITask GetNewTask()
        {
            var nodeCount = Random.Range(minNodeCount, maxNodeCount);
            var edgeToNodeRatio = Random.Range(minEdgeToNodeRatio, maxEdgeToNodeRatio);
            var edgeCount = (int)(nodeCount * edgeToNodeRatio);
            return new NodeEdgesAndConnectedTask(nodeCount, edgeCount, shouldBeConnected, taskInfo);
        }
    }
}
