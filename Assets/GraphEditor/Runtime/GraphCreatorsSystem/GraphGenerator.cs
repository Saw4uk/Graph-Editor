using System;
using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    public abstract class GraphGenerator : ScriptableObject
    {
        public event Action GenerateIsFinished;

        protected void FinishGenerate()
        {
            GenerateIsFinished?.Invoke(); 
        }
        
        public abstract MonoGraph Generate();
    }
}