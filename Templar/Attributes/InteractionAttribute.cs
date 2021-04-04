using System;

namespace Templar
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InteractionAttribute : Attribute
    {
        public string Name { get; }
        public InteractionAttribute(string name) => Name = name;
    }
}
