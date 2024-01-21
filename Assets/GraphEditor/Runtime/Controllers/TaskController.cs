using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraphEditor.Runtime
{
    public class TaskController : MonoBehaviour
    {
        [SerializeField] private string taskTitleText = "Необходимо построить дерево удовлетворяющее требованиям:";
        [SerializeField] private TMP_Text titleField;
        [SerializeField] private Button checkButton;
        [SerializeField] private GameObject taskContainer;
        [SerializeField] private QuestionDrawer questionDrawerPrefab;
        [SerializeField] private TMP_Text descriptionFieldPrefab;

        private ITask currentTask;

        public void Initialize(ITask taskData)
        {
            currentTask = taskData;

            if (currentTask is QuestionTask questionTask)
            {
                foreach (var question in questionTask.Questions)
                {
                    var questionDrawer = Instantiate(questionDrawerPrefab, taskContainer.transform);
                    questionDrawer.DrawQuestion(question);
                }
            }
            else
            {
                Instantiate(descriptionFieldPrefab, taskContainer.transform);
                descriptionFieldPrefab.text = currentTask.GetDescription();
            }
            
            titleField.text = taskTitleText;
            checkButton.onClick.AddListener(CheckTaskOnButtonClick);
        }

        private void CheckTaskOnButtonClick()
        {
            if (currentTask is QuestionTask)
            {
                var questionDrawers = taskContainer.GetComponentsInChildren<QuestionDrawer>();
                foreach (var questionDrawer in questionDrawers)
                {
                    questionDrawer.SetInputDataInQuestion();
                }
            }
            
            Debug.Log(currentTask.CheckTask(GraphEditorRoot.Instance.MonoGraph) ? "Верно" : "Неверно");
        }
    }
}
