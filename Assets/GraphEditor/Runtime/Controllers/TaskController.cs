using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraphEditor.Runtime
{
    public class TaskController : MonoBehaviour
    {
        [SerializeField] private BaseTask taskObject;
        [SerializeField] private string taskTitleText = "Необходимо построить дерево удовлетворяющее требованиям:";
        [SerializeField] private TMP_Text titleField;
        [SerializeField] private TMP_Text descriptionField;
        [SerializeField] private Button checkButton;

        void Awake()
        {
            if (taskObject == null)
                throw new ArgumentException($"Task is not an implementation of {typeof(BaseTask)}");
        
            titleField.text = taskTitleText;
            descriptionField.text = taskObject.GetDescription();
            checkButton.onClick.AddListener(CheckTaskOnButtonClick);
        }

        private void CheckTaskOnButtonClick()
        {
            Debug.Log(taskObject.CheckTask(GraphTool.Instance.Graph) ? "Верно" : "Неверно");
        }
    }
}
