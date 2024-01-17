using System.Linq;
using GraphEditor;
using GraphEditor.Runtime;
using NUnit.Framework;
using UnityEngine;

public class Tests
{
    [Test]
    [TestCase(100, 10, 2,4)]
    public void TestRandomGraphSimplePasses(int count, int nodesCount, int minEdgesCount, int maxEdgesCount)
    {
        for (int i = 0; i < count; i++)
        {
            var randomGraph = UndirectedVertexGraph.GenerateRandomGraph(nodesCount, (minEdgesCount, maxEdgesCount));
            
            Assert.IsTrue(randomGraph.AsReadOnlyNodesDictionary.Values.Min(x => x.NeighboursVertex.Count) >= minEdgesCount);
            Assert.IsTrue(randomGraph.AsReadOnlyNodesDictionary.Values.Max(x => x.NeighboursVertex.Count) <= maxEdgesCount);

            foreach (var node in randomGraph.AsReadOnlyNodesDictionary.Values)
            {
                foreach (var vert in node.NeighboursVertex)
                {
                    Assert.IsTrue(randomGraph.AsReadOnlyNodesDictionary[vert].NeighboursVertex.Contains(node.Value));
                }
            }
        }
    }
    
    
    [Test]
    public void TestConnectivityGraph()
    {
        var graph = new UndirectedVertexGraph();
        graph.AddNode(0);
        graph.AddNode(1);
        graph.AddNode(2);
        graph.AddNode(3);
        
        graph.ConnectNodes(0,1);
        graph.ConnectNodes(0,2);
        graph.ConnectNodes(0,3);
        
        Assert.IsTrue(UndirectedVertexGraph.CheckGraphForConnectivity(graph));
    }

    [Test]
    public void TestNotConnectivityGraph()
    {
        var graph = new UndirectedVertexGraph();
        graph.AddNode(0);
        graph.AddNode(1);
        graph.AddNode(2);
        graph.AddNode(3);
        
        Assert.IsFalse(UndirectedVertexGraph.CheckGraphForConnectivity(graph));
    }

    [Test]
    public void TestSegmentIntersection()
    {
        var a1 = new Vector2(0, -100);
        var a2 = new Vector2(0, 100);
        
        var b1 = new Vector2(-100, 0);
        var b2 = new Vector2(100, 0);
        
        Assert.IsTrue(CustomMath.SegmentsIsIntersects(a1, a2, b1, b2));
        Assert.IsTrue(CustomMath.SegmentsIsIntersects(a2, a1, b2, b1));
        
        Assert.IsFalse(CustomMath.SegmentsIsIntersects(a1, b1, a2, b2));
        
        Assert.IsTrue(CustomMath.SegmentsIsIntersects(a1, b1, a1, b2));
    }
    
    [Test]
    public void TestSegmentIntersection1()
    {
        var a1 = new Vector2(1, 1);
        var a2 = new Vector2(3, 2);
        
        var b1 = new Vector2(1, 4);
        var b2 = new Vector2(2, -1);
        
        Assert.IsTrue(CustomMath.SegmentsIsIntersects(a1, a2, b1, b2));
        Assert.IsTrue(CustomMath.SegmentsIsIntersects(a2, a1, b2, b1));
    }
}
