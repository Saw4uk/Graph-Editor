using UnityEngine;

namespace GraphEditor
{
    public interface IMonoEdge<TNode, TEdge>: INumbered
        where TNode : MonoBehaviour, IMonoNode<TNode, TEdge>
        where TEdge : MonoBehaviour, IMonoEdge<TNode, TEdge>
    {
        void Initialize(int index, TNode firstMonoNode, TNode secondMonoNode);
        
        TNode FirstNode { get; set; }
        TNode SecondNode { get; set; }
        
        SpriteRenderer SpriteRenderer { get; }
        BoxCollider2D BoxCollider2D { get; }
        
        void Redraw();
    }
}