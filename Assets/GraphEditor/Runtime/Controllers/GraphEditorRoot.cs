using System;
using DataStructures;
using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    public class GraphEditorRoot : Singleton<GraphEditorRoot>
    {
        [SerializeField] private TaskData taskData;

        [Header("Controllers")] 
        [SerializeField] private CameraController cameraController;
        [SerializeField] private NodeSelector nodeSelector;
        [SerializeField] private SelectionAreaController selectionAreaController;
        [SerializeField] private GraphTool graphTool;
        [SerializeField] private TaskController taskController;
        [SerializeField] private LineWarsGameGraphCreator randomGraphCreator;
        [SerializeField] private TreeCreator treeCreator;
        
        public GraphTool GraphTool => graphTool;

        public CameraController CameraController => cameraController;

        public NodeSelector NodeSelector => nodeSelector;

        public SelectionAreaController SelectionAreaController => selectionAreaController;

        public TaskController TaskController => taskController;
        public LineWarsGameGraphCreator RandomGraphCreator => randomGraphCreator;
        public TreeCreator TreeCreator => treeCreator;
        public MonoGraph MonoGraph { get; set; }
        public ITask CurrentTask { get; set; }

        private void Start()
        {
            StartGraphEditor();
        }

        private void StartGraphEditor()
        {
            CurrentTask = taskData.GetNewTask();
            taskController.Initialize(CurrentTask);

            MonoGraph = CurrentTask.TaskInfo.GraphGenerator.Generate();
            
            cameraController.Initialize();

            nodeSelector.Initialize();
            selectionAreaController.Initialize();
            graphTool.Initialize(MonoGraph, NodeSelector);
        }

        public void RegenerateGraph()
        {
            Destroy(MonoGraph.gameObject);
            Undo.Clear();
            
            MonoGraph = CurrentTask.TaskInfo.GraphGenerator.Generate();
            GraphTool.Graph = MonoGraph;
        }
    }
}