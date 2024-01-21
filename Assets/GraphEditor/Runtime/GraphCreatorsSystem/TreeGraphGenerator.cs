using System.Collections;
using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    [CreateAssetMenu(menuName = "Graph Generators/Tree Graph Generator")]
    public class TreeGraphGenerator : GraphGenerator
    {
        [Header("Layers Settings")] 
        [SerializeField, Min(1)] private int minLayersCount;
        [SerializeField, Min(1)]private int maxLayersCount;

        [Header("Heirs Settings")] 
        [SerializeField, Min(1)] private int minHeirsCount;
        [SerializeField, Min(1)] private int maxHeirsCount;

        private MonoBehaviour coroutineObj;
        private MonoGraph monoGraph;

        public override MonoGraph Generate()
        {
            var randomLayerCount = Random.Range(minLayersCount, maxLayersCount);
            var randomHeirsRange = CustomMath.CreateRandomRange(minHeirsCount, maxHeirsCount);

            var treeCreator = GraphEditorRoot.Instance.TreeCreator;
            var tree = Tree.CreateRandomTree(randomLayerCount, new Vector2Int(randomHeirsRange.Item1, randomHeirsRange.Item2));
            monoGraph = treeCreator.Restart(tree, Vector2.zero);
            monoGraph.name = "Tree Graph";

            if (coroutineObj == null)
                coroutineObj = new GameObject().AddComponent<RoutineObject>();
            coroutineObj.StopAllCoroutines();
            coroutineObj.StartCoroutine(RoutineGenerate(treeCreator));
            
            return monoGraph;
        }

        private IEnumerator RoutineGenerate(TreeCreator treeCreator)
        {
            var graphIsGenerated = true;
            while (graphIsGenerated)
            {
                yield return new WaitForSeconds(0.3f);
                
                graphIsGenerated = treeCreator.Iterate();
                monoGraph.RedrawAllEdges();
                
                GraphEditorRoot.Instance.CameraController.MoveTo(monoGraph.GetBounds());
            }

            FinishGenerate();
            Destroy(coroutineObj.gameObject);
        }
    }
}