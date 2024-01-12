using System;
using System.Linq;
using System.Reflection;
using GraphEditor.Attributes;
using UnityEngine;

namespace GraphEditor.Runtime
{
    public abstract class BaseTask<T> : ScriptableObject, ITask
        where T : BaseTask<T>
    {
        private static readonly PropertyInfo[] properties;

        static BaseTask()
        {
            properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetCustomAttribute<DisplayNameAttribute>() != null)
                .ToArray();
        }

        public string GetDescription()
        {
            var stringProperties = properties.Select(property =>
                $"{property.GetCustomAttribute<DisplayNameAttribute>().DisplayName} - {property.GetValue(this)}");
            return string.Join("\n", stringProperties);
        }

        public abstract bool CheckTask(MonoGraph monoGraph);
    }
}