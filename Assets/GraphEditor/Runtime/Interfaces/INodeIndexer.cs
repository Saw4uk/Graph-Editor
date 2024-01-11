namespace GraphEditor.Runtime
{
    public interface INodeIndexer
    {
        MonoNode this[int id] { get; set; }
    }
}