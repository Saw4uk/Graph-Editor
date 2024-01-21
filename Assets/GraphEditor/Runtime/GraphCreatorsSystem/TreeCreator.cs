using System;
using System.Collections.Generic;
using System.Linq;
using GraphEditor.Runtime;
using UnityEngine;
// ReSharper disable Unity.InefficientPropertyAccess

namespace GraphEditor
{
    public class TreeCreator: MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private MonoGraph monoGraphPrefab;
        [SerializeField] private MonoEdge monoEdgePrefab;
        [SerializeField] private MonoNode monoNodePrefab;

        [Header("Settings")]
        [SerializeField] private TreeCreatorEnumerableType treeCreatorEnumerableType;
        [SerializeField, Range(0, 100)] private float DistanceBetweenLayers;
        [SerializeField, Range(0, 100)] private float MinDistanceBetweenNodes;

        [Header("TreeSettings")]
        [SerializeField, Min(0)] private int layersCount = 4;
        [SerializeField] private Vector2Int heirsRange = new Vector2Int(2, 4);
        [SerializeField] private Vector2 rootPosition = Vector2.zero;
        
        private MonoGraph monoGraph;
        private IEnumerator<TreeEnumerableElement> treeEnumerator;

        private Dictionary<Tree, MonoNode> treeToNode;
        private Dictionary<MonoNode, Tree> nodeToTree;
        private int curNodeId;
        private int curEdgeId;
        private MonoNode rootNode;

        public MonoNode RootNode => rootNode;

        public MonoGraph Restart()
        {
            var tree = Tree.CreateRandomTree(layersCount, heirsRange);
            return Restart(tree, rootPosition);
        }
        
        public MonoGraph Restart(Tree root, Vector2 rootPosition)
        {
            switch (treeCreatorEnumerableType)
            {
                case TreeCreatorEnumerableType.BFS:
                    treeEnumerator = root.BfsEnumerate().GetEnumerator();
                    break;
                case TreeCreatorEnumerableType.DFS:
                    treeEnumerator = root.DfsEnumerate().GetEnumerator();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.rootPosition = rootPosition;
            monoGraph = Instantiate(monoGraphPrefab);
            monoGraph.Initialize();

            treeToNode = new Dictionary<Tree, MonoNode>();
            nodeToTree = new Dictionary<MonoNode, Tree>();
            curNodeId = 0;
            curEdgeId = 0;
            rootNode = null;
            return monoGraph;
        }

        public bool Iterate()
        {
            if (!treeEnumerator.MoveNext())
                return false;
            
            var element = treeEnumerator.Current;
            
            var node = Instantiate(monoNodePrefab, monoGraph.NodesParent.transform);
            node.transform.position = node.transform.position.WithY(GetLayerY(element.Layer));
            node.Initialize(curNodeId);
            curNodeId++;
            
            treeToNode[element.Curren] = node;
            nodeToTree[node] = element.Curren;
            
            monoGraph.AddNode(node);
            
            if (element.Parent == null)
            {
                rootNode = node;
                node.transform.position = rootPosition;
            }
            else
            {
                monoGraph.ConnectNodes(node, treeToNode[element.Parent], monoEdgePrefab, curEdgeId);
                curEdgeId++;
                DrawTree(rootNode);
            }
            
            return true;
        }

        public float DrawTree(MonoNode element)
        {
            var heirs = GetNodeHeirs(element);
        
            var widths = heirs
                .Select(DrawTree)
                .ToArray();
            var totalWidth = Math.Max(widths.Sum(), MinDistanceBetweenNodes);
            var curPoint = element.transform.position.x - totalWidth / 2;
            for (var i = 0; i < heirs.Count; i++)
            {
                var width = widths[i];
                var node = heirs[i];
                curPoint += width / 2;
                var offset = curPoint - node.transform.position.x;
                MoveNode(node, new Vector3(offset, 0,0));
                curPoint += width / 2;
            }

            return totalWidth;
        }

        private void MoveNode(MonoNode monoNode, Vector3 vector3)
        {
            foreach (var node in EnumerateNode(monoNode))
                node.transform.position += vector3;
        }

        private IReadOnlyList<MonoNode> GetNodeHeirs(MonoNode node)
        {
            var tree = nodeToTree[node];
            return tree.Heirs
                .Where(x => treeToNode.ContainsKey(x))
                .Select(x => treeToNode[x])
                .ToArray();
        }

        private IEnumerable<MonoNode> EnumerateNode(MonoNode rootNode)
        {
            foreach (var heir in GetNodeHeirs(rootNode))
            {
                foreach (var el in EnumerateNode(heir))
                    yield return el;
            }
            yield return rootNode;
        }

        private float GetLayerY(int layerIndex)
        {
            return rootPosition.y - layerIndex * DistanceBetweenLayers;
        }
    }

    public enum TreeCreatorEnumerableType
    {
        BFS,
        DFS
    }
}