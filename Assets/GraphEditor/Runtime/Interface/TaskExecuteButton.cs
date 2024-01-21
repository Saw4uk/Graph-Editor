using System;
using System.Collections;
using System.Collections.Generic;
using GraphEditor;
using GraphEditor.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class TaskExecuteButton : MonoBehaviour
{
    [SerializeField] private TaskData taskData;
    [SerializeField] private Button button;
    private void Awake()
    {
        button.onClick.AddListener(() => GraphEditorRoot.Instance.StartGraphEditor(taskData));
    }
}
