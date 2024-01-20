using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GraphEditor
{
    [Serializable]
    public class InitializeGraphSettings
    {
        [SerializeField] private int iterationCountBeforeDeleteEdges = 1000;
        [SerializeField] private int iterationCountAfterDeleteEdges = 50;
        
        public int IterationCountBeforeDeleteEdges
        {
            get => iterationCountBeforeDeleteEdges;
            set => iterationCountBeforeDeleteEdges = value;
        }
        
        public int IterationCountAfterDeleteEdges
        {
            get => iterationCountAfterDeleteEdges;
            set => iterationCountAfterDeleteEdges = value;
        }
    }
}