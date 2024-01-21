using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using GraphEditor.Attributes;

namespace GraphEditor.Runtime
{
    public abstract class BaseTask<T> : BaseTask
        where T : BaseTask<T>
    {
        private static DisplayNameAttribute[] displayNameAttributes;
        private static readonly PropertyInfo[] properties;

        static BaseTask()
        {
            properties = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetCustomAttribute<DisplayNameAttribute>() != null)
                .ToArray();
            displayNameAttributes = properties
                .Select(property => property.GetCustomAttribute<DisplayNameAttribute>())
                .ToArray();
        }

        protected BaseTask(TaskInfo taskInfo) : base(taskInfo)
        {
        }
        
        public override string GetDescription()
        {
            // var stringProperties = properties.Select(property =>
            //    $"{property.GetCustomAttribute<DisplayNameAttribute>().DisplayName} - {property.GetValue(this)}");

            var stringProperties = new StringBuilder();
            for(var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var attribute = displayNameAttributes[i];
                if (property.GetType() == typeof(bool))
                {
                    var result = (bool)property.GetValue(this) ? "Да" : "Нет";
                    stringProperties.Append($"{attribute.DisplayName} - {result}");

                }
                else
                    stringProperties.Append(
                        $"{property.GetCustomAttribute<DisplayNameAttribute>().DisplayName} - {property.GetValue(this)}");
                if (i != properties.Length - 1)
                    stringProperties.Append("\n");
            }
            return stringProperties.ToString();
        }
    }

    public abstract class BaseTask : ITask
    {
        private const float MAX_MARK = 100;

        public static float MaxMark => MAX_MARK;
        public static float GetMark(IEnumerable<bool> conditions)
        {
            var coefficient = (float)conditions.Count(x => x) / conditions.Count();
            return (float)Math.Round(MaxMark * coefficient, 1);
        }

        public static float GetMark(params bool[] conditions)
            => GetMark(conditions);
        
        public TaskInfo TaskInfo { get; }

        protected BaseTask(TaskInfo taskInfo)
        {
            TaskInfo = taskInfo;
        }
        
        public abstract string GetDescription();

        public abstract float CheckTask(MonoGraph monoGraph);
    }
}