using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    [System.Serializable]
    public abstract class BaseQuestion : ScriptableObject
    {
        [SerializeField, TextArea] private string questionText;
        public string GetQuestionText => questionText;
        public abstract bool Check(MonoGraph monoGraph);
    }

    public interface IInputQuestion
    {
        InputQuestionType InputQuestionType { get; }
        void SetInputData(string data);
        bool CheckInput(string data);
        bool TrySetInputData(string data);
    }

    public enum InputQuestionType
    {
        Int,
        Checkbox
    }
}