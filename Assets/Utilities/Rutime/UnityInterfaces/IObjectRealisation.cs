using UnityEngine;
using Object = UnityEngine.Object;

public interface IObjectImplementation
{
    int GetInstanceID();
    string name { get; set; }
    HideFlags hideFlags { get; set; }
}