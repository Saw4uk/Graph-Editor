using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    [CreateAssetMenu(menuName = "Graph Generators/Empty Graph Generator")]
    public class EmptyGraphGenerator : GraphGenerator
    {
        [SerializeField] private MonoGraph monoGraphPrefab;
        
        public override MonoGraph Generate()
        {
            var monoGraph = Instantiate(monoGraphPrefab);
            monoGraph.name = "Empty Graph";
            
            FinishGenerate();
            return monoGraph;
        }
    }
}