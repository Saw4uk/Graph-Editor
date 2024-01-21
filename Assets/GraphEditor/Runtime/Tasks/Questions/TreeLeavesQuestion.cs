using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(menuName = "Questions/Tree Leaves Question")]
    public class TreeLeavesQuestion : BaseQuestion, IInputQuestion
    {
        private int leavesCount;
        public InputQuestionType InputQuestionType => InputQuestionType.Int;

        public override bool Check(MonoGraph monoGraph)
        {
            var rootNode = GraphEditorRoot.Instance.GetRootNode();
            return leavesCount == monoGraph.Nodes
                .Where(node => node != rootNode && node.GetNeighbors().Count() < 2)
                .Count();
        }

        public bool CheckInput(string data)
        {
            return int.TryParse(data, out _);
        }

        public void SetInputData(string data)
        {
            leavesCount = int.Parse(data);
        }

        public bool TrySetInputData(string data)
        {
            return int.TryParse(data, out leavesCount);
        }
    }
}
