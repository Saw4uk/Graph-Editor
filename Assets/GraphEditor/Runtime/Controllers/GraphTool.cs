using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GraphEditor.Runtime
{
    public class GraphTool : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private MonoNode monoNodePrefab;
        [SerializeField] private MonoEdge monoEdgePrefab;

        private MonoGraph graph;
        private NodeSelector selector;

        public MonoGraph Graph
        {
            get => graph;
            set => graph = value;
        }

        public bool isEditable { get; set; }

        private void Update()
        {
            if (!isEditable) return;
            
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

        public void Initialize(MonoGraph monoGraph, NodeSelector selector)
        {
            graph = monoGraph;
            this.selector = selector;
        }

        public MonoNode CreateNode(Vector2 position)
        {
            if (!isEditable) return null;
            
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

            selector.Add(node.Id);

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
            if (!isEditable) return false;
            
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

            selector.Remove(node.Id);
            graph.RemoveNode(node);

            Destroy(node.gameObject);

            return true;
        }

        public void DeleteSelectedNodes()
        {
            if (!isEditable) return;
            
            var nodeDeletedNodeInfos = selector.SelectedNodes.Select(node => node.GetNodeInfo()).ToList();

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
            var selectedNodes = selector.SelectedNodes.ToArray();

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
                selector.Remove(node.Id);
                graph.RemoveNode(node);
                Destroy(node.gameObject);
            }
        }

        public void ConnectOrDisconnectNodes(MonoNode firstMonoNode, MonoNode secondMonoNode)
        {
            if (!isEditable) return;
            
            var intersect = GetIntersectEdges(firstMonoNode, secondMonoNode);
            if (intersect.Count == 0)
                ConnectNodes(firstMonoNode.Id, secondMonoNode.Id);
            else
                DisconnectNodes(firstMonoNode.Id, secondMonoNode.Id);
        }

        public MonoEdge ConnectNodes(int firstNodeId, int secondNodeId)
        {
            if (!isEditable) return null;
            
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
            if (!isEditable) return;
            
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
        
        public void MoveSelectedNodes(Vector3 deltaPosition)
        {
            if (!isEditable) return;
            
            foreach (var node in selector.SelectedNodes)
            {
                if (node == null)
                    throw new ArgumentNullException();

                node.transform.position += new Vector3(deltaPosition.x, deltaPosition.y);
            }
            
            graph.RedrawAllEdges();
            selector.RedrawBordersSelectedObjects();
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