using System.Collections.Generic;
using GraphEditor.Runtime;
using UnityEngine;

namespace GraphEditor
{
    [CreateAssetMenu(fileName = "New Question Task", menuName = "Tasks/Question")]
    public class QuestionTaskData : TaskData
    {
        [SerializeField]
        private List<BaseQuestion> questions;

        public override ITask GetNewTask()
        {
            return new QuestionTask(questions, taskInfo);
        }
    }
}