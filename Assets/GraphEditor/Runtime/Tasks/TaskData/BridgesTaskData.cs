using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(fileName = "New Bridges Task", menuName = "Tasks/Bridges")]
    public class BridgesTaskData : TaskData
    {
        [SerializeField, Min(1)] private int minBridgesCount;
        [SerializeField, Min(1)] private int maxBridgesCount;

        private void OnValidate()
        {
            if (minBridgesCount > maxBridgesCount)
                Debug.LogWarning($"Min Bridges Count({minBridgesCount}) is greater than" +
                    $"Max Bridges Count({maxBridgesCount})!");
        }

        public override ITask GetNewTask()
        {
            var bridgesCount = UnityEngine.Random.Range(minBridgesCount, maxBridgesCount);
            return new BridgesTask(bridgesCount, taskInfo);
        }
    }
}
