using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;
// ReSharper disable Unity.InefficientPropertyAccess


namespace GraphEditor
{
    public abstract class GraphToolBase<TNode, TEdge, TGraph> : EditorTool
        where TNode : MonoBehaviour, IMonoNode<TNode, TEdge>
        where TEdge : MonoBehaviour, IMonoEdge<TNode, TEdge>
        where TGraph : MonoBehaviour, IMonoGraph<TNode, TEdge>
    {
        private TEdge monoEdgePrefab;
        private TNode monoNodePrefab;
        private TGraph graph;
        
        private SelectionListener<TNode> nodeListener;

        protected abstract TNode GetNodePrefab();
        protected abstract TEdge GetEdgePrefab();

        public override void OnActivated()
        {
            base.OnActivated();

            monoEdgePrefab = GetEdgePrefab();
            monoNodePrefab = GetNodePrefab();

            AssignGraph();

            foreach (var gameObject in FindObjectsOfType<GameObject>())
                SceneVisibilityManager.instance.DisablePicking(gameObject, false);

            SceneVisibilityManager.instance.EnablePicking(graph.gameObject, false);
            SceneVisibilityManager.instance.EnablePicking(graph.NodesParent.gameObject, true);

            EditorApplication.RepaintHierarchyWindow();

            nodeListener = new SelectionListener<TNode>();
        }
        

        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();
            OnDisable();
        }

        private void OnDisable()
        {
            foreach (var gameObject in FindObjectsOfType<GameObject>())
                SceneVisibilityManager.instance.EnablePicking(gameObject, false);

            EditorApplication.RepaintHierarchyWindow();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            UsePositionHandle();
            if (Event.current.Equals(Event.KeyboardEvent("k")))
            {
                PutNodeInMousePosition();
            }
            else if (Event.current.Equals(Event.KeyboardEvent("delete")))
            {
                DeleteSelectedNodes();
            }
        }

        private void AssignGraph()
        {
            var graphObj = GameObject.Find("Graph") ?? new GameObject("Graph");
            graph = graphObj.GetComponent<TGraph>() ??
                    graphObj.AddComponent<TGraph>();

            if (graph.NodesParent == null)
            {
                graph.NodesParent = new GameObject("Nodes");
                graph.NodesParent.transform.SetParent(graph.transform);
            }

            if (graph.EdgesParent == null)
            {
                graph.EdgesParent = new GameObject("Edges");
                graph.EdgesParent.transform.SetParent(graph.transform);
            }
        }

        private void DeleteSelectedNodes()
        {
            var allDeletedEdges = new List<TEdge>();
            var allDeletedNodes = new List<TNode>();
            var allNeighboringNodes = new List<TNode>();

            foreach (var node in nodeListener.GetActive().ToArray())
            {
                allDeletedNodes.Add(node);
                allNeighboringNodes.AddRange(node.GetNeighbors());
                allDeletedEdges.AddRange(node.Edges);
            }

            allDeletedEdges = allDeletedEdges.Distinct().ToList();
            allNeighboringNodes = allNeighboringNodes.Distinct().ToList();

            Undo.IncrementCurrentGroup();
            foreach (var node in allNeighboringNodes)
            {
                Undo.RecordObject(node, "Delete Node");
                var myDeleteEdges = node.Edges.Intersect(allDeletedEdges).ToArray();
                foreach (var deleteEdge in myDeleteEdges)
                    node.RemoveEdge(deleteEdge);
                EditorUtility.SetDirty(node);
            }

            foreach (var node in allDeletedNodes)
            {
                Undo.RecordObject(node, "Delete Node");
                var myDeleteEdges = node.Edges.Intersect(allDeletedEdges).ToArray();
                foreach (var deleteEdge in myDeleteEdges)
                    node.RemoveEdge(deleteEdge);
            }

            foreach (var deletedEdge in allDeletedEdges)
            {
                Undo.RecordObject(deletedEdge, "Delete Node");
                deletedEdge.FirstNode = null;
                deletedEdge.SecondNode = null;
            }

            foreach (var node in allDeletedNodes)
            {
                Undo.DestroyObjectImmediate(node.gameObject);
            }

            foreach (var deletedEdge in allDeletedEdges)
            {
                Undo.DestroyObjectImmediate(deletedEdge.gameObject);
            }
        }

        private void PutNodeInMousePosition()
        {
            var activeNodes = nodeListener.GetActive().ToArray();
            switch (activeNodes.Length)
            {
                case 0:
                    CreateNode();
                    break;
                case 1:
                    var newNode = CreateNode();
                    ConnectNodes(newNode, activeNodes[0]);
                    break;
                case 2:
                    ConnectOrDisconnectNodes(activeNodes[0], activeNodes[1]);
                    break;
                case > 2:
                    Debug.LogError("Too many nodes");
                    break;
            }
        }

        private void ConnectOrDisconnectNodes(TNode firstMonoNode, TNode secondMonoNode)
        {
            var intersect = GetIntersectEdges(firstMonoNode, secondMonoNode);
            if (intersect.Count == 0)
                ConnectNodes(firstMonoNode, secondMonoNode);
            else
                DisconnectNodes(firstMonoNode, secondMonoNode, intersect);
        }

        private TEdge ConnectNodes(TNode firstMonoNode, TNode secondMonoNode)
        {
            Undo.IncrementCurrentGroup();

            var edge = CreateEdge();
            edge.Initialize(GetNextIndex(edge), firstMonoNode, secondMonoNode);


            Undo.RecordObject(firstMonoNode, "ConnectNodes");
            firstMonoNode.AddEdge(edge);
            Undo.RecordObject(secondMonoNode, "ConnectNodes");
            secondMonoNode.AddEdge(edge);

            EditorUtility.SetDirty(firstMonoNode);
            EditorUtility.SetDirty(secondMonoNode);
            EditorUtility.SetDirty(edge);
            RedrawEdgeEditor(edge);
            return edge;
        }

        private TEdge CreateEdge()
        {
            var edge = (TEdge) PrefabUtility.InstantiatePrefab(monoEdgePrefab, graph.EdgesParent.transform);
            Undo.RegisterCreatedObjectUndo(edge.gameObject, "CreateEdge");
            SceneVisibilityManager.instance.DisablePicking(edge.gameObject, false);
            return edge;
        }

        private void DisconnectNodes(TNode firstMonoNode, TNode secondMonoNode, List<TEdge> intersect)
        {
            Undo.IncrementCurrentGroup();
            Undo.RecordObject(firstMonoNode, "DisconnectNodes");
            Undo.RecordObject(secondMonoNode, "DisconnectNodes");

            foreach (var edge in intersect)
            {
                firstMonoNode.RemoveEdge(edge);
                secondMonoNode.RemoveEdge(edge);
                Undo.DestroyObjectImmediate(edge.gameObject);
            }

            EditorUtility.SetDirty(firstMonoNode);
            EditorUtility.SetDirty(secondMonoNode);
        }

        private TNode CreateNode()
        {
            Undo.IncrementCurrentGroup();

            var node = (TNode) PrefabUtility.InstantiatePrefab(monoNodePrefab, graph.NodesParent.transform);
            node.transform.position = GetMousePosition2D();
            node.Initialize(GetNextIndex(node));
            Selection.activeObject = node.gameObject;

            Undo.RegisterCreatedObjectUndo(node.gameObject, "CreateNode");
            EditorUtility.SetDirty(node);

            return node;
        }

        private void UsePositionHandle()
        {
            if (target is GameObject activeObj)
            {
                if (activeObj.GetComponent<TNode>() == null) return;

                EditorGUI.BeginChangeCheck();
                var oldPos = activeObj.transform.position;
                var newPos = Handles.PositionHandle(oldPos, Quaternion.identity);
                var offset = newPos - oldPos;
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (var node in targets
                                 .OfType<GameObject>()
                                 .GetComponentMany<TNode>()
                            )
                    {
                        Undo.RecordObject(node.transform, "Move Node");
                        node.transform.position += offset;
                        ReDrawEdges(node);
                    }
                }
            }
        }

        private void ReDrawEdges(TNode node)
        {
            foreach (var edge in node.Edges)
            {
                RedrawEdgeEditor(edge);
            }
        }


        private List<TEdge> GetIntersectEdges(TNode firstMonoNode, TNode secondMonoNode)
        {
            return firstMonoNode.Edges
                .Intersect(secondMonoNode.Edges)
                .ToList();
        }

        private Vector2 GetMousePosition2D()
        {
            var mousePos = Event.current.mousePosition;
            var mouseX = mousePos.x;
            var mouseY = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePos.y;
            var coord = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(new Vector3(mouseX, mouseY, 0));
            return coord;
        }

        private int GetNextIndex<T>(T obj) where T : Object, INumbered
        {
            var objects = FindObjectsOfType<T>()
                .Where(x => x != obj)
                .OrderBy(x => x.Id);
            var nextIndex = 0;
            foreach (var o in objects)
            {
                if (o.Id == nextIndex)
                    nextIndex++;
                else
                    return nextIndex;
            }

            return nextIndex;
        }

        private void RedrawEdgeEditor(TEdge monoEdge)
        {
            Undo.RecordObject(monoEdge.gameObject, "Redraw Edge");
            monoEdge.name = $"Edge{monoEdge.Id}";
            RedrawLine();
            AlineCollider();

            void RedrawLine()
            {
                var v1 = monoEdge.FirstNode ? monoEdge.FirstNode.Position : Vector2.zero;
                var v2 = monoEdge.SecondNode ? monoEdge.SecondNode.Position : Vector2.right;
                var distance = Vector2.Distance(v1, v2);
                var center = v1;
                var newSecondNodePosition = v2 - center;
                var radian = Mathf.Atan2(newSecondNodePosition.y, newSecondNodePosition.x) * 180 / Mathf.PI;

                Undo.RecordObject(monoEdge.SpriteRenderer.transform, "Redraw Edge");
                monoEdge.SpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, radian);
                monoEdge.SpriteRenderer.transform.position = (v1 + v2) / 2;

                Undo.RecordObject(monoEdge.SpriteRenderer, "Redraw Edge");
                monoEdge.SpriteRenderer.size = new Vector2(distance, monoEdge.SpriteRenderer.size.y);
            }

            void AlineCollider()
            {
                Undo.RecordObject(monoEdge.BoxCollider2D, "Redraw Edge");
                monoEdge.BoxCollider2D.size = monoEdge.SpriteRenderer.size;
            }
        }
    }
}