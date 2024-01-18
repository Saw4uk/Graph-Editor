using GraphEditor.Runtime;
using UnityEditor;
using UnityEngine;
using Undo = UnityEditor.Undo;

namespace GraphEditor.Editor
{
    [CustomEditor(typeof(GraphCreator))]
    public class GraphCreatorEditor : UnityEditor.Editor
    {
        private GraphCreator GraphCreator => (GraphCreator)target;
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
                GraphCreator.SimpleIterate();
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
                GraphCreator.DrawBorder();
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
                GraphCreator.DeleteIntersectingEdgesByIntersectionsCount();
            }

            if (GUILayout.Button("Delete Intersects by Length"))
            {
                Undo.IncrementCurrentGroup();
                GraphCreator.DeleteIntersectingEdgesByLength();
            }
        }
    }
}