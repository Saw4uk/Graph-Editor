using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
// ReSharper disable Unity.InefficientPropertyAccess

namespace GraphEditor
{
    public class GraphCreator : MonoBehaviour
    {
        [Header("CreatorSettings")] 
        [SerializeField] private bool autoInitialize;
        [SerializeField] private int numberOfIterations = 1000;

        [Header("Prefabs")]
        [SerializeField] private MonoGraph graphPrefab;
        [SerializeField] private MonoNode monoNodePrefab;
        [SerializeField] private MonoEdge monoEdgePrefab;

        [Header("Points settings")] 
        [SerializeField] private Vector2 areaSize;
        [SerializeField, Min(0)]private float paddings = 0.3f;
        [SerializeField] private float powerOfConnection;
        [SerializeField, Range(0, 1)] private float multiplierOfConnection;
        [SerializeField] private float powerOfRepulsion;
        [SerializeField, Range(0, 1)] private float multiplierOfRepulsion;

        [Header("Graph settings")] 
        [SerializeField] private int nodesCount;
        [SerializeField] private Vector2Int edgesRange;

        private UndirectedVertexGraph undirectedGraph;
        private MonoGraph monoGraph;

        private Dictionary<int, MonoNode> monoNodes;
        private Dictionary<int, MonoEdge> monoEdges;


        private void Awake()
        {
            if (autoInitialize)
            {
                Restart();
                for (int i = 0; i < numberOfIterations; i++)
                    Iterate();
                RedrawAllEdges();
            }
        }

        public void Restart()
        {
            GenerateConnectiveGraph();
            CreateInstanceOfGraph();
            DrawLine();
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

        private void CreateInstanceOfGraph()
        {
            monoNodes = new Dictionary<int, MonoNode>();
            monoEdges = new Dictionary<int, MonoEdge>();

            monoGraph = Instantiate(graphPrefab);
            foreach (var node in undirectedGraph.Nodes)
            {
                var monoNode = Instantiate(monoNodePrefab, monoGraph.NodesParent.transform);
                monoNode.transform.position = GetRandomPositionInArea();
                monoNode.Initialize(node.Vertex);
                monoNodes[node.Vertex] = monoNode;
            }

            var edgeIndex = 0;
            foreach (var (node1, node2) in undirectedGraph.Edges)
            {
                var monoEdge = Instantiate(monoEdgePrefab, monoGraph.EdgesParent.transform);
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

        private void GenerateConnectiveGraph()
        {
            for (var i = 0; i < 1000; i++)
            {
                undirectedGraph = UndirectedVertexGraph.GenerateRandomGraph(nodesCount, (edgesRange.x, edgesRange.y));
                if (undirectedGraph.IsConnectedGraph())
                    return;
            }

            Debug.LogError("Cant generate connective graph");
        }

        private Vector2 GetRandomPositionInArea()
        {
            return new Vector2(Random.Range(paddings, areaSize.x - paddings),
                Random.Range(paddings, areaSize.y - paddings));
        }

        private void DrawLine()
        {
            var lineRenderer = new GameObject("LineRenderer").AddComponent<LineRenderer>();
            lineRenderer.sharedMaterial = new Material(Shader.Find("Standart"));
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