using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GraphEditor.Runtime
{
    public class GraphTool : MonoBehaviour
    {
        public static GraphTool Instance { get; private set; }

        [Header("Prefabs")] [SerializeField] private MonoGraph graphPrefab;
        [SerializeField] private MonoNode monoNodePrefab;
        [SerializeField] private MonoEdge monoEdgePrefab;

        private MonoGraph graph;

        public MonoGraph Graph => graph;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogError($"Больше чем два {nameof(GraphTool)} на сцене");
                Destroy(gameObject);
            }

            AssignGraph();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete))
                DeleteSelectedNodes();

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    Undo.InvokeRedoAction();
                else
                    Undo.InvokeUndoAction();
            }
        }

        private void AssignGraph()
        {
            var graphObj = GameObject.Find("Graph") ?? new GameObject("Graph");
            graph = graphObj.GetComponent<MonoGraph>() ??
                    graphObj.AddComponent<MonoGraph>();
            graph.Initialize();

            if (graph.NodesParent == null)
            {
                graph.NodesParent = new GameObject("Nodes");
                graph.NodesParent.transform.SetParent(graph.transform);
            }

            if (graph.EdgesParent == null)
            {
                graph.EdgesParent = new GameObject("Edges");
                graph.EdgesParent.transform.SetParent(graph.transform);
            }
        }

        public MonoNode CreateNode(Vector2 position)
        {
            var newNode = _CreateNode(position);

            Undo.AddActions(
                () => _DeleteNode(newNode.Id),
                () => _CreateNode(position)
            );

            return newNode;
        }

        private MonoNode _CreateNode(NodeInfo nodeInfo)
        {
            var node = Instantiate(monoNodePrefab, graph.NodesParent.transform);
            node.transform.position = nodeInfo.position;
            node.Initialize(nodeInfo.id);

            graph.AddNode(node);

            foreach (var neighborId in nodeInfo.neighborsId)
                if (graph.ContainsNode(neighborId))
                    _ConnectNodes(nodeInfo.id, neighborId);

            NodeSelector.Instance.Add(node.Id);

            return node;
        }

        private MonoNode _CreateNode(Vector2 position)
        {
            var node = Instantiate(monoNodePrefab, graph.NodesParent.transform);
            node.transform.position = position;
            node.Initialize(GetNextIndex(node));

            graph.AddNode(node);

            return node;
        }

        public bool DeleteNode(int nodeId)
        {
            var nodeInfo = GetNodeById(nodeId).GetNodeInfo();
            var removed = _DeleteNode(nodeInfo.id);

            if (removed)
                Undo.AddActions(
                    () => _CreateNode(nodeInfo),
                    () => _DeleteNode(nodeInfo.id)
                );

            return removed;
        }

        private bool _DeleteNode(int nodeId)
        {
            var node = GetNodeById(nodeId);

            var allDeletedEdges = node.Edges.ToList();
            var allNeighboringNodes = node.GetNeighbors().ToList();

            foreach (var neighbor in allNeighboringNodes)
            {
                var myDeleteEdges = neighbor.Edges.Intersect(allDeletedEdges).ToArray();
                foreach (var deleteEdge in myDeleteEdges)
                    neighbor.RemoveEdge(deleteEdge);
            }

            foreach (var deletedEdge in allDeletedEdges)
            {
                deletedEdge.FirstNode = null;
                deletedEdge.SecondNode = null;
                graph.RemoveEdge(deletedEdge);
                Destroy(deletedEdge.gameObject);
            }

            NodeSelector.Instance.Remove(node.Id);
            graph.RemoveNode(node);

            Destroy(node.gameObject);

            return true;
        }

        public void DeleteSelectedNodes()
        {
            var nodeDeletedNodeInfos = NodeSelector.Instance.SelectedNodes.Select(node => node.GetNodeInfo()).ToList();

            if (!nodeDeletedNodeInfos.Any())
                return;
            
            Undo.AddActions(
                () => nodeDeletedNodeInfos.ForEach(nodeInfo => _CreateNode(nodeInfo)),
                _DeleteSelectedNodes
            );
            
            _DeleteSelectedNodes();
        }

        private void _DeleteSelectedNodes()
        {
            var selectedNodes = NodeSelector.Instance.SelectedNodes.ToArray();

            if (!selectedNodes.Any())
                return;

            var allDeletedEdges = new List<MonoEdge>();
            var allDeletedNodes = new List<MonoNode>();
            var allNeighboringNodes = new List<MonoNode>();

            foreach (var node in selectedNodes)
            {
                allDeletedNodes.Add(node);
                allNeighboringNodes.AddRange(node.GetNeighbors());
                allDeletedEdges.AddRange(node.Edges);
            }

            allDeletedEdges = allDeletedEdges.Distinct().ToList();
            allNeighboringNodes = allNeighboringNodes.Distinct().ToList();

            foreach (var node in allNeighboringNodes)
            {
                var myDeleteEdges = node.Edges.Intersect(allDeletedEdges).ToArray();
                foreach (var deleteEdge in myDeleteEdges)
                    node.RemoveEdge(deleteEdge);
            }

            foreach (var node in allDeletedNodes)
            {
                var myDeleteEdges = node.Edges.Intersect(allDeletedEdges).ToArray();
                foreach (var deleteEdge in myDeleteEdges)
                    node.RemoveEdge(deleteEdge);
            }

            foreach (var deletedEdge in allDeletedEdges)
            {
                deletedEdge.FirstNode = null;
                deletedEdge.SecondNode = null;
                graph.RemoveEdge(deletedEdge);
                Destroy(deletedEdge.gameObject);
            }

            foreach (var node in allDeletedNodes)
            {
                NodeSelector.Instance.Remove(node.Id);
                graph.RemoveNode(node);
                Destroy(node.gameObject);
            }
        }

        public void ConnectOrDisconnectNodes(MonoNode firstMonoNode, MonoNode secondMonoNode)
        {
            var intersect = GetIntersectEdges(firstMonoNode, secondMonoNode);
            if (intersect.Count == 0)
                ConnectNodes(firstMonoNode.Id, secondMonoNode.Id);
            else
                DisconnectNodes(firstMonoNode.Id, secondMonoNode.Id);
        }

        public MonoEdge ConnectNodes(int firstNodeId, int secondNodeId)
        {
            var edge = _ConnectNodes(firstNodeId, secondNodeId);

            Undo.AddActions(
                () => _DisconnectNodes(firstNodeId, secondNodeId),
                () => _ConnectNodes(firstNodeId, secondNodeId)
            );

            return edge;
        }

        private MonoEdge _ConnectNodes(int firstNodeId, int secondNodeId)
        {
            var firstMonoNode = GetNodeById(firstNodeId);
            var secondMonoNode = GetNodeById(secondNodeId);

            var edge = CreateEdge();
            edge.Initialize(GetNextIndex(edge), firstMonoNode, secondMonoNode);

            firstMonoNode.AddEdge(edge);
            secondMonoNode.AddEdge(edge);

            edge.Redraw();

            graph.AddEdge(edge);

            return edge;
        }

        public void DisconnectNodes(int firstNodeId, int secondNodeId)
        {
            _DisconnectNodes(firstNodeId, secondNodeId);

            Undo.AddActions(
                () => _ConnectNodes(firstNodeId, secondNodeId),
                () => _DisconnectNodes(firstNodeId, secondNodeId)
            );
        }

        private void _DisconnectNodes(int firstNodeId, int secondNodeId)
        {
            var firstMonoNode = GetNodeById(firstNodeId);
            var secondMonoNode = GetNodeById(secondNodeId);

            var intersect = GetIntersectEdges(firstMonoNode, secondMonoNode);

            foreach (var edge in intersect)
            {
                firstMonoNode.RemoveEdge(edge);
                secondMonoNode.RemoveEdge(edge);
                graph.RemoveEdge(edge);
                Destroy(edge.gameObject);
            }
        }

        private MonoEdge CreateEdge()
        {
            return Instantiate(monoEdgePrefab, graph.EdgesParent.transform);
        }

        private List<MonoEdge> GetIntersectEdges(MonoNode firstMonoNode, MonoNode secondMonoNode)
        {
            return firstMonoNode.Edges
                .Intersect(secondMonoNode.Edges)
                .ToList();
        }

        private int GetNextIndex<T>(T obj) where T : Object, INumbered
        {
            var objects = FindObjectsOfType<T>()
                .Where(x => x != obj)
                .OrderBy(x => x.Id);
            var nextIndex = 0;
            foreach (var o in objects)
            {
                if (o.Id == nextIndex)
                    nextIndex++;
                else
                    return nextIndex;
            }

            return nextIndex;
        }

        private MonoNode GetNodeById(int nodeId)
        {
            var node = graph.IdToNode[nodeId];

            if (node == null)
                throw new ArgumentNullException();

            return node;
        }
    }
}