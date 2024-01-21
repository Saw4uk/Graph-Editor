using GraphEditor;
using GraphEditor.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.GraphEditor.Runtime.Tasks
{
    public class TreeDepthTask : BaseTask<TreeDepthTask>
    {
        public TreeDepthTask(TaskInfo taskInfo) : base(taskInfo)
        {
        }

        public override float CheckTask(MonoGraph monoGraph)
        {
            throw new NotImplementedException();
        }
    }
}
