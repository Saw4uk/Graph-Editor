using UnityEngine;

namespace GraphEditor
{
    public interface IObjectCreatorAndDestroyer
    {
        public TObj CreateInstance<TObj>(TObj obj) where TObj : Object;
        public TObj CreateInstance<TObj>(TObj obj, Transform parent) where TObj : Object;
        public void ReleaseInstance<TObj>(TObj obj) where TObj : Object;
    }
}