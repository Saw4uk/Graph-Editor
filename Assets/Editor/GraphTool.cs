using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using Object = UnityEngine.Object;


[EditorTool("CreateGraph")]
public class GraphTool : EditorTool
{
    private Edge edgePrefab;
    private Node nodePrefab;
    private MonoGraph graph;


    private SelectionListener<Node> nodeListener;

    public override void OnActivated()
    {
        base.OnActivated();

        edgePrefab = Resources.Load<Edge>("Prefabs/Edge");
        nodePrefab = Resources.Load<Node>("Prefabs/Node");

        AssignGraph();

        foreach (var gameObject in FindObjectsOfType<GameObject>())
            SceneVisibilityManager.instance.DisablePicking(gameObject, false);

        SceneVisibilityManager.instance.EnablePicking(graph.gameObject, false);
        SceneVisibilityManager.instance.EnablePicking(graph.NodesParent.gameObject, true);

        EditorApplication.RepaintHierarchyWindow();

        nodeListener = new SelectionListener<Node>();
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
        DrawOutlineForActiveNodes();
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
        graph = graphObj.GetComponent<MonoGraph>() ??
                graphObj.AddComponent<MonoGraph>();

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

    private void DrawOutlineForActiveNodes()
    {
        foreach (var activatedNode in nodeListener.GetActivated())
        {
            activatedNode.SetActiveOutline(true);
        }

        foreach (var disableNode in nodeListener.GetDisabled())
        {
            disableNode.SetActiveOutline(false);
        }
    }

    private void DeleteSelectedNodes()
    {
        Debug.Log("DELETE");
        var allDeletedEdges = new List<Edge>();
        var allDeletedNodes = new List<Node>();
        var allNeighboringNodes = new List<Node>();

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

    private void ConnectManyNode(Node[] nodes)
    {
        var allPairs = nodes.Zip(nodes, (node1, node2) => (node1, node2))
            .Where(x => x.node1.GetLine(x.node2));
    }


    private void ConnectOrDisconnectNodes(Node firstNode, Node secondNode)
    {
        var intersect = GetIntersectEdges(firstNode, secondNode);
        if (intersect.Count == 0)
            ConnectNodes(firstNode, secondNode);
        else
            DisconnectNodes(firstNode, secondNode, intersect);
    }

    private Edge ConnectNodes(Node firstNode, Node secondNode)
    {
        Undo.IncrementCurrentGroup();

        var edge = CreateEdge();
        edge.Initialize(GetNextIndex(edge), firstNode, secondNode);


        Undo.RecordObject(firstNode, "ConnectNodes");
        firstNode.AddEdge(edge);
        Undo.RecordObject(secondNode, "ConnectNodes");
        secondNode.AddEdge(edge);

        EditorUtility.SetDirty(firstNode);
        EditorUtility.SetDirty(secondNode);
        EditorUtility.SetDirty(edge);
        edge.Redraw();
        return edge;
    }

    private Edge CreateEdge()
    {
        var edge = (Edge) PrefabUtility.InstantiatePrefab(edgePrefab, graph.EdgesParent.transform);
        Undo.RegisterCreatedObjectUndo(edge.gameObject, "CreateEdge");
        SceneVisibilityManager.instance.DisablePicking(edge.gameObject, false);
        return edge;
    }

    private void DisconnectNodes(Node firstNode, Node secondNode, List<Edge> intersect)
    {
        Undo.IncrementCurrentGroup();
        Undo.RecordObject(firstNode, "DisconnectNodes");
        Undo.RecordObject(secondNode, "DisconnectNodes");

        foreach (var edge in intersect)
        {
            firstNode.RemoveEdge(edge);
            secondNode.RemoveEdge(edge);
            Undo.DestroyObjectImmediate(edge.gameObject);
        }

        EditorUtility.SetDirty(firstNode);
        EditorUtility.SetDirty(secondNode);
    }

    private Node CreateNode()
    {
        Undo.IncrementCurrentGroup();

        var node = (Node) PrefabUtility.InstantiatePrefab(nodePrefab, graph.NodesParent.transform);
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
            if (activeObj.GetComponent<Node>() == null) return;

            EditorGUI.BeginChangeCheck();
            var oldPos = activeObj.transform.position;
            var newPos = Handles.PositionHandle(oldPos, Quaternion.identity);
            var offset = newPos - oldPos;
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var node in targets
                             .OfType<GameObject>()
                             .GetComponentMany<Node>()
                        )
                {
                    Undo.RecordObject(node.transform, "Move Node");
                    node.transform.position += offset;
                    ReDrawEdges(node);
                }
            }
        }
    }

    private void ReDrawEdges(Node node)
    {
        foreach (var edge in node.Edges)
        {
            Undo.RecordObject(edge.gameObject, "Move Node");
            Undo.RecordObject(edge.transform, "Move Node");
            Undo.RecordObject(edge.SpriteRenderer, "Move Node");
            Undo.RecordObject(edge.BoxCollider2D, "Move Node");
            edge.Redraw();
        }
    }


    private List<Edge> GetIntersectEdges(Node firstNode, Node secondNode)
    {
        return firstNode.Edges
            .Intersect(secondNode.Edges)
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
}


public static class EdgeEditorExtension
{
    public static void Redraw(this Edge edge)
    {
        Undo.RecordObject(edge.gameObject, "Redraw Edge");
        edge.name = $"Edge{edge.Id}";
        RedrawLine();
        AlineCollider();

        void RedrawLine()
        {
            var v1 = edge.FirstNode ? edge.FirstNode.Position : Vector2.zero;
            var v2 = edge.SecondNode ? edge.SecondNode.Position : Vector2.right;
            var distance = Vector2.Distance(v1, v2);
            var center = v1;
            var newSecondNodePosition = v2 - center;
            var radian = Mathf.Atan2(newSecondNodePosition.y, newSecondNodePosition.x) * 180 / Mathf.PI;

            Undo.RecordObject(edge.SpriteRenderer.transform, "Redraw Edge");
            edge.SpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, radian);
            edge.SpriteRenderer.transform.position = (v1 + v2) / 2;

            Undo.RecordObject(edge.SpriteRenderer, "Redraw Edge");
            edge.SpriteRenderer.size = new Vector2(distance, edge.SpriteRenderer.size.y);
        }

        void AlineCollider()
        {
            Undo.RecordObject(edge.BoxCollider2D, "Redraw Edge");
            edge.BoxCollider2D.size = edge.SpriteRenderer.size;
        }
    }
}