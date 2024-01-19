using System.Collections.Generic;
using UnityEngine;

namespace GraphEditor
{
    public class Tree
    {
        private readonly List<Tree> heirs = new List<Tree>();
        public IReadOnlyList<Tree> Heirs => heirs;

        public Tree AddChild()
        {
            var tree = new Tree();
            heirs.Add(tree);
            return tree;
        }
        public IEnumerable<TreeEnumerableElement> DfsEnumerate()
        {
            var stack = new Stack<TreeEnumerableElement>();
            stack.Push(new TreeEnumerableElement(0, this, null));
            while (stack.Count != 0)
            {
                var element = stack.Pop();
                yield return element;
                foreach (var heir in element.Curren.heirs)
                    stack.Push(
                        new TreeEnumerableElement(
                            element.Layer + 1,
                            heir,
                            element.Curren)
                    );
            }
        }

        public IEnumerable<TreeEnumerableElement> BfsEnumerate()
        {
            var queue = new Queue<TreeEnumerableElement>();
            queue.Enqueue(new TreeEnumerableElement(0, this, null));
            while (queue.Count != 0)
            {
                var element = queue.Dequeue();
                yield return element;
                foreach (var heir in element.Curren.heirs)
                    queue.Enqueue(
                        new TreeEnumerableElement(
                            element.Layer + 1,
                            heir,
                            element.Curren)
                    );
            }
        }


        public static Tree CreateRandomTree(int countOfLayers, Vector2Int heirsRange)
        {
            var queue = new Queue<(int, Tree)>();
            var root = new Tree();
            queue.Enqueue((0, root));
            while (queue.Count != 0)
            {
                var (layer, tree) = queue.Dequeue();
                if (layer + 1 == countOfLayers)
                    continue;
                var heirsCount = Random.Range(heirsRange.x, heirsRange.y);
                for (var i = 0; i < heirsCount; i++)
                {
                    var heir = tree.AddChild();
                    queue.Enqueue((layer + 1, heir));
                }
            }

            return root;
        }
    }

    public readonly struct TreeEnumerableElement
    {
        public readonly int Layer;
        public readonly Tree Curren;
        public readonly Tree Parent;

        public TreeEnumerableElement(int layer, Tree curren, Tree parent)
        {
            Layer = layer;
            Curren = curren;
            Parent = parent;
        }
    }
}