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
        where TGraph : MonoBehaviour, IMonoGraph
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

        private Dictionary<int, TNode> monoNodes;
        private Dictionary<int, TEdge> monoEdges;
        
        private IObjectCreator creator;

        public void Initialize(IObjectCreator creator)
        {
            this.creator = creator;
        }
        
        
        public void Restart()
        {
            var graph = GenerateConnectiveGraph();
            CreateInstanceOfGraph(graph);
        }

        public void Iterate()
        {
            var nodeAndForce = new Dictionary<int, Vector2>();
            foreach (var monoNode in monoNodes.Values)
                nodeAndForce[monoNode.Id] = Vector2.zero;

            foreach (var monoNode in monoNodes.Values)
            {
                var nodePosition = monoNode.transform.position;
                Vector2 wallForce = new Vector2(1 / (multiplierOfRepulsion * Mathf.Pow(nodePosition.x, powerOfRepulsion)), 0);
                wallForce += new Vector2(-1 / (multiplierOfRepulsion * Mathf.Pow(areaSize.x - nodePosition.x, powerOfRepulsion)), 0);
                wallForce += new Vector2(0, 1 / (multiplierOfRepulsion * Mathf.Pow(nodePosition.y, powerOfRepulsion)));
                wallForce += new Vector2(0, -1 / (multiplierOfRepulsion * Mathf.Pow(areaSize.y - nodePosition.y, powerOfRepulsion)));

                Vector2 nodesForce = Vector2.zero;
                foreach (var neighbor in monoNodes.Values.Where(x => x.Id != monoNode.Id))
                {
                    Vector2 force = monoNode.transform.position - neighbor.transform.position;
                    nodesForce += 1 / (multiplierOfRepulsion * Mathf.Pow(force.magnitude, powerOfRepulsion)) * force.normalized;
                }

                nodeAndForce[monoNode.Id] += wallForce + nodesForce;
            }

            foreach (var monoEdge in monoEdges.Values)
            {
                Vector2 force = monoEdge.SecondNode.transform.position - monoEdge.FirstNode.transform.position;
                var node1Node2Force = multiplierOfConnection * Mathf.Pow(force.magnitude, powerOfConnection) * force.normalized;

                nodeAndForce[monoEdge.FirstNode.Id] += node1Node2Force;
                nodeAndForce[monoEdge.SecondNode.Id] -= node1Node2Force;
            }

            foreach (var (nodeId, force) in nodeAndForce)
            {
                monoNodes[nodeId].transform.position += (Vector3) force;
            }

            AssignNodes();
        }

        public void DeleteIntersectingEdges()
        {
            foreach (var monoEdge1 in monoEdges.Values.ToArray())
            {
                foreach (var monoEdge2 in monoEdges.Values.ToArray())
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
                        .ThenBy(x => (x.FirstNode.transform.position - x.SecondNode.transform.position).magnitude);
                    
                    var edgeToDelete = ordered.Last();
                    edgeToDelete.FirstNode.RemoveEdge(edgeToDelete);
                    edgeToDelete.SecondNode.RemoveEdge(edgeToDelete);
                    
                    if (edgeToDelete.FirstNode.EdgesCount == 0)
                        DestroyNode(edgeToDelete.FirstNode.Id);
                    if (edgeToDelete.SecondNode.EdgesCount == 0)
                        DestroyNode(edgeToDelete.SecondNode.Id);
                    DestroyEdge(edgeToDelete.Id);
                }
            }
        }

        private void DestroyNode(int nodeId)
        {
            creator.ReleaseInstance(monoNodes[nodeId].gameObject);
            monoNodes.Remove(nodeId);
        }

        private void DestroyEdge(int edgeId)
        {
            creator.ReleaseInstance(monoEdges[edgeId].gameObject);
            monoEdges.Remove(edgeId);
        }

        private void AssignNodes()
        {
            foreach (var node in monoNodes.Values)
            {
                node.transform.position = new Vector2(
                    Mathf.Clamp(node.transform.position.x, paddings, areaSize.x - paddings),
                    Mathf.Clamp(node.transform.position.y, paddings, areaSize.y - paddings)
                );
            }
        }

        public void CreateInstanceOfGraph(UndirectedVertexGraph undirectedGraph)
        {
            monoNodes = new Dictionary<int, TNode>();
            monoEdges = new Dictionary<int, TEdge>();

            monoGraph = creator.CreateInstance(graphPrefab);
            foreach (var node in undirectedGraph.Nodes)
            {
                var monoNode = creator.CreateInstance(monoNodePrefab, monoGraph.NodesParent.transform);
                monoNode.transform.position = GetRandomPositionInArea();
                monoNode.Initialize(node.Vertex);
                monoNodes[node.Vertex] = monoNode;
            }

            var edgeIndex = 0;
            foreach (var (node1, node2) in undirectedGraph.Edges)
            {
                var monoEdge = creator.CreateInstance(monoEdgePrefab, monoGraph.EdgesParent.transform);
                monoEdge.Initialize(edgeIndex, monoNodes[node1.Vertex], monoNodes[node2.Vertex]);
                monoNodes[node1.Vertex].AddEdge(monoEdge);
                monoNodes[node2.Vertex].AddEdge(monoEdge);
                monoEdges[edgeIndex] = monoEdge;
                edgeIndex++;
            }
        }

        public void RedrawAllEdges()
        {
            foreach (var monoEdge in monoEdges.Values)
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

            Debug.LogError("Cant generate connective graph");
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
    }
}