using UnityEngine;

namespace GraphEditor.Runtime
{
    public abstract class TaskData : ScriptableObject
    {
        public abstract ITask GetNewTask();
    }
}

