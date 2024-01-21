using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(menuName = "Questions/Tree Depth Question")]
    public class TreeDepthQuestion : BaseQuestion, IInputQuestion
    {
        private int depth;
        public InputQuestionType InputQuestionType => InputQuestionType.Int;

        public override bool Check(MonoGraph monoGraph)
        {
            return depth == GetMaxDepth(monoGraph);
        }

        private int GetMaxDepth(MonoGraph monoGraph)
        {
            var rootNode = GraphEditorRoot.Instance.GetRootNode();
            var maxDepth = int.MinValue;
            var queue = new Queue<(MonoNode, int)>();
            var visited = new HashSet<MonoNode>();
            queue.Enqueue((rootNode, 0));
            while (queue.Count > 0)
            {
                var nodeInfo = queue.Dequeue();
                visited.Add(nodeInfo.Item1);
                maxDepth = Mathf.Max(maxDepth, nodeInfo.Item2);
                foreach(var neighbor in nodeInfo.Item1.GetNeighbors())
                {
                    if (visited.Contains(neighbor)) continue;
                    queue.Enqueue((neighbor, nodeInfo.Item2 + 1));
                }
            }

            return maxDepth;
        }

        public bool CheckInput(string data)
        {
            return int.TryParse(data, out _);
        }

        public void SetInputData(string data)
        {
            depth = int.Parse(data);
        }

        public bool TrySetInputData(string data)
        {
            return int.TryParse(data, out depth);
        }
    }
}
