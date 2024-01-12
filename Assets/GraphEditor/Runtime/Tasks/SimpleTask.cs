using System;
using GraphEditor.Attributes;
using UnityEngine;
using Random = System.Random;

namespace GraphEditor.Runtime
{
    [CreateAssetMenu(fileName = "SimpleTask", menuName = "ScriptableObjects/SpawnManagerScriptableObject")]
    public class SimpleTask : BaseTask<SimpleTask>
    {
        [DisplayName("Количество листьев")]
        private int NumberLeaves {
            get
            {
                var random = new Random();
                return random.Next(6, 18);
            }
        }

        [DisplayName("Глубина")] 
        private int Depth => (int)Math.Floor(Math.Log(NumberLeaves, 2));
    
        public override bool CheckTask(MonoGraph graph)
        {
            return true;
        }
    }
}
