using UnityEngine;


public class Edge : MonoBehaviour, INumbered
{
    [Header("Graph Settings")]
    [SerializeField] private int id;

    [SerializeField] private Node firstNode;
    [SerializeField] private Node secondNode;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer edgeSpriteRenderer;
    [SerializeField] private BoxCollider2D edgeCollider;
    
    public SpriteRenderer SpriteRenderer => edgeSpriteRenderer;
    public BoxCollider2D BoxCollider2D => edgeCollider;

    public int Id => id;
    

    public Node FirstNode
    {
        get => firstNode;
        set => firstNode = value;
    }

    public Node SecondNode
    {
        get => secondNode;
        set => secondNode = value;
    }
    
    public void Initialize(int index, Node firstNode, Node secondNode)
    {
        this.id = index;
        this.firstNode = firstNode;
        this.secondNode = secondNode;
    }
}
