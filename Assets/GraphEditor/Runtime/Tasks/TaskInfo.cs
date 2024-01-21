using UnityEngine;

namespace GraphEditor
{
    [System.Serializable]
    public class TaskInfo
    {
        [SerializeField] private bool isEditable;
        [SerializeField] private GraphGenerator graphGenerator;
        [SerializeField] [TextArea] private string description;

        public bool IsEditable => isEditable;
        public GraphGenerator GraphGenerator => graphGenerator;

        public string Description => description;
    }
}