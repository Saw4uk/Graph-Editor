using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraphEditor.Runtime
{
    public class TaskController : MonoBehaviour
    {
        [SerializeField] private ScriptableObject taskObject;
        [SerializeField] private string taskTitleText = "Необходимо построить дерево удовлетворяющее требованиям:";
        [SerializeField] private TMP_Text titleField;
        [SerializeField] private TMP_Text descriptionField;
        [SerializeField] private Button checkButton;

        private ITask task;
    
        void Awake()
        {
            task = taskObject as ITask;

            if (task == null)
                throw new ArgumentException($"Task is not an implementation of {typeof(ITask)}");
        
            titleField.text = taskTitleText;
            descriptionField.text = task.GetDescription();
            checkButton.onClick.AddListener(CheckTaskOnButtonClick);
        }

        private void CheckTaskOnButtonClick()
        {
            Debug.Log(task.CheckTask(GraphTool.Instance.Graph) ? "Верно" : "Неверно");
        }
    }
}
