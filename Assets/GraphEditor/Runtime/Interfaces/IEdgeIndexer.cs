namespace GraphEditor.Runtime
{
    public interface IEdgeIndexer
    {
        MonoEdge this[int id] { get; set; }
    }
}
