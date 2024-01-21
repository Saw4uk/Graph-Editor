using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(fileName = "New Connected Task", menuName = "Tasks/Connected")]
    public class ConnectedTaskData : TaskData
    {
        [SerializeField] private bool isConnected;

        public override ITask GetNewTask()
        {
            return new ConnectedTask(isConnected, taskInfo);
        }
    }
}
