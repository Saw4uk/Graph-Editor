using System;
using GraphEditor.Runtime;
using UnityEditor;
using UnityEngine;

namespace GraphEditor
{
    [CustomEditor(typeof(TreeCreator))]
    public class TreeCreatorEditor: Editor
    {
        private TreeCreator TreeCreator => (TreeCreator) target;
        private MonoGraph graph;
        
        private bool startIterate;
        
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
            if (startIterate)
            {
                TreeCreator.Iterate(out var rect);
                graph.RedrawAllEdges();
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
                graph = TreeCreator.Restart();
            }
            
            if (GUILayout.Button("SingleIterate"))
            {
                TreeCreator.Iterate(out var rect);
                graph.RedrawAllEdges();
            }
            
            if (GUILayout.Button("Start Iterate"))
            {
                startIterate = true;
            }

            if (GUILayout.Button("Stop Iterate"))
            {
                startIterate = false;
            }
        }
    }
}