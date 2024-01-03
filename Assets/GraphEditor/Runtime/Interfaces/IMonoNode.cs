using System.Collections.Generic;
using UnityEngine;

namespace GraphEditor
{
    public interface IMonoNode<TNode, TEdge>: INumbered
        where TNode : MonoBehaviour, IMonoNode<TNode, TEdge>
        where TEdge : MonoBehaviour, IMonoEdge<TNode, TEdge>
    {
        void Initialize(int index);
        
        Vector2 Position { get; }
        IEnumerable<TEdge> Edges { get; }
        int EdgesCount { get; }
        
        
        void AddEdge(TEdge monoEdge);
        bool RemoveEdge(TEdge monoEdge);
        TEdge GetLine(TNode monoNode);
        
        IEnumerable<TNode> GetNeighbors();
    }
}