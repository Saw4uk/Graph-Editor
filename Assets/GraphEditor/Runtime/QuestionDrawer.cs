using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GraphEditor.Runtime
{
    public class QuestionDrawer : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Toggle checkBox;
        [SerializeField] private TMP_Text text;

        private BaseQuestion question;

        private void Awake()
        {
            inputField.gameObject.SetActive(false);
            checkBox.gameObject.SetActive(false);
        }

        public void DrawQuestion(BaseQuestion question)
        {
            if (question is IInputQuestion inputQuestion)
                switch (inputQuestion.InputQuestionType)
                {
                    case InputQuestionType.Int:
                        inputField.gameObject.SetActive(true);
                        break;
                    case InputQuestionType.Checkbox:
                        checkBox.gameObject.SetActive(true);
                        break;
                }
            
            text.text = question.GetQuestionText;
            this.question = question;
        }

        public void SetInputDataInQuestion()
        {
            if (question is IInputQuestion inputQuestion)
                switch (inputQuestion.InputQuestionType)
                {
                    case InputQuestionType.Int:
                        inputQuestion.TrySetInputData(inputField.text);
                        break;
                    case InputQuestionType.Checkbox:
                        inputQuestion.TrySetInputData(checkBox.isOn.ToString());
                        break;
                }
        }
    }
}
