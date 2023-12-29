using System;
using System.Collections;
using GraphEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GraphCreator))]
public class GraphCreatorEditor : Editor
{
    private GraphCreator GraphCreator => (GraphCreator) target;
    private bool iterateStarted;

    private void OnEnable()
    {
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Update;
    }

    private void Update()
    {
        if (iterateStarted)
        {
            GraphCreator.Iterate();
            GraphCreator.RedrawAllEdges();
        }
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Restart"))
        {
            foreach (var g in FindObjectsOfType<MonoGraph>())
            {
                DestroyImmediate(g.gameObject);
            }

            GraphCreator.Restart();
            GraphCreator.RedrawAllEdges();
            iterateStarted = false;
        }

        if (GUILayout.Button("Start Iterate"))
        {
            iterateStarted = true;
        }

        if (GUILayout.Button("Stop Iterate"))
        {
            iterateStarted = false;
        }
        
        if (GUILayout.Button("Single Iterate"))
        {
            GraphCreator.Iterate();
            GraphCreator.RedrawAllEdges();
        }
    }
}