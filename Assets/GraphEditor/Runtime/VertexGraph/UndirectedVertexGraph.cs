using System;
using System.Collections.Generic;
using System.Linq;
using Random = System.Random;

namespace GraphEditor
{
    public class UndirectedVertexGraph
    {
        private Stack<Action> undoActions = new Stack<Action>();
        private readonly Dictionary<int, VertexNode> nodes = new();

        public IReadOnlyDictionary<int, IReadOnlyVertexNode> AsReadOnlyNodesDictionary
        {
            get { return nodes.ToDictionary(x => x.Key, x => (IReadOnlyVertexNode) x.Value); }
        }

        public IEnumerable<IReadOnlyVertexNode> Nodes => nodes.Values;

        public IEnumerable<(IReadOnlyVertexNode, IReadOnlyVertexNode)> Edges
        {
            get
            {
                var hashSet = new HashSet<(int, int)>();

                foreach (var node in nodes.Values)
                {
                    foreach (var neighbour in node.NeighboursVertex)
                    {
                        if (hashSet.Contains((node.Vertex, neighbour))
                            || hashSet.Contains((neighbour, node.Vertex)))
                            continue;
                        hashSet.Add((node.Vertex, neighbour));
                        yield return (node, nodes[neighbour]);
                    }
                }
            }
        }

        public void AddNode(int vertex)
        {
            _AddNode(vertex);
            undoActions.Push(() => _RemoveNode(vertex));
        }

        public void _AddNode(int vertex)
        {
            if (nodes.ContainsKey(vertex))
                throw new InvalidOperationException();
            nodes[vertex] = new VertexNode(vertex);
        }

        public bool RemoveNode(int vertex)
        {
            var removed = _RemoveNode(vertex);
            if (removed)
                undoActions.Push(() => _AddNode(vertex));

            return removed;
        }

        private bool _RemoveNode(int vertex)
        {
            return nodes.Remove(vertex);
        }

        public bool CheckForConnection(int vertex1, int vertex2)
        {
            return nodes[vertex1].NeighboursVertexSet.Contains(vertex2)
                   && nodes[vertex2].NeighboursVertexSet.Contains(vertex1);
        }

        public void ConnectNodes(int vertex1, int vertex2)
        {
            if (_ConnectNodes(vertex1, vertex2))
                undoActions.Push(() => _DisconnectNodes(vertex1, vertex2));
        }

        private bool _ConnectNodes(int vertex1, int vertex2)
        {
            if (CheckForConnection(vertex1, vertex2))
                return false;
            nodes[vertex1].NeighboursVertexSet.Add(vertex2);
            nodes[vertex2].NeighboursVertexSet.Add(vertex1);
            return true;
        }

        public void DisconnectNodes(int vertex1, int vertex2)
        {
            if (_DisconnectNodes(vertex1, vertex2))
                undoActions.Push(() => _ConnectNodes(vertex1, vertex2));
        }

        private bool _DisconnectNodes(int vertex1, int vertex2)
        {
            if (!CheckForConnection(vertex1, vertex2))
                return false;
            nodes[vertex1].NeighboursVertexSet.Remove(vertex2);
            nodes[vertex2].NeighboursVertexSet.Remove(vertex1);
            return true;
        }

        public void Undo()
        {
            if (undoActions.Count > 0)
                undoActions.Pop().Invoke();
        }

        public bool IsConnectedGraph()
        {
            return CheckGraphForConnectivity(this);
        }

        public override string ToString()
        {
            return string.Join("\n", nodes.Values.Select(x => x.ToString()));
        }

        public static UndirectedVertexGraph GenerateRandomGraph(int nodesCount, (int, int) edgesRange)
        {
            if (nodesCount < 1)
                throw new InvalidOperationException();
            if (edgesRange.Item1 < 0 || edgesRange.Item2 < 0 || edgesRange.Item1 > edgesRange.Item2)
                throw new InvalidOperationException();

            var random = new Random();
            var graph = new UndirectedVertexGraph();
            for (var i = 0; i < nodesCount; i++)
                graph.nodes[i] = new VertexNode(i);

            for (var i = 0; i < nodesCount; i++)
            {
                var node = graph.nodes[i];
                var minEdgesCount = Math.Max(0, edgesRange.Item1 - node.NeighboursVertexSet.Count);
                var maxEdgesCount = Math.Max(0, edgesRange.Item2 - node.NeighboursVertexSet.Count);
                var additionalEdgesCount = random.Next(minEdgesCount, maxEdgesCount);

                var possibleNodesList = graph.nodes.Values
                    .Where(x => x != node && !node.NeighboursVertexSet.Contains(x.Vertex) &&
                                x.NeighboursVertexSet.Count < edgesRange.Item2)
                    .ToArray();

                var additionalEdges = possibleNodesList
                    .GetRandomElements(random, additionalEdgesCount)
                    .Select(x => x.Vertex)
                    .ToArray();


                foreach (var additionalEdge in additionalEdges)
                {
                    graph.nodes[additionalEdge].NeighboursVertexSet.Add(i);
                    graph.nodes[i].NeighboursVertexSet.Add(additionalEdge);
                }
            }

            return graph;
        }

        public static bool CheckGraphForConnectivity(UndirectedVertexGraph undirectedGraph)
        {
            if (undirectedGraph.nodes.Count < 2)
                return true;

            var check = new bool[undirectedGraph.nodes.Count];
            var stack = new Stack<int>();
            stack.Push(0);
            check[0] = true;
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                foreach (var neighbour in undirectedGraph.nodes[node].NeighboursVertexSet)
                {
                    if (check[neighbour])
                        continue;
                    check[neighbour] = true;
                    stack.Push(neighbour);
                }
            }

            return check.All(x => x);
        }
    }
}