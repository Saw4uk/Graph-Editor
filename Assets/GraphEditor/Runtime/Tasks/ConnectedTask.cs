using GraphEditor.Attributes;

namespace GraphEditor.Runtime
{
    public class ConnectedTask : BaseTask<ConnectedTask>
    {
        private bool shouldBeConnected;
        public ConnectedTask(bool shouldBeConnected, TaskInfo taskInfo) : base(taskInfo)
        {
            this.shouldBeConnected = shouldBeConnected;
        }

        [DisplayName("Должен ли быть связным")]
        public bool ShouldBeConnected => shouldBeConnected;
        public override float CheckTask(MonoGraph monoGraph)
        {
            var isConnected = monoGraph.CheckGraphForConnectivity();
            return GetMark(isConnected == shouldBeConnected);
        }
    }
}
