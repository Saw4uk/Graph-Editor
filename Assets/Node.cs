using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Node : MonoBehaviour, INumbered
{
    [SerializeField] private int id;

    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private List<Edge> edges;

    [field: Header("References")] 
    [SerializeField] private SpriteRenderer spriteRenderer;

    public int Id => id;
    public Vector2 Position => transform.position;
    public IEnumerable<Edge> Edges => edges;


    public void Initialize(int index)
    {
        id = index;
        edges = new List<Edge>();
    }

    public void AddEdge(Edge edge)
    {
        if (edge == null || edges.Contains(edge))
            return;
        edges.Add(edge);
    }

    public bool RemoveEdge(Edge edge) => edges.Remove(edge);
    public Edge GetLine(Node node) => Edges.Intersect(node.Edges).FirstOrDefault();
    public IEnumerable<Node> GetNeighbors()
    {
        foreach (var edge in Edges)
        {
            if (edge.FirstNode.Equals(this))
                yield return edge.SecondNode;
            else
                yield return edge.FirstNode;
        }
    }

    public void Redraw()
    {
    }
    
    public void SetActiveOutline(bool value)
    {
    }
}
