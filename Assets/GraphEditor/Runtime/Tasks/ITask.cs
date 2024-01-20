namespace GraphEditor.Runtime
{
    public interface ITask
    {
        string GetDescription();
        TaskInfo TaskInfo { get; }
        bool CheckTask(MonoGraph monoGraph);
    }
}
