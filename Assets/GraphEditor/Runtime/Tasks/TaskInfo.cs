using UnityEngine;

namespace GraphEditor
{
    [System.Serializable]
    public class TaskInfo
    {
        [SerializeField] private bool isEditable;
        [SerializeField] private GraphGenerator graphGenerator;

        public bool IsEditable => isEditable;
        public GraphGenerator GraphGenerator => graphGenerator;
    }
}