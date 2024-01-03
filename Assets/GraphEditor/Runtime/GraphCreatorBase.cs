// ReSharper disable Unity.InefficientPropertyAccess

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


namespace GraphEditor
{
    public abstract class GraphCreatorBase<TNode, TEdge, TGraph> : MonoBehaviour
        where TNode : MonoBehaviour, IMonoNode<TNode, TEdge>
        where TEdge : MonoBehaviour, IMonoEdge<TNode, TEdge>
        where TGraph : MonoBehaviour, IMonoGraph<TNode, TEdge>
    {
        [Header("Prefabs")] 
        [SerializeField] private TGraph graphPrefab;
        [SerializeField] private TNode monoNodePrefab;
        [SerializeField] private TEdge monoEdgePrefab;

        [Header("Points settings")]
        [SerializeField] private Vector2 areaSize;

        [SerializeField, Min(0)] private float paddings = 0.3f;
        [SerializeField] private float powerOfConnection;
        [SerializeField, Range(0, 1)] private float multiplierOfConnection;
        [SerializeField] private float powerOfRepulsion;
        [SerializeField, Range(0, 1)] private float multiplierOfRepulsion;

        [Header("Graph settings")] 
        [SerializeField] private int nodesCount;
        [SerializeField] private Vector2Int edgesRange;

        private TGraph monoGraph;
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
                Vector2 wallForce = new Vector2(1 / (multiplierOfRepulsion * Mathf.Pow(nodePosition.x, powerOfRepulsion)), 0);
                wallForce += new Vector2(-1 / (multiplierOfRepulsion * Mathf.Pow(areaSize.x - nodePosition.x, powerOfRepulsion)), 0);
                wallForce += new Vector2(0, 1 / (multiplierOfRepulsion * Mathf.Pow(nodePosition.y, powerOfRepulsion)));
                wallForce += new Vector2(0, -1 / (multiplierOfRepulsion * Mathf.Pow(areaSize.y - nodePosition.y, powerOfRepulsion)));

                Vector2 nodesForce = Vector2.zero;
                foreach (var neighbor in monoGraph.Nodes.Where(x => x.Id != monoNode.Id))
                {
                    Vector2 force = monoNode.transform.position - neighbor.transform.position;
                    nodesForce += 1 / (multiplierOfRepulsion * Mathf.Pow(force.magnitude, powerOfRepulsion)) * force.normalized;
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

            foreach (var (nodeId, force) in nodeAndForce)
            {
                monoGraph.IdToNode[nodeId].transform.position += (Vector3) force;
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
            var edgeAndIntersections = new Dictionary<TEdge, List<TEdge>>();
            
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
                        edgeAndIntersections[monoEdge1] = new List<TEdge>();
                    edgeAndIntersections[monoEdge1].Add(monoEdge2);
                }
            }

            var deletedEdges = new List<TEdge>();
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
                monoNode.Initialize(node.Vertex);
                monoGraph.IdToNode[node.Vertex] = monoNode;
            }

            var edgeIndex = 0;
            foreach (var (node1, node2) in undirectedGraph.Edges)
            {
                var monoEdge = creator.CreateInstance(monoEdgePrefab, monoGraph.EdgesParent.transform);
                monoEdge.Initialize(edgeIndex, monoGraph.IdToNode[node1.Vertex], monoGraph.IdToNode[node2.Vertex]);
                monoGraph.IdToNode[node1.Vertex].AddEdge(monoEdge);
                monoGraph.IdToNode[node2.Vertex].AddEdge(monoEdge);
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
            monoGraph.IdToNode.Remove(nodeId);
        }

        private void DestroyEdge(int edgeId)
        {
            creator.ReleaseInstance(monoGraph.IdToEdge[edgeId].gameObject);
            monoGraph.IdToEdge.Remove(edgeId);
        }
        
        
//         private TObj InstantiateObj<TObj>(TObj obj, Transform parent) where TObj: Object
//         {
// #if UNITY_EDITOR
//             if (Application.isEditor)
//                 return (TObj) UnityEditor.PrefabUtility.InstantiatePrefab(obj, parent);
//             else
//                 return Instantiate(obj, parent);
// #else
//             return Instantiate(obj, parent);
// #endif
//         }
//         
//         private void DestroyNode(int nodeId)
//         {
// #if UNITY_EDITOR
//             if (Application.isEditor)
//                 UnityEditor.Undo.DestroyObjectImmediate(monoGraph.IdToNode[nodeId].gameObject);
//             else
//                 Destroy(monoGraph.IdToNode[nodeId].gameObject);
// #else
//             Destroy(monoGraph.IdToNode[nodeId].gameObject);
// #endif
//             monoGraph.IdToNode.Remove(nodeId);
//         }
//
//         private void DestroyEdge(int edgeId)
//         {
// #if UNITY_EDITOR
//             if (Application.isEditor)
//                 UnityEditor.Undo.DestroyObjectImmediate(monoGraph.IdToEdge[edgeId].gameObject);
//             else
//                 Destroy(monoGraph.IdToEdge[edgeId].gameObject);
// #else
//             Destroy(monoGraph.IdToEdge[edgeId].gameObject);
// #endif
//             monoGraph.IdToEdge.Remove(edgeId);
//         }
    }
}