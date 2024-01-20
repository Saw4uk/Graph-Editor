using System;
using System.Collections;
using GraphEditor.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GraphEditor
{
    [CreateAssetMenu(menuName = "Graph Generators/Random Graph Generator")]
    public class RandomGraphCreator  : GraphGenerator
    {
        [Header("Nodes Settings")] 
        [SerializeField, Min(1)] private int minNodesCount;
        [SerializeField, Min(1)] private int maxNodesCount;

        [Header("Edges Settings")] 
        [SerializeField, Min(1)] private int minEdgesCount;
        [SerializeField, Min(1)] private int maxEdgesCount;
        
        [SerializeField] private InitializeGraphSettings initializeGraphSettings;
        [SerializeField] private bool isDebug;

        private MonoBehaviour coroutineObj;
        private LineWarsGameGraphCreator randomGraphCreator;
        private MonoGraph monoGraph;
        
        public override MonoGraph Generate()
        {
            var randomNodesCount = Random.Range(minNodesCount, maxNodesCount);
            var randomEdgesRange = CustomMath.CreateRandomRange(minEdgesCount, maxEdgesCount);
            
            randomGraphCreator = GraphEditorRoot.Instance.RandomGraphCreator;
            
            var undirectedGraph = UndirectedVertexGraph.GenerateRandomGraph(randomNodesCount, randomEdgesRange);
            monoGraph = randomGraphCreator.Restart(undirectedGraph);
            
            if (isDebug)
                randomGraphCreator.DrawBorder();
            
            monoGraph.name = "Random Graph";
            
            coroutineObj = new GameObject().AddComponent<RoutineObject>();
            coroutineObj.StartCoroutine(IterateGraph());
            
            return monoGraph;
        }

        private IEnumerator IterateGraph()
        {
            for (var i = 0; i < initializeGraphSettings.IterationCountBeforeDeleteEdges; i++)
            {
                yield return null;
                randomGraphCreator.Iterate();
                monoGraph.RedrawAllEdges();
                
                GraphEditorRoot.Instance.CameraController.MoveTo(monoGraph.GetBounds());
            }

            randomGraphCreator.DeleteIntersectingEdgesByIntersectionsCount();

            for (var i = 0; i < initializeGraphSettings.IterationCountAfterDeleteEdges; i++)
            {
                yield return null;
                randomGraphCreator.Iterate();
                monoGraph.RedrawAllEdges();
                
                GraphEditorRoot.Instance.CameraController.MoveTo(monoGraph.GetBounds());
            }

            FinishGenerate();
            Destroy(coroutineObj);
        }
    }
}