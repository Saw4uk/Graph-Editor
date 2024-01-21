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
        public bool IsEditable => CurrentTask.TaskInfo.IsEditable;

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

            DisableInteraction();
            
            CurrentTask.TaskInfo.GraphGenerator.GenerateIsFinished += ReturnInteraction;
        }

        public void RegenerateGraph()
        {
            Destroy(MonoGraph.gameObject);
            Undo.Clear();
            nodeSelector.Clear();
            DisableInteraction();

            MonoGraph = CurrentTask.TaskInfo.GraphGenerator.Generate();
            GraphTool.Graph = MonoGraph;
        }

        public MonoNode GetRootNode()
        {
            if (CurrentTask.TaskInfo.GraphGenerator is TreeGraphGenerator treeGraphGenerator)
                return treeGraphGenerator.RootNode;
            return null;
        }

        private void ReturnInteraction()
        {
            nodeSelector.isEditable = IsEditable;
            selectionAreaController.isEditable = IsEditable;
            graphTool.isEditable = IsEditable;
        }

        private void DisableInteraction()
        {
            nodeSelector.isEditable = false;
            selectionAreaController.isEditable = false;
            graphTool.isEditable = false;
        }
    }
}