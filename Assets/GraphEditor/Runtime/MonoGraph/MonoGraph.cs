using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures.PriorityQueue;
using JetBrains.Annotations;
using UnityEngine;

namespace GraphEditor.Runtime
{
    public class MonoGraph : MonoBehaviour, IEdgeIndexer, INodeIndexer
    {
        [field: SerializeField] public GameObject NodesParent { get; set; }
        [field: SerializeField] public GameObject EdgesParent { get; set; }

        private List<MonoNode> nodes;
        private List<MonoEdge> edges;

        private Dictionary<int, MonoNode> idToNode;
        private Dictionary<int, MonoEdge> idToEdge;


        public IReadOnlyList<MonoNode> Nodes => nodes;
        public IReadOnlyList<MonoEdge> Edges => edges;

        public INodeIndexer IdToNode => this;
        public IEdgeIndexer IdToEdge => this;

        private bool initialized;
        public void Initialize()
        {
            if (initialized)
            {
                Debug.LogError($"{nameof(MonoGraph)} is initialized");
                return;
            }

            initialized = true;

            nodes = new List<MonoNode>();
            edges = new List<MonoEdge>();

            idToNode = new Dictionary<int, MonoNode>();
            idToEdge = new Dictionary<int, MonoEdge>();
        }

        public void Initialize(IEnumerable<MonoNode> nodes, IEnumerable<MonoEdge> edges)
        {
            if (initialized)
            {
                Debug.LogError($"{nameof(MonoGraph)} is initialized");
                return;
            }

            initialized = true;

            this.nodes = nodes.ToList();
            this.edges = edges.ToList();

            idToNode = this.nodes.ToDictionary(x => x.Id, x => x);
            idToEdge = this.edges.ToDictionary(x => x.Id, x => x);
        }

        public void AddNode(MonoNode node)
        {
            if (node == null)
                throw new ArgumentNullException();
            if (idToNode.ContainsKey(node.Id))
                throw new ArgumentException();

            nodes.Add(node);
            idToNode.Add(node.Id, node);
        }

        public bool RemoveNode(MonoNode node)
        {
            return RemoveNode(node.Id);
        }

        public bool RemoveNode(int nodeId)
        {
            if (!idToNode.ContainsKey(nodeId))
                return false;

            nodes.Remove(idToNode[nodeId]);
            idToNode.Remove(nodeId);
            return true;
        }

        public bool ContainsNode(MonoNode node)
        {
            return idToNode.ContainsKey(node.Id);
        }

        public bool ContainsNode(int nodeId)
        {
            return idToNode.ContainsKey(nodeId);
        }

        public void AddEdge(MonoEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException();
            if (idToEdge.ContainsKey(edge.Id))
                throw new ArgumentException();
            if (edge.FirstNode == null
                || edge.SecondNode == null
                || !idToNode.ContainsKey(edge.FirstNode.Id)
                || !idToNode.ContainsKey(edge.SecondNode.Id))
                throw new ArgumentException();

            edges.Add(edge);
            idToEdge.Add(edge.Id, edge);
        }

        public bool RemoveEdge(MonoEdge edge)
        {
            return RemoveEdge(edge.Id);
        }

        public bool RemoveEdge(int edgeId)
        {
            if (!idToEdge.ContainsKey(edgeId))
                return false;

            edges.Remove(idToEdge[edgeId]);
            idToEdge.Remove(edgeId);
            return true;
        }

        public bool ContainsEdge(MonoEdge edge)
        {
            return idToEdge.ContainsKey(edge.Id);
        }

        public bool ContainsEdge(int edgeId)
        {
            return idToEdge.ContainsKey(edgeId);
        }

        MonoNode INodeIndexer.this[int id]
        {
            get => idToNode[id];
            set
            {
                if (idToNode.TryGetValue(id, out var node))
                {
                    nodes.Remove(node);
                    idToNode.Remove(id);
                }
                AddNode(value);
            }
        }

        MonoEdge IEdgeIndexer.this[int id]
        {
            get => idToEdge[id];
            set
            {
                if (idToEdge.TryGetValue(id, out var edge))
                {
                    edges.Remove(edge);
                    idToEdge.Remove(id);
                }
                AddEdge(value);
            }
        }

        public bool CheckForConnection(MonoNode node1, MonoNode node2)
        {
            return node1.GetLine(node2) != null;
        }
        public MonoEdge ConnectNodes(MonoNode node1, MonoNode node2, MonoEdge edgePrefab)
        {
            var edgeId = 0;
            while (idToEdge.ContainsKey(edgeId))
                edgeId++;
            return ConnectNodes(node1, node2, edgePrefab, edgeId);
        }

        public void RedrawAllEdges()
        {
            foreach (var edge in edges)
                edge.Redraw();
        }
        
        public MonoEdge ConnectNodes(MonoNode node1, MonoNode node2, MonoEdge edgePrefab, int edgeId)
        {
            if (CheckForConnection(node1, node2))
                return null;

            var edge = Instantiate(edgePrefab, EdgesParent.transform);
            edge.Initialize(edgeId, node1, node2);
            node1.AddEdge(edge);
            node2.AddEdge(edge);
            
            idToEdge.Add(edge.Id, edge);
            edges.Add(edge);
            return edge;
        }

        public List<MonoNode> FindShortestPath(
            [NotNull] MonoNode start,
            [NotNull] MonoNode end,
            Func<MonoNode, MonoNode, bool> condition = null)
        {
            if (start == null || !idToNode.ContainsKey(start.Id))
                throw new ArgumentNullException(nameof(start));
            if (end == null || !idToNode.ContainsKey(end.Id))
                throw new ArgumentNullException(nameof(end));

            var queue = new Queue<MonoNode>();
            var track = new Dictionary<MonoNode, MonoNode>();
            queue.Enqueue(start);
            track[start] = null;
            while (queue.Count != 0)
            {
                var node = queue.Dequeue();
                foreach (var neighborhood in node.GetNeighbors())
                {
                    if (track.ContainsKey(neighborhood)) continue;
                    if (!idToNode.ContainsKey(neighborhood.Id)) throw new InvalidOperationException();
                    if (condition != null && !condition(node, neighborhood)) continue;
                    track[neighborhood] = node;
                    queue.Enqueue(neighborhood);
                }

                if (track.ContainsKey(end)) break;
            }

            if (!track.ContainsKey(end))
                return new List<MonoNode>();

            var pathItem = end;
            var result = new List<MonoNode>();
            while (pathItem != null)
            {
                result.Add(pathItem);
                pathItem = track[pathItem];
            }

            result.Reverse();
            return result;
        }

        /// <summary>
        /// Действует как волновой алгоритм
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="range">Если 0, то возвращает только ноду, если 1, то возвращает ноду и ее соседей</param>
        /// <param name="condition">Условие перехода из ноды в ноду, если истина, то переход возможен, если ложно, то нет.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Стартовая нода не содержится в графе</exception>
        /// <exception cref="InvalidOperationException">Выход за пределы графа</exception>
        public IEnumerable<MonoNode> GetNodesInRange(
            [NotNull] MonoNode startNode,
            uint range,
            Func<MonoNode, MonoNode, bool> condition = null)
        {
            if (startNode == null || !idToNode.ContainsKey(startNode.Id))
                throw new ArgumentException(nameof(startNode));

            var queue = new Queue<MonoNode>();
            var distanceMemory = new Dictionary<MonoNode, uint>();

            queue.Enqueue(startNode);
            distanceMemory[startNode] = 0;
            while (queue.Count != 0)
            {
                var node = queue.Dequeue();
                yield return node;
                foreach (var neighborhood in node.GetNeighbors())
                {
                    if (distanceMemory.ContainsKey(neighborhood)) continue;
                    if (!idToNode.ContainsKey(neighborhood.Id)) throw new InvalidOperationException();
                    if (condition != null && !condition(node, neighborhood)) continue;
                    var distanceForNextNode = distanceMemory[node] + 1;
                    if (distanceForNextNode > range) continue;

                    distanceMemory[neighborhood] = distanceForNextNode;
                    queue.Enqueue(neighborhood);
                }
            }
        }

        /// <summary>
        /// Алгоритм обхода графа в ширину из нескольких точек,
        /// которые передаются в словаре startNodes, где по ключу ноды передается ее "сила".
        /// Сила - расстояние, на которое пройдет волновой алгоритм из данной точки.
        ///
        /// interferingNodes - ноды, которые мешают проходу, или могут ему помогать
        /// </summary>
        public IEnumerable<MonoNode> MultiStartsLimitedBfs(
            IReadOnlyDictionary<MonoNode, int> startNodes,
            IReadOnlyDictionary<MonoNode, int> interferingNodes)
        {
            if (startNodes.Count == 0)
                throw new ArgumentException("Нет стартовых нод!");
            if (startNodes.Keys.Any(x => !idToNode.ContainsKey(x.Id))
                || interferingNodes.Keys.Any(x => !idToNode.ContainsKey(x.Id)))
                throw new InvalidOperationException("Есть ноды не содержащиеся в графе!");

            var closedNodes = new HashSet<MonoNode>();
            var priorityQueue = new PriorityQueue<MonoNode, int>(0);
            foreach (var pair in startNodes)
                priorityQueue.Enqueue(pair.Key, -pair.Value + FindValue(pair.Key));

            while (priorityQueue.Count != 0)
            {
                var (node, currentVisibility) = priorityQueue.Dequeue();
                if (closedNodes.Contains(node)) continue;

                closedNodes.Add(node);
                yield return node;
                if (currentVisibility == 0) continue;
                foreach (var neighbor in node.GetNeighbors())
                {
                    if (closedNodes.Contains(neighbor)) continue;
                    var nextVisibility = currentVisibility + 1 + FindValue(neighbor);
                    if (nextVisibility > 0) continue;
                    priorityQueue.Enqueue(neighbor, nextVisibility);
                }
            }

            int FindValue(MonoNode node)
            {
                return interferingNodes.TryGetValue(node, out var negativeValue)
                    ? negativeValue
                    : 0;
            }
        }

        /// <summary>
        /// Возвращает ноду и расстояние до нее, расстояние в данном случае — это минимальное чисто нод,
        /// встретившихся по пути из rootNode, включая конечную.
        /// RootNode включена в перечисление и имеет расстояние 0.
        /// Не возвращает несвязанные части графа.
        /// </summary>
        public IEnumerable<(MonoNode, uint)> FindDistanceToNodes(MonoNode rootNode)
        {
            if (rootNode == null || !idToNode.ContainsKey(rootNode.Id))
                throw new ArgumentException(nameof(rootNode));

            var queue = new Queue<MonoNode>();
            var distanceMemory = new Dictionary<MonoNode, uint>();

            queue.Enqueue(rootNode);
            distanceMemory[rootNode] = 0;
            while (queue.Count != 0)
            {
                var node = queue.Dequeue();
                yield return (node, distanceMemory[node]);
                foreach (var neighborhood in node.GetNeighbors())
                {
                    if (distanceMemory.ContainsKey(neighborhood)) continue;
                    if (!idToNode.ContainsKey(neighborhood.Id)) throw new InvalidOperationException();
                    var distanceForNextNode = distanceMemory[node] + 1;
                    distanceMemory[neighborhood] = distanceForNextNode;
                    queue.Enqueue(neighborhood);
                }
            }
        }


        public HashSet<(MonoNode, MonoNode)> FindMostRemoteNodes()
        {
            var maxDistance = uint.MinValue;
            var mostRemoteNodes = new HashSet<(MonoNode, MonoNode)>();

            foreach (var node1 in nodes)
            {
                foreach (var (node2, distance) in FindDistanceToNodes(node1))
                {
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        mostRemoteNodes = new HashSet<(MonoNode, MonoNode)>() { (node1, node2) };
                    }
                    else if (distance == maxDistance
                             && !mostRemoteNodes.Contains((node2, node1)))
                    {
                        mostRemoteNodes.Add((node1, node2));
                    }
                }
            }

            return mostRemoteNodes;
        }

        public static void BFS(MonoNode start,
            Action<MonoNode, MonoNode> beforeTransitionAction = null)
        {
            var visited = new HashSet<MonoNode>();
            var queue = new Queue<MonoNode>();
            queue.Enqueue(start);
            visited.Add(start);
            while (queue.Count != 0)
            {
                var node = queue.Dequeue();

                foreach (var neighbor in node.Neighbors)
                {
                    beforeTransitionAction(node, neighbor);
                    if (visited.Contains(neighbor))
                        continue;
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }
        
        public IEnumerable<(MonoEdge, MonoEdge)> GetIntersectionsEdges()
        {
            foreach (var monoEdge1 in Edges.ToArray())
            {
                foreach (var monoEdge2 in Edges.ToArray())
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

                    yield return (monoEdge1, monoEdge2);
                }
            }
        }
    }
}
