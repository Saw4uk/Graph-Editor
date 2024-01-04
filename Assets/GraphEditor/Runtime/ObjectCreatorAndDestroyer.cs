using UnityEngine;

namespace GraphEditor
{
    public class ObjectCreatorAndDestroyer: IObjectCreatorAndDestroyer
    {
        public TObj CreateInstance<TObj>(TObj obj) where TObj : Object
        {
            return Object.Instantiate(obj);
        }

        public TObj CreateInstance<TObj>(TObj obj, Transform parent) where TObj : Object
        {
            return Object.Instantiate(obj, parent);
        }

        public void ReleaseInstance<TObj>(TObj obj) where TObj : Object
        {
            Object.Destroy(obj);
        }
    }
}