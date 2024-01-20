using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    [CreateAssetMenu(menuName = "Questions/Number Edges Question")]
    public class NumberEdgesQuestion : BaseQuestion, IInputQuestion
    {
        private int numberEdges;
        
        public InputQuestionType InputQuestionType => InputQuestionType.Int;
        
        public override bool Check(MonoGraph monoGraph)
        {
            return monoGraph.Edges.Count == numberEdges;
        }

        public void SetInputData(string data)
        {
            numberEdges = int.Parse(data);
        }

        public bool CheckInput(string data)
        {
            return int.TryParse(data, out _);
        }

        public bool TrySetInputData(string data)
        {
            return int.TryParse(data, out numberEdges);
        }
    }
}