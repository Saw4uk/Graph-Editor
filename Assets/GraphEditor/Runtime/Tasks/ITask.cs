namespace GraphEditor.Runtime
{
    public interface ITask
    {
        string GetDescription();
        TaskInfo TaskInfo { get; }
        float CheckTask(MonoGraph monoGraph);
    }
}
