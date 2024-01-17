// ReSharper disable Unity.InefficientPropertyAccess

using System;
using System.Collections.Generic;
using System.Linq;
using GraphEditor.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;


namespace GraphEditor
{
    public class GraphCreator : MonoBehaviour
    {
        [Header("Prefabs")] 
        [SerializeField] private MonoGraph graphPrefab;
        [SerializeField] private MonoNode monoNodePrefab;
        [SerializeField] private MonoEdge monoEdgePrefab;

        [Header("Points settings")]
        [SerializeField] private Vector2 areaSize;

        [SerializeField, Min(0)] private float paddings = 0.3f;
        [SerializeField] private float powerOfConnection = 1;
        [SerializeField, Range(0, 0.01f)] private float multiplierOfConnection = 0.006f;
        [SerializeField] private float powerOfRepulsion = 2;
        [SerializeField, Range(0, 1)] private float multiplierOfRepulsion = 1;

        [Header("Graph settings")] 
        [SerializeField] private int nodesCount;
        [SerializeField] private Vector2Int edgesRange;

        private MonoGraph monoGraph;
        private UndirectedVertexGraph vertexGraph;
        private IObjectCreatorAndDestroyer creator;

        public void Initialize(IObjectCreatorAndDestroyer creatorAndDestroyer)
        {
            creator = creatorAndDestroyer;
        }
        
        public void Restart()
        {
            var graph = GenerateConnectiveGraph();
            CreateInstanceOfGraph(graph);
        }

        public void Iterate()
        {
            var nodeAndForce = new Dictionary<int, Vector2>();
            foreach (var monoNode in monoGraph.Nodes)
                nodeAndForce[monoNode.Id] = Vector2.zero;

            foreach (var monoNode in  monoGraph.Nodes)
            {
                var nodePosition = monoNode.transform.position;
                Vector2 wallForce = new Vector2(multiplierOfRepulsion / Mathf.Pow(nodePosition.x, powerOfRepulsion), 0);
                wallForce += new Vector2(-multiplierOfRepulsion / Mathf.Pow(areaSize.x - nodePosition.x, powerOfRepulsion), 0);
                wallForce += new Vector2(0, multiplierOfRepulsion / Mathf.Pow(nodePosition.y, powerOfRepulsion));
                wallForce += new Vector2(0, -multiplierOfRepulsion / Mathf.Pow(areaSize.y - nodePosition.y, powerOfRepulsion));

                Vector2 nodesForce = Vector2.zero;
                foreach (var neighbor in monoGraph.Nodes.Where(x => x.Id != monoNode.Id))
                {
                    Vector2 force = monoNode.transform.position - neighbor.transform.position;
                    nodesForce += multiplierOfRepulsion / Mathf.Pow(force.magnitude, powerOfRepulsion) * force.normalized;
                }

                nodeAndForce[monoNode.Id] += wallForce + nodesForce;
            }

            foreach (var monoEdge in monoGraph.Edges)
            {
                Vector2 force = monoEdge.SecondNode.transform.position - monoEdge.FirstNode.transform.position;
                var node1Node2Force = multiplierOfConnection * Mathf.Pow(force.magnitude, powerOfConnection) * force.normalized;

                nodeAndForce[monoEdge.FirstNode.Id] += node1Node2Force;
                nodeAndForce[monoEdge.SecondNode.Id] -= node1Node2Force;
            }

            foreach (var pair in nodeAndForce)
            {
                monoGraph.IdToNode[pair.Key].transform.position += (Vector3) pair.Value;
            }

            AssignNodes();
        }
        

        public void DeleteIntersectingEdgesByLength()
        {
            foreach (var monoEdge1 in monoGraph.Edges.ToArray())
            {
                foreach (var monoEdge2 in monoGraph.Edges.ToArray())
                {
                    if (monoEdge1 == null || monoEdge2 == null)
                    {
                        continue;
                    }
                    if (monoEdge1.FirstNode == monoEdge2.FirstNode 
                        || monoEdge1.FirstNode == monoEdge2.SecondNode 
                        || monoEdge1.SecondNode == monoEdge2.FirstNode
                        || monoEdge1.SecondNode== monoEdge2.SecondNode)
                    {
                        continue;
                    }
                    if (!CustomMath.SegmentsIsIntersects(
                            monoEdge1.FirstNode.Position, monoEdge1.SecondNode.Position,
                            monoEdge2.FirstNode.Position, monoEdge2.SecondNode.Position))
                    {
                        continue;
                    }
                    
                    var ordered = new[] {monoEdge1, monoEdge2}
                        .OrderBy(x => (x.FirstNode.Edges.Count() == 1 || x.SecondNode.Edges.Count() == 1) ? 0 : 1)
                        .ThenBy(x => (x.FirstNode.transform.position - x.SecondNode.transform.position).magnitude)
                        .ToArray();
                    
                    var edgeToDelete = ordered[1];
                    
                    vertexGraph.DisconnectNodes(edgeToDelete.FirstNode.Id, edgeToDelete.SecondNode.Id);
                    if (!vertexGraph.IsConnectedGraph())
                    {
                        vertexGraph.Undo();
                        edgeToDelete = ordered[0];
                    }
                    
                    vertexGraph.DisconnectNodes(edgeToDelete.FirstNode.Id, edgeToDelete.SecondNode.Id);
                    if (!vertexGraph.IsConnectedGraph())
                    {
                        vertexGraph.Undo();
                        continue;
                    }
                    
                    edgeToDelete.FirstNode.RemoveEdge(edgeToDelete);
                    edgeToDelete.SecondNode.RemoveEdge(edgeToDelete);
                    DestroyEdge(edgeToDelete.Id);
                    
                    if (edgeToDelete.FirstNode.EdgesCount == 0)
                        DestroyNode(edgeToDelete.FirstNode.Id);
                    if (edgeToDelete.SecondNode.EdgesCount == 0)
                        DestroyNode(edgeToDelete.SecondNode.Id);
                }
            }
        }
        
        /// <summary>
        /// В приоритете удаляются те ребра у которых больше всего пересечений
        /// </summary>
        public void DeleteIntersectingEdgesByIntersectionsCount()
        {
            var edgeAndIntersections = new Dictionary<MonoEdge, List<MonoEdge>>();
            
            foreach (var monoEdge1 in monoGraph.Edges.ToArray())
            {
                foreach (var monoEdge2 in monoGraph.Edges.ToArray())
                {
                    if (monoEdge1.FirstNode == monoEdge2.FirstNode
                        || monoEdge1.FirstNode == monoEdge2.SecondNode
                        || monoEdge1.SecondNode == monoEdge2.FirstNode
                        || monoEdge1.SecondNode == monoEdge2.SecondNode)
                    {
                        continue;
                    }
                    
                    if (!CustomMath.SegmentsIsIntersects(
                            monoEdge1.FirstNode.Position, monoEdge1.SecondNode.Position,
                            monoEdge2.FirstNode.Position, monoEdge2.SecondNode.Position))
                    {
                        continue;
                    }

                    if (!edgeAndIntersections.ContainsKey(monoEdge1))
                        edgeAndIntersections[monoEdge1] = new List<MonoEdge>();
                    edgeAndIntersections[monoEdge1].Add(monoEdge2);
                }
            }

            var deletedEdges = new List<MonoEdge>();
            while (edgeAndIntersections.Count != 0)
            {
                var currentPair = edgeAndIntersections
                    .OrderByDescending(x => x.Value.Count)
                    .First();
                var edgeToDelete = currentPair.Key;
                vertexGraph.DisconnectNodes(edgeToDelete.FirstNode.Id, edgeToDelete.SecondNode.Id);
                edgeAndIntersections.Remove(edgeToDelete);
                
                if (vertexGraph.IsConnectedGraph())
                {
                    deletedEdges.Add(edgeToDelete);
                    foreach (var intersectionEdge in currentPair.Value)
                    {
                        if (edgeAndIntersections.TryGetValue(intersectionEdge, out var otherIntersections))
                        {
                            otherIntersections.Remove(edgeToDelete);
                        }
                    }
                }
                else
                {
                    vertexGraph.Undo();
                }
            }

            foreach (var deletedEdge in deletedEdges)
            {
                deletedEdge.FirstNode.RemoveEdge(deletedEdge);
                deletedEdge.SecondNode.RemoveEdge(deletedEdge);
                DestroyEdge(deletedEdge.Id);
            }
        }
        
        private void AssignNodes()
        {
            foreach (var node in monoGraph.Nodes)
            {
                node.transform.position = new Vector2(
                    Mathf.Clamp(node.transform.position.x, paddings, areaSize.x - paddings),
                    Mathf.Clamp(node.transform.position.y, paddings, areaSize.y - paddings)
                );
            }
        }

        private void CreateInstanceOfGraph(UndirectedVertexGraph undirectedGraph)
        {
            vertexGraph = undirectedGraph;
            monoGraph = creator.CreateInstance(graphPrefab);
            foreach (var node in undirectedGraph.Nodes)
            {
                var monoNode = creator.CreateInstance(monoNodePrefab, monoGraph.NodesParent.transform);
                monoNode.transform.position = GetRandomPositionInArea();
                monoNode.Initialize(node.Value);
                monoGraph.IdToNode[node.Value] = monoNode;
            }

            var edgeIndex = 0;
            foreach (var (node1, node2) in undirectedGraph.Edges)
            {
                var monoEdge = creator.CreateInstance(monoEdgePrefab, monoGraph.EdgesParent.transform);
                monoEdge.Initialize(edgeIndex, monoGraph.IdToNode[node1.Value], monoGraph.IdToNode[node2.Value]);
                monoGraph.IdToNode[node1.Value].AddEdge(monoEdge);
                monoGraph.IdToNode[node2.Value].AddEdge(monoEdge);
                monoGraph.IdToEdge[edgeIndex] = monoEdge;
                edgeIndex++;
            }
        }

        public void RedrawAllEdges()
        {
            foreach (var monoEdge in monoGraph.Edges)
                monoEdge.Redraw();
        }

        private UndirectedVertexGraph GenerateConnectiveGraph()
        {
            for (var i = 0; i < 1000; i++)
            {
                var graph = UndirectedVertexGraph.GenerateRandomGraph(nodesCount, (edgesRange.x, edgesRange.y));
                if (graph.IsConnectedGraph())
                    return graph;
            }

            EditorDebug.LogError("Cant generate connective graph");
            return null;
        }

        private Vector2 GetRandomPositionInArea()
        {
            return new Vector2(
                Random.Range(paddings, areaSize.x - paddings),
                Random.Range(paddings, areaSize.y - paddings)
            );
        }

        public void DrawBorder()
        {
            var lineRenderer = new GameObject("LineRenderer").AddComponent<LineRenderer>();
            lineRenderer.transform.SetParent(monoGraph.transform);

            lineRenderer.loop = true;
            lineRenderer.positionCount = 4;
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, new Vector3(areaSize.x, 0, 0));
            lineRenderer.SetPosition(2, new Vector3(areaSize.x, areaSize.y, 0));
            lineRenderer.SetPosition(3, new Vector3(0, areaSize.y, 0));
        }
        
        private void DestroyNode(int nodeId)
        {
            creator.ReleaseInstance(monoGraph.IdToNode[nodeId].gameObject);
            if (monoGraph.ContainsNode(nodeId))
                monoGraph.RemoveNode(nodeId);
        }

        private void DestroyEdge(int edgeId)
        {
            creator.ReleaseInstance(monoGraph.IdToEdge[edgeId].gameObject);
            if (monoGraph.ContainsEdge(edgeId))
                monoGraph.RemoveEdge(edgeId);
        }
    }
}