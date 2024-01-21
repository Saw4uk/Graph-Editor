using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GraphEditor.Runtime
{
    public class SelectionAreaController : MonoBehaviour
    {
        public static readonly List<MonoNode> Nodes = new List<MonoNode>();
        private static readonly List<MonoNode> nodesInSelectionArea = new List<MonoNode>();

        [SerializeField] private Image selectionAreaImage;

        private RectTransform selectionAreaRectTransform;
        private bool draw;
        private Vector2 startPos;
        private Vector2 endPos;
        private Camera mainCamera;
        public bool isEditable { get; set; }
        
        public void Initialize()
        {
            selectionAreaRectTransform = selectionAreaImage.GetComponent<RectTransform>();
            mainCamera = CameraController.MainCamera;
            Deactivate();
        }

        private void Update()
        {
            if (!isEditable) return;
            
            if (!draw && Input.GetMouseButtonDown(0) && !CameraController.PointerIsOverAnyObject() &&
                !CameraController.PointerIsOverUI())
            {
                startPos = Input.mousePosition;
                nodesInSelectionArea.Clear();
                draw = true;
            }

            if (draw && Input.GetMouseButtonUp(0))
            {
                draw = false;
                Select();
                Deactivate();
            }

            if (!draw) return;
            if (!DrawSelectionArea())
                return;

            var position = selectionAreaRectTransform.position;
            var size = selectionAreaRectTransform.sizeDelta;

            SelectNodesInArea(new Vector2(position.x, position.y - size.y),
                new Vector2(position.x + size.x, position.y));
        }

        private bool DrawSelectionArea()
        {
            endPos = Input.mousePosition;
            if (startPos == endPos) return false;

            Activate();

            selectionAreaRectTransform.position =
                new Vector3(Mathf.Min(endPos.x, startPos.x), Mathf.Max(endPos.y, startPos.y));

            selectionAreaRectTransform.sizeDelta = new Vector2(
                Mathf.Max(endPos.x, startPos.x) - Mathf.Min(endPos.x, startPos.x),
                Mathf.Max(endPos.y, startPos.y) - Mathf.Min(endPos.y, startPos.y));
            return true;
        }

        private void SelectNodesInArea(Vector2 areaLeftBottomCorner, Vector2 areaRightTopCorner)
        {
            nodesInSelectionArea.Clear();
            foreach (var unit in Nodes)
            {
                var bounds = unit.Bounds;
                var unitLeftBottomCorner = (Vector2)mainCamera.WorldToScreenPoint(bounds.min);
                var unitRightTopCorner = (Vector2)mainCamera.WorldToScreenPoint(bounds.max);

                if ((unitRightTopCorner.x > areaLeftBottomCorner.x && unitRightTopCorner.y > areaLeftBottomCorner.y) &&
                    (areaRightTopCorner.x > unitLeftBottomCorner.x && areaRightTopCorner.y > unitLeftBottomCorner.y) &&
                    !nodesInSelectionArea.Contains(unit))
                    nodesInSelectionArea.Add(unit);
            }
        }

        private void Select()
        {
            var selectedUnits = nodesInSelectionArea.Select(unit => unit.GetComponent<INumbered>().Id).ToArray();

            if (selectedUnits.Length < 1)
                return;

            Undo.AddActions(
                () =>
                {
                    nodesInSelectionArea.Clear();
                    GraphEditorRoot.Instance.NodeSelector.Clear();
                },
                () => GraphEditorRoot.Instance.NodeSelector.AddMany(selectedUnits)
            );

            GraphEditorRoot.Instance.NodeSelector.AddMany(selectedUnits);
        }

        public void Activate()
        {
            selectionAreaImage.gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            selectionAreaImage.gameObject.SetActive(false);
            startPos = endPos = Vector2.zero;
        }

        private void OnDisable()
        {
            draw = false;
            startPos = endPos = Vector2.zero;
        }
    }
}