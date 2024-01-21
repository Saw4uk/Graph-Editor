using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    [CreateAssetMenu(menuName = "Questions/Number Nodes Question")]
    public class NumberNodesQuestion : BaseQuestion, IInputQuestion
    {
        private int numberNodes;

        public InputQuestionType InputQuestionType => InputQuestionType.Int;
    
        public override bool Check(MonoGraph monoGraph)
        {
            return monoGraph.Nodes.Count == numberNodes;
        }

        public void SetInputData(string data)
        {
            numberNodes = int.Parse(data);
        }

        public bool CheckInput(string data)
        {
            return int.TryParse(data, out _);
        }

        public bool TrySetInputData(string data)
        {
            return int.TryParse(data, out numberNodes);
        }
    }
}
