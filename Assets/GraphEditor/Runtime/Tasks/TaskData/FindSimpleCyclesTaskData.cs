using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(fileName = "New Simple Cycles Task", menuName = "Tasks/Cycles")]
    public class FindSimpleCyclesTaskData : TaskData
    {
        [SerializeField, Min(1)] private int minSimpleCyclesCount;
        [SerializeField, Min(1)] private int maxSimpleCyclesCount;

        private void OnValidate()
        {
            if(minSimpleCyclesCount > maxSimpleCyclesCount)
            {
                Debug.LogWarning($"Min Simple Cycles Count ({minSimpleCyclesCount}) is greater than" +
                    $"Max Simple Cycles Count ({maxSimpleCyclesCount})");
            }
        }

        public override ITask GetNewTask()
        {
            var simpleCyclesCount = UnityEngine.Random.Range(minSimpleCyclesCount, maxSimpleCyclesCount);
            return new FindSimpleCyclesTask(simpleCyclesCount);
        }
    }
}
