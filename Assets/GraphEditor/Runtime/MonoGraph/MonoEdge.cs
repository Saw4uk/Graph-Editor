using UnityEngine;

namespace GraphEditor
{
    public class MonoEdge : MonoBehaviour, INumbered
    {
        [Header("Graph Settings")]
        [SerializeField] private int id;

        [SerializeField] private MonoNode firstNode;
        [SerializeField] private MonoNode secondNode;

        [Header("References")] 
        [SerializeField] private SpriteRenderer edgeSpriteRenderer;
        [SerializeField] private BoxCollider2D edgeCollider;

        public SpriteRenderer SpriteRenderer => edgeSpriteRenderer;
        public BoxCollider2D BoxCollider2D => edgeCollider;

        public int Id => id;


        public MonoNode FirstNode
        {
            get => firstNode;
            set => firstNode = value;
        }

        public MonoNode SecondNode
        {
            get => secondNode;
            set => secondNode = value;
        }

        public void Initialize(int index, MonoNode firstMonoNode, MonoNode secondMonoNode)
        {
            this.id = index;
            this.firstNode = firstMonoNode;
            this.secondNode = secondMonoNode;
        }

        public void Redraw()
        {
            name = $"Edge{Id}";
            RedrawLine();
            AlineCollider();
            return;

            void RedrawLine()
            {
                var v1 = FirstNode ? FirstNode.Position : Vector2.zero;
                var v2 = SecondNode ? SecondNode.Position : Vector2.right;
                var distance = Vector2.Distance(v1, v2);
                var center = v1;
                var newSecondNodePosition = v2 - center;
                var radian = Mathf.Atan2(newSecondNodePosition.y, newSecondNodePosition.x) * 180 / Mathf.PI;

    
                SpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, radian);
                SpriteRenderer.transform.position = (v1 + v2) / 2;
            
                SpriteRenderer.size = new Vector2(distance, SpriteRenderer.size.y);
            }

            void AlineCollider()
            {
                BoxCollider2D.size = SpriteRenderer.size;
            }
        }
    }
}