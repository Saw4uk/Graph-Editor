using System.Collections.Generic;
using System.Linq;
using GraphEditor.Runtime;

namespace GraphEditor
{
    public class QuestionTask : BaseTask<QuestionTask>
    {
        private readonly List<BaseQuestion> questions;

        public List<BaseQuestion> Questions => questions;
    
        public QuestionTask(List<BaseQuestion> questions, TaskInfo taskInfo) : base(taskInfo)
        {
            this.questions = questions;
        }
    
        public override float CheckTask(MonoGraph monoGraph)
        {
            return GetMark(questions.Select(x => x.Check(monoGraph)));
        }
    }
}
