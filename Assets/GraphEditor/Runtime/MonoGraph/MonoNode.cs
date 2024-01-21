using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphEditor.Runtime
{
    public class MonoNode : MonoBehaviour, INumbered
    {
        [SerializeField] private int id;

        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private List<MonoEdge> edges;

        [field: Header("References")] [SerializeField]
        private SpriteRenderer spriteRenderer;

        private Vector3 pivotPoint;
        private Vector3 offsetPosition;

        public int Id => id;
        public Vector2 Position => transform.position;
        public IEnumerable<MonoEdge> Edges => edges;
        public IEnumerable<MonoNode> Neighbors => GetNeighbors();
        public int EdgesCount => edges.Count;
        public SpriteRenderer SpriteRenderer => spriteRenderer;


        public void Initialize(int index)
        {
            id = index;
            edges = new List<MonoEdge>();
            
            if (GraphEditorRoot.Instance.IsEditable)
                SelectionAreaController.Nodes.Add(this);
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

        public NodeInfo GetNodeInfo()
        {
            var neighborsId = GetNeighbors().Select(neighbor => neighbor.id).ToArray();
            return new NodeInfo(id, neighborsId, Position);
        }

        private void OnMouseDown()
        {
            GraphEditorRoot.Instance.NodeSelector.ChangeSelectionState(id);
            var mousePosition = CameraController.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            offsetPosition = mousePosition - transform.position;
            pivotPoint = mousePosition;
        }

        private void OnMouseUp()
        {
            var deltaPositionDistance = Vector3.Distance(pivotPoint - offsetPosition, transform.position);
            if (deltaPositionDistance > 0.000001)
            {
                var deltaPosition = transform.position - pivotPoint + offsetPosition;

                if (GraphEditorRoot.Instance.GraphTool.isEditable)
                    Undo.AddActions(
                        () => GraphEditorRoot.Instance.GraphTool.MoveSelectedNodes(-deltaPosition),
                        () => GraphEditorRoot.Instance.GraphTool.MoveSelectedNodes(deltaPosition)
                    );
            }

            offsetPosition = Vector2.zero;
            pivotPoint = Vector2.zero;
        }

        private void OnMouseDrag()
        {
            var mousePosition = CameraController.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            GraphEditorRoot.Instance.GraphTool.MoveSelectedNodes(mousePosition - offsetPosition - transform.position);
        }

        private void OnDestroy()
        {
            if (GraphEditorRoot.Instance != null && GraphEditorRoot.Instance.IsEditable)
                SelectionAreaController.Nodes.Remove(this);
        }
    }
}