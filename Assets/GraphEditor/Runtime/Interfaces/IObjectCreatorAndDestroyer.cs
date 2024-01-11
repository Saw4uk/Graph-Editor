using UnityEngine;

namespace GraphEditor.Runtime
{
    public interface IObjectCreatorAndDestroyer
    {
        TObj CreateInstance<TObj>(TObj obj) where TObj : Object;
        TObj CreateInstance<TObj>(TObj obj, Transform parent) where TObj : Object;
        void ReleaseInstance<TObj>(TObj obj) where TObj : Object;
    }
}