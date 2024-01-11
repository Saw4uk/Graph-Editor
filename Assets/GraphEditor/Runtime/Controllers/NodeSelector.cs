using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GraphEditor.Runtime
{
    public class NodeSelector : MonoBehaviour
    {
        public static NodeSelector Instance { get; private set; }

        private HashSet<int> selectedObjects = new HashSet<int>();

        public IEnumerable<MonoNode> SelectedNodes =>
            selectedObjects.Select(nodeId => GraphTool.Instance.Graph.IdToNode[nodeId]).ToArray();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogError($"Больше чем два {nameof(NodeSelector)} на сцене");
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && !CameraController.PointerIsOverAnyObject() &&
                !CameraController.PointerIsOverUI() && selectedObjects.Count > 0 &&
                !UI.Instance.IsWaitingMouseClick)
            {
                var oldSelectedObjects = selectedObjects.ToArray();

                Undo.AddActions(
                    () => AddMany(oldSelectedObjects),
                    Clear
                );
                Clear();
            }
        }

        public void ChangeSelectionState(int nodeId)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                if (selectedObjects.Contains(nodeId))
                {
                    Remove(nodeId);

                    Undo.AddActions(
                        () => Add(nodeId),
                        () => Remove(nodeId)
                    );
                }
                else
                {
                    Add(nodeId);

                    Undo.AddActions(
                        () => Remove(nodeId),
                        () => Add(nodeId)
                    );
                }
            else
            {
                if (selectedObjects.Contains(nodeId))
                    return;

                var oldSelectedObjects = selectedObjects.ToArray();

                Undo.AddActions(
                    () =>
                    {
                        Remove(nodeId);
                        AddMany(oldSelectedObjects);
                    },
                    () =>
                    {
                        Clear();
                        Add(nodeId);
                    }
                );

                Clear();
                Add(nodeId);
            }
        }

        public void AddMany(IEnumerable<int> nodesId)
        {
            foreach (var id in nodesId)
                Add(id);
        }

        public void Add(int nodeId)
        {
            if (selectedObjects.Contains(nodeId))
            {
                Debug.LogError("The node has already been added to selectedObjects");
                return;
            }

            selectedObjects.Add(nodeId);
            SelectNode(nodeId);
            Debug.Log($"Node with ID {nodeId} was added. Number of selected objects {selectedObjects.Count}");
        }

        public void Remove(int nodeId)
        {
            if (!selectedObjects.Remove(nodeId))
                return;

            DeselectNode(nodeId);
            Debug.Log($"Node with ID {nodeId} was removed. Number of selected objects {selectedObjects.Count}");
        }

        private void Clear()
        {
            if (selectedObjects.Count < 1)
                return;

            foreach (var nodeId in selectedObjects)
                DeselectNode(nodeId);

            selectedObjects = new HashSet<int>();
            Debug.Log($"Cleared");
        }

        private void SelectNode(int nodeId)
        {
            var node = GraphTool.Instance.Graph.IdToNode[nodeId];
            if (node != null)
            {
                node.SpriteRenderer.color = new Color(0.97f, 0.51f, 0f);
            }
        }

        private void DeselectNode(int nodeId)
        {
            var node = GraphTool.Instance.Graph.IdToNode[nodeId];
            if (node != null)
                node.SpriteRenderer.color = Color.white;
        }

        public void DragSelectedObjects(Vector3 deltaPosition)
        {
            foreach (var node in SelectedNodes)
            {
                if (node == null)
                    throw new ArgumentNullException();

                node.transform.position += new Vector3(deltaPosition.x, deltaPosition.y);

                foreach (var edge in node.Edges)
                    edge.Redraw();
            }
        }
    }
}