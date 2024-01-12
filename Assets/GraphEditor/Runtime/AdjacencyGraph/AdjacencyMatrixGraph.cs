using System.Collections.Generic;
using System;

namespace GraphEditor.Runtime
{
    public class AdjacencyMatrixGraph
    {
        private bool[,] adjacencyMatrix;

        public int NodeCount => adjacencyMatrix.GetLength(0);
        
        public AdjacencyMatrixGraph(bool[,] adjacencyMatrix)
        {
            if (adjacencyMatrix.GetLength(0) != adjacencyMatrix.GetLength(1))
                throw new ArgumentException("Adjacency Matrix should be square!");
            this.adjacencyMatrix = adjacencyMatrix.Clone() as bool[,];
        }

        public AdjacencyMatrixGraph(int nodeCount)
        {
            adjacencyMatrix = new bool[nodeCount, nodeCount];
        }

        public IReadOnlyList<bool> GetAdjacencyArray(int nodeId)
        {
            var resultArray = new bool[NodeCount];
            for(var i = 0; i < NodeCount; i++)
            {
                resultArray[i] = adjacencyMatrix[nodeId, i];
            }

            return resultArray;
        }

        public bool AreIntersect(int nodeId1, int nodeId2)
        {
            return adjacencyMatrix[nodeId1, nodeId2];
        }

        public void SetAdjacency(int nodeId1, int nodeId2, bool value)
        {
            adjacencyMatrix[nodeId1, nodeId2] = value;
            adjacencyMatrix[nodeId2, nodeId1] = value;
        }

        public void FindNumberOfSimpleCycles()
        {
            int ans = 1;
            int[,] dp = new int[(1 << NodeCount), NodeCount];
            for (int mask = 0;
                 mask < (1 << NodeCount); mask++)
            {
                int nodeSet
                    = Builtin_popcountll(mask);
                int firstSetBit
                    = GetFirstSetBitPos(mask);
                if (nodeSet == 1)
                {
                    dp[mask, firstSetBit - 1] = 1;
                }
                else
                {
                    for (int j = firstSetBit + 1;
                         j < NodeCount; j++)
                    {
                        if ((mask & (1 << j)) != 0)
                        {
                            int newNodeSet = mask ^ (1 << j);
                            for (int k = 0; k < NodeCount; k++)
                            {
                                if ((newNodeSet & (1 << k)) != 0
                                    && adjacencyMatrix[k, j])
                                {
                                    dp[mask, j]
                                        += dp[newNodeSet, k];
                                    if (adjacencyMatrix[j, firstSetBit]
                                        && nodeSet > 2)
                                        ans += dp[mask, j];
                                }
                            }
                        }
                    }
                }
            }
        }

        private int Builtin_popcountll(long x)
        {
            int setBits = 0;
            while (x != 0)
            {
                x = x & (x - 1);
                setBits++;
            }
            return setBits;
        }
        private int GetFirstSetBitPos(int n)
        {
            return (int)((Math.Log10(n & -n)) / Math.Log10(2)) + 1;
        }

        public static AdjacencyMatrixGraph FromMonoGraph(MonoGraph graph)
        {
            var newGraph = new AdjacencyMatrixGraph(graph.Nodes.Count);
            foreach(var edge in graph.Edges)
            {
                var nodeId1 = graph.Nodes.FindIndex(edge.FirstNode);
                var nodeId2 = graph.Nodes.FindIndex(edge.SecondNode);
                newGraph.SetAdjacency(nodeId1, nodeId2, true);
            }

            return newGraph;
        }
    }
}
