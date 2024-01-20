using UnityEngine;

namespace GraphEditor.Runtime
{
    public abstract class TaskData : ScriptableObject
    {
        [SerializeField] protected TaskInfo taskInfo;
        
        public abstract ITask GetNewTask();
    }
}

