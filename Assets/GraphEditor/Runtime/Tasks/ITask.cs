namespace GraphEditor.Runtime
{
    public interface ITask
    {
        string GetDescription();
        bool CheckTask();
    }
}
