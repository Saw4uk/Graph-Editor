// ReSharper disable Unity.InefficientPropertyAccess

using System.Collections.Generic;
using System.Linq;
using GraphEditor.Runtime;
using UnityEngine;
using static System.Single;
using Random = UnityEngine.Random;


namespace GraphEditor
{
    public class LineWarsGameGraphCreator : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private MonoGraph graphPrefab;
        [SerializeField] private MonoNode monoNodePrefab;
        [SerializeField] private MonoEdge monoEdgePrefab;

        [Header("Points settings")]
        [SerializeField] private Vector2 areaSize = new Vector2(40, 20);

        [SerializeField, Min(0)] private float paddings = 0.3f;
        [SerializeField] private float powerOfConnection = 1;
        [SerializeField, Range(0, 0.1f)] private float multiplierOfConnection = 0.006f;
        [SerializeField] private float powerOfRepulsion = 2;
        [SerializeField, Range(0, 100)] private float multiplierOfRepulsion = 1;

        [Header("Graph settings")]
        [SerializeField] private int nodesCount = 10;
        [SerializeField] private Vector2Int edgesRange = new Vector2Int(2, 4);

        [Header("Debug")]
        [SerializeField] private bool log = true;

        private MonoGraph monoGraph;
        private UndirectedVertexGraph vertexGraph;

        public void LoadSettings(LineWarsGameGraphCreatorSettings settings)
        {
            areaSize = settings.AreaSize;
            paddings = settings.Paddings;
            powerOfConnection = settings.PowerOfConnection;
            multiplierOfConnection = settings.MultiplierOfConnection;
            powerOfRepulsion = settings.PowerOfRepulsion;
            multiplierOfRepulsion = settings.MultiplierOfRepulsion;
            nodesCount = settings.NodesCount;
            edgesRange = settings.EdgesRange;
        }

        public MonoGraph Restart()
        {
            var undirectedVertexGraph = GenerateConnectiveGraph();
            return CreateInstanceOfGraph(undirectedVertexGraph);
        }
        
        public MonoGraph Restart(UndirectedVertexGraph undirectedVertexGraph)
        {
            return CreateInstanceOfGraph(undirectedVertexGraph);
        }

        public void Iterate()
        {
            Iterate(new[] { GetWallRepulsion(), GetNodesRepulsion(), GetConnectionsPower() });
        }

        public void Iterate(IEnumerable<IEnumerable<(int, Vector2)>> powers)
        {
            var nodeAndForce = new Dictionary<int, Vector2>();
            foreach (var node in monoGraph.Nodes)
                nodeAndForce[node.Id] = Vector2.zero;

            foreach (var enumerablePower in powers)
                foreach (var (id, force) in enumerablePower)
                    nodeAndForce[id] += AssignForce(force);

            foreach (var pair in nodeAndForce)
            {
                var nodeId = pair.Key;
                var force = pair.Value;
                monoGraph.IdToNode[nodeId].transform.position += (Vector3)AssignForce(force);
            }

            AssignNodes();
        }

        public IEnumerable<(int, Vector2)> GetWallRepulsion()
        {
            foreach (var monoNode in monoGraph.Nodes)
            {
                var nodePosition = monoNode.transform.position;
                Vector2 wallForce = new Vector2(multiplierOfRepulsion / Mathf.Pow(nodePosition.x, powerOfRepulsion), 0);
                wallForce += new Vector2(-multiplierOfRepulsion / Mathf.Pow(areaSize.x - nodePosition.x, powerOfRepulsion), 0);
                wallForce += new Vector2(0, multiplierOfRepulsion / Mathf.Pow(nodePosition.y, powerOfRepulsion));
                wallForce += new Vector2(0, -multiplierOfRepulsion / Mathf.Pow(areaSize.y - nodePosition.y, powerOfRepulsion));
                yield return (monoNode.Id, wallForce);
            }
        }

        public IEnumerable<(int, Vector2)> GetNodesRepulsion()
        {
            foreach (var monoNode in monoGraph.Nodes)
            {
                var nodesForce = Vector2.zero;
                foreach (var neighbor in monoGraph.Nodes.Where(x => x.Id != monoNode.Id))
                {
                    Vector2 force = monoNode.transform.position - neighbor.transform.position;
                    nodesForce += GetRepulsionForce(force);
                }
                yield return (monoNode.Id, nodesForce);
            }
        }

        public IEnumerable<(int, Vector2)> GetConnectionsPower()
        {
            foreach (var monoEdge in monoGraph.Edges)
            {
                Vector2 force1 = monoEdge.SecondNode.transform.position - monoEdge.FirstNode.transform.position;
                var force2 = GetConnectionForce(force1);

                yield return (monoEdge.FirstNode.Id, force2);
                yield return (monoEdge.SecondNode.Id, -force2);
            }
        }

        public IEnumerable<(int, Vector2)> GetEdgesRepulsion()
        {
            foreach (var node in monoGraph.Nodes)
            {
                foreach (var edge in monoGraph.Edges)
                {
                    if (edge.FirstNode == node || edge.SecondNode == node)
                        continue;
                    var center = (edge.FirstNode.Position + edge.SecondNode.Position) / 2;
                    var force1 = node.Position - center;
                    var force2 = GetRepulsionForce(force1);
                    yield return (node.Id, force2);
                    yield return (edge.FirstNode.Id, -force2 / 2);
                    yield return (edge.SecondNode.Id, -force2 / 2);
                }
            }
        }

        public IEnumerable<(int, Vector2)> GetHardEdgesRepulsion()
        {
            foreach (var node in monoGraph.Nodes)
            {
                foreach (var edge in monoGraph.Edges)
                {
                    if (edge.FirstNode == node || edge.SecondNode == node)
                        continue;

                    var force1 = CustomMath.GetMinVectorFromSegmentToPoint(
                        edge.FirstNode.Position,
                        edge.SecondNode.Position,
                        node.Position);

                    if (force1.magnitude == 0
                        || force1.magnitude is NaN
                        || IsPositiveInfinity(force1.magnitude)
                        || IsNegativeInfinity(force1.magnitude))
                        yield break;

                    var force2 = GetRepulsionForce(force1);

                    yield return (node.Id, force2);
                    yield return (edge.FirstNode.Id, -force2 / 2);
                    yield return (edge.SecondNode.Id, -force2 / 2);
                }
            }
        }

        private Vector2 GetRepulsionForce(Vector2 force)
        {
            return multiplierOfRepulsion / Mathf.Pow(force.magnitude, powerOfRepulsion) * force.normalized;
        }

        private Vector2 GetConnectionForce(Vector2 force1)
        {
            return multiplierOfConnection * Mathf.Pow(force1.magnitude, powerOfConnection) * force1.normalized;
        }

        private Vector2 AssignForce(Vector2 force)
        {
            if (force.x is NaN || IsPositiveInfinity(force.x) || IsNegativeInfinity(force.x))
                force.x = 0;
            if (force.y is NaN || IsPositiveInfinity(force.y) || IsNegativeInfinity(force.y))
                force.y = 0;
            return force;
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

                    var ordered = new[] { monoEdge1, monoEdge2 }
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

            foreach (var (monoEdge1, monoEdge2) in monoGraph.GetIntersectionsEdges())
            {
                if (!edgeAndIntersections.ContainsKey(monoEdge1))
                    edgeAndIntersections[monoEdge1] = new List<MonoEdge>();
                edgeAndIntersections[monoEdge1].Add(monoEdge2);
            }

            var deletedEdges = new List<MonoEdge>();
            var idToNode = vertexGraph.AsReadOnlyNodesDictionary;
            while (edgeAndIntersections.Count != 0)
            {
                var currentPair = edgeAndIntersections
                    .OrderByDescending(x => x.Value.Count)
                    .First();
                var edgeToDelete = currentPair.Key;
                if (currentPair.Value.Count == 0)
                {
                    edgeAndIntersections.Remove(edgeToDelete);
                    continue;
                }
                vertexGraph.DisconnectNodes(edgeToDelete.FirstNode.Id, edgeToDelete.SecondNode.Id);
                edgeAndIntersections.Remove(edgeToDelete);


                if (vertexGraph.IsConnectedGraph()
                    && idToNode[edgeToDelete.FirstNode.Id].NeighboursVertex.Count != 1
                    && idToNode[edgeToDelete.SecondNode.Id].NeighboursVertex.Count != 1)
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

        public int GetIntersectionsCount()
        {
            return monoGraph.GetIntersectionsEdges().Count() / 2;
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

        private MonoGraph CreateInstanceOfGraph(UndirectedVertexGraph undirectedGraph)
        {
            vertexGraph = undirectedGraph;
            monoGraph = Instantiate(graphPrefab);
            monoGraph.Initialize();
            foreach (var node in undirectedGraph.Nodes)
            {
                var monoNode = Instantiate(monoNodePrefab, monoGraph.NodesParent.transform);
                monoNode.transform.position = GetRandomPositionInArea();
                monoNode.Initialize(node.Value);
                monoGraph.IdToNode[node.Value] = monoNode;
            }

            var edgeIndex = 0;
            foreach (var (node1, node2) in undirectedGraph.Edges)
            {
                var monoEdge = Instantiate(monoEdgePrefab, monoGraph.EdgesParent.transform);
                monoEdge.Initialize(edgeIndex, monoGraph.IdToNode[node1.Value], monoGraph.IdToNode[node2.Value]);
                monoGraph.IdToNode[node1.Value].AddEdge(monoEdge);
                monoGraph.IdToNode[node2.Value].AddEdge(monoEdge);
                monoGraph.IdToEdge[edgeIndex] = monoEdge;
                edgeIndex++;
            }

            return monoGraph;
        }

        public void RedrawAllEdges()
        {
            monoGraph.RedrawAllEdges();
        }

        private UndirectedVertexGraph GenerateConnectiveGraph()
        {
            for (var i = 0; i < 1000; i++)
            {
                var graph = UndirectedVertexGraph.GenerateRandomGraph(nodesCount, (edgesRange.x, edgesRange.y));
                if (graph.IsConnectedGraph())
                {
                    if (log)
                    {
                        Debug.Log($"UndirectedVertexGraph был успешно создан с {i + 1} попытки");
                    }
                    return graph;
                }
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
            if (Application.isEditor)
                DestroyImmediate(monoGraph.IdToNode[nodeId].gameObject);
            else
                Destroy(monoGraph.IdToNode[nodeId].gameObject);
            monoGraph.RemoveNode(nodeId);
        }

        private void DestroyEdge(int edgeId)
        {
            if (Application.isEditor)
                DestroyImmediate(monoGraph.IdToEdge[edgeId].gameObject);
            else
                Destroy(monoGraph.IdToEdge[edgeId].gameObject);
            monoGraph.RemoveEdge(edgeId);
        }
    }
}