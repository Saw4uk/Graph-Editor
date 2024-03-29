﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraphEditor.Runtime
{
    public class TaskController : MonoBehaviour
    {
        private string taskTitleText = "Ваш граф соответствует критериям:";
        [SerializeField] private TMP_Text titleField;
        [SerializeField] private Button checkButton;
        [SerializeField] private GameObject taskContainer;
        [SerializeField] private QuestionDrawer questionDrawerPrefab;
        [SerializeField] private TMP_Text descriptionFieldPrefab;
        [SerializeField] private FinalWindow finalWindow;

        private ITask currentTask;

        public void Initialize(ITask taskData)
        {
            var questionDrawers = taskContainer.GetComponentsInChildren<QuestionDrawer>();

            foreach (var questionDrawer in questionDrawers)
            {
                Destroy(questionDrawer.gameObject);
            }
            
            
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
            
            titleField.text = currentTask.TaskInfo.Description;
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

            var result = currentTask.CheckTask(GraphEditorRoot.Instance.MonoGraph);
            
            finalWindow.ShowWindow(result);
        }
    }
}
