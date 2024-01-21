using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    [CreateAssetMenu(menuName = "Questions/Connected Question")]
    public class ConnectedQuestion : BaseQuestion, IInputQuestion
    {
        private bool isConnected;

        public InputQuestionType InputQuestionType => InputQuestionType.Checkbox;

        public override bool Check(MonoGraph monoGraph)
        {
            return monoGraph.CheckGraphForConnectivity() == isConnected;
        }

        public void SetInputData(string data)
        {
            isConnected = bool.Parse(data);
        }

        public bool CheckInput(string data)
        {
            return bool.TryParse(data, out _);
        }

        public bool TrySetInputData(string data)
        {
            return bool.TryParse(data, out isConnected);
        }
    }
}