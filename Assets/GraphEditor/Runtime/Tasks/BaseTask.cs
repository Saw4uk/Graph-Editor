using System;
using System.Linq;
using System.Reflection;
using GraphEditor.Attributes;
using UnityEngine;

namespace GraphEditor.Runtime
{
    public abstract class BaseTask: ScriptableObject, ITask
    {
        public abstract string GetDescription();
        public abstract bool CheckTask();
    }
    
    public abstract class BaseTask<T> : BaseTask
        where T : BaseTask<T>
    {
        private static readonly PropertyInfo[] properties;

        static BaseTask()
        {
            properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(property => property.GetCustomAttribute<DisplayNameAttribute>() != null)
                .ToArray();
        }

        public override string GetDescription()
        {
            var stringProperties = properties.Select(property =>
                $"{property.GetCustomAttribute<DisplayNameAttribute>().DisplayName} - {property.GetValue(this)}");
            return string.Join("\n", stringProperties);
        }
    }
}