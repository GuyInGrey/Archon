using System;

namespace Templar
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : Attribute
    {
        public string EventName;

        public EventAttribute(Events e) => EventName = e.ToString();
    }
}
