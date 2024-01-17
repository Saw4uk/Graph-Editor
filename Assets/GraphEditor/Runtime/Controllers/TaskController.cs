using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraphEditor.Runtime
{
    public class TaskController : MonoBehaviour
    {
        [SerializeField] private TaskData taskData;
        [SerializeField] private string taskTitleText = "Необходимо построить дерево удовлетворяющее требованиям:";
        [SerializeField] private TMP_Text titleField;
        [SerializeField] private TMP_Text descriptionField;
        [SerializeField] private Button checkButton;

        private ITask currentTask;

        void Awake()
        {
            if (taskData == null)
                throw new ArgumentException("No Task Data!");
            currentTask = taskData.GetNewTask();
            titleField.text = taskTitleText;
            descriptionField.text = currentTask.GetDescription();
            checkButton.onClick.AddListener(CheckTaskOnButtonClick);
        }

        private void CheckTaskOnButtonClick()
        {
            Debug.Log(currentTask.CheckTask(GraphTool.Instance.Graph) ? "Верно" : "Неверно");
        }
    }
}
