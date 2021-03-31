using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Discord.Commands;

namespace Templar
{
    internal class EventService
    {
        public List<RegisteredEvent> RegisteredEvents;

        public EventService()
        {
            Console.WriteLine("Loading Events...");
            var methods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .SelectMany(t => t.GetMethods())
                .Where(m => m.IsStatic && m.IsPublic)
                .ToList();

            RegisteredEvents = new List<RegisteredEvent>();
            foreach (var m in methods)
            {
                foreach (var e in m.GetCustomAttributes<EventAttribute>())
                {
                    var priority = 0;
                    if (m.HasAttribute<PriorityAttribute>(out var att))
                    {
                        priority = -att.Priority;
                    }

                    RegisteredEvents.Add(new RegisteredEvent()
                    {
                        EventName = e.EventName,
                        Priority = priority,
                        ToExecute = m,
                    });
                }
            }

            RegisteredEvents = RegisteredEvents.OrderBy(e => e.Priority).ToList();
            Console.WriteLine("Done Loading Events");
        }

        public int OnEvent(string name, params object[] args)
        {
            var executed = 0;
            foreach (var e in RegisteredEvents)
            {
                if (e.EventName != name) { continue; }

                e.Run(args);
            }
            return executed;
        }
    }

    internal class RegisteredEvent
    {
        public string EventName;
        public MethodInfo ToExecute;
        public int Priority;

        public bool Run(params object[] args)
        {
            try
            {
                ToExecute?.Invoke(null, args);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
