using System;
using System.Collections.Generic;
using System.Linq;
using DataStructures;
using UnityEngine;

namespace GraphEditor.Runtime
{
    public class NodeSelector : MonoBehaviour
    {
        private HashSet<int> selectedObjects = new HashSet<int>();

        [SerializeField] private float widthOfMeshSelectedObjects = 0.5f;

        private Mesh meshSelectedObjects;

        public IEnumerable<MonoNode> SelectedNodes =>
            selectedObjects.Select(nodeId => GraphEditorRoot.Instance.MonoGraph.IdToNode[nodeId]).ToArray();

        public bool isEditable { get; set; }

        public void Initialize()
        {
            meshSelectedObjects = new Mesh();
            GetComponent<MeshFilter>().mesh = meshSelectedObjects;
        }

        private void Update()
        {
            if (isEditable && Input.GetMouseButtonDown(0) && !CameraController.PointerIsOverAnyObject() &&
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
            if (!isEditable) return;
            
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
            if (!isEditable) return;
            
            if (selectedObjects.Contains(nodeId))
            {
                //Debug.LogError($"The node with ID {nodeId} has already been added to selectedObjects");
                return;
            }

            selectedObjects.Add(nodeId);
            SelectNode(nodeId);
            Debug.Log($"Node with ID {nodeId} was added. Number of selected objects {selectedObjects.Count}");
        }

        public void Remove(int nodeId)
        {
            if (!isEditable) return;
            
            if (!selectedObjects.Remove(nodeId))
            {
                //Debug.LogError($"SelectedObjects does not have a node with ID {nodeId}");
                return;
            }

            DeselectNode(nodeId);
            Debug.Log($"Node with ID {nodeId} was removed. Number of selected objects {selectedObjects.Count}");
        }

        public void Clear()
        {
            if (selectedObjects.Count < 1)
                return;

            foreach (var nodeId in selectedObjects)
                DeselectNode(nodeId);

            selectedObjects = new HashSet<int>();
            meshSelectedObjects.Clear();
            Debug.Log($"Cleared");
        }

        private void SelectNode(int nodeId)
        {
            var node = GraphEditorRoot.Instance.MonoGraph.IdToNode[nodeId];
            if (node != null)
                node.Select();

            RedrawBordersSelectedObjects();
        }

        private void DeselectNode(int nodeId)
        {
            var node = GraphEditorRoot.Instance.MonoGraph.IdToNode[nodeId];
            if (node != null)
                node.Deselect();

            RedrawBordersSelectedObjects();
        }

        public void RedrawBordersSelectedObjects()
        {
            if (!SelectedNodes.Any())
            {
                meshSelectedObjects.Clear();
                return;
            }
            
            var leftBottomCorner = Vector2.positiveInfinity;
            var rightTopCorner = Vector2.negativeInfinity;

            foreach (var node in SelectedNodes)
            {
                leftBottomCorner = Vector2.Min(leftBottomCorner, node.SpriteRenderer.bounds.min);
                rightTopCorner = Vector2.Max(rightTopCorner, node.SpriteRenderer.bounds.max);
            }

            var corners = new[]
            {
                new Vector3(leftBottomCorner.x, leftBottomCorner.y), new Vector3(rightTopCorner.x, leftBottomCorner.y),
                new Vector3(rightTopCorner.x, rightTopCorner.y), new Vector3(leftBottomCorner.x, rightTopCorner.y),
            };

            DrawHollowMesh(corners, widthOfMeshSelectedObjects);
        }

        void DrawHollowMesh(Vector3[] innerPoints, float width)
        {
            var pointsList = new List<Vector3>();

            var outerPoints = GetOuterPoints(innerPoints, width);
            pointsList.AddRange(outerPoints);
            pointsList.AddRange(innerPoints);

            var polygonPoints = pointsList.ToArray();
            var polygonTriangles = DrawHollowTriangles(polygonPoints);

            meshSelectedObjects.Clear();
            meshSelectedObjects.vertices = polygonPoints;
            meshSelectedObjects.triangles = polygonTriangles;
        }

        private int[] DrawHollowTriangles(Vector3[] points)
        {
            var sides = points.Length / 2;

            var newTriangles = new List<int>();
            for (var i = 0; i < sides; i++)
            {
                var outerIndex = i;
                var innerIndex = i + sides;

                //first triangle starting at outer edge i
                newTriangles.Add(outerIndex);
                newTriangles.Add(innerIndex);
                newTriangles.Add((i + 1) % sides);

                //second triangle starting at outer edge i
                newTriangles.Add(outerIndex);
                newTriangles.Add(sides + ((sides + i - 1) % sides));
                newTriangles.Add(outerIndex + sides);
            }

            return newTriangles.ToArray();
        }

        private List<Vector3> GetOuterPoints(IReadOnlyList<Vector3> points, float width)
        {
            var outerPoints = new List<Vector3>();
            var allResVectors = new List<Vector3>();

            for (int i = 0; i < points.Count; i++)
            {
                var p1 = i == 0 ? points[points.Count - 1] : points[i - 1];
                var p2 = points[i];

                var l = p2 - p1;

                var resVec = new Vector2(l.y, -l.x).normalized * width;
                allResVectors.Add(resVec);
            }

            for (int i = 0; i < allResVectors.Count; i++)
            {
                var n1 = i == 0 ? allResVectors[allResVectors.Count - 1] : allResVectors[i - 1];
                var n2 = allResVectors[i];

                var sum = n1 + n2;

                var dotProduct = Vector3.Dot(n1, n2) + 1;

                var currentPoint = i == 0 ? points[points.Count - 1] : points[i - 1];

                outerPoints.Add(currentPoint + (sum / dotProduct));
            }

            return outerPoints;
        }
    }
}