using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphEditor
{
    public class MonoNode : MonoBehaviour, INumbered
    {
        [SerializeField] private int id;

        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private List<MonoEdge> edges;

        [field: Header("References")] 
        [SerializeField] private SpriteRenderer spriteRenderer;

        public int Id => id;
        public Vector2 Position => transform.position;
        public IEnumerable<MonoEdge> Edges => edges;
        public int EdgesCount => edges.Count;


        public void Initialize(int index)
        {
            id = index;
            edges = new List<MonoEdge>();
        }

        public void AddEdge(MonoEdge monoEdge)
        {
            if (monoEdge == null || edges.Contains(monoEdge))
                return;
            edges.Add(monoEdge);
        }

        public bool RemoveEdge(MonoEdge monoEdge) => edges.Remove(monoEdge);
        public MonoEdge GetLine(MonoNode monoNode) => Edges.Intersect(monoNode.Edges).FirstOrDefault();
        public IEnumerable<MonoNode> GetNeighbors()
        {
            foreach (var edge in Edges)
            {
                if (edge.FirstNode.Equals(this))
                    yield return edge.SecondNode;
                else
                    yield return edge.FirstNode;
            }
        }
    }
}
