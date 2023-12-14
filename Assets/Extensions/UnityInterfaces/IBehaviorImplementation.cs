using UnityEngine;
using Object = UnityEngine.Object;


public interface IBehaviorImplementation : IComponentImplementation
{
    bool enabled { get; set; }
    bool isActiveAndEnabled { get; }
}