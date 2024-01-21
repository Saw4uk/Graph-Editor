using GraphEditor.Runtime;
using UnityEditor;
using UnityEngine;
using Undo = UnityEditor.Undo;

namespace GraphEditor
{
    [CustomEditor(typeof(LineWarsGameGraphCreator))]
    public class GraphCreatorEditor : Editor
    {
        private LineWarsGameGraphCreator LineWarsGameGraphCreator => (LineWarsGameGraphCreator)target;
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
                LineWarsGameGraphCreator.Iterate();
                LineWarsGameGraphCreator.RedrawAllEdges();
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

                LineWarsGameGraphCreator.Restart();
                LineWarsGameGraphCreator.RedrawAllEdges();
                LineWarsGameGraphCreator.DrawBorder();
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

            if (GUILayout.Button("Delete Intersects by Count"))
            {
                Undo.IncrementCurrentGroup();
                LineWarsGameGraphCreator.DeleteIntersectingEdgesByIntersectionsCount();
            }

            if (GUILayout.Button("Delete Intersects by Length"))
            {
                Undo.IncrementCurrentGroup();
                LineWarsGameGraphCreator.DeleteIntersectingEdgesByLength();
            }
        }
    }
}