
using System.Reflection;
using System;
using System.Collections.Generic;

namespace ScenarioRTL
{
    internal static class Asm
    {
        static bool _initialised = false;
        static Assembly _asm;

        internal static void Init()
        {
            if (_initialised)
                return;

            Assembly asm = Assembly.GetAssembly(typeof(Mission));
            Type[] alltypes = asm.GetTypes();
            _asm = asm;

            _initialised = true;

            _actionTypes = new Dictionary<string, StatementObj>();
            _conditionTypes = new Dictionary<string, StatementObj>();
            _eventListenerTypes = new Dictionary<string, StatementObj>();

            foreach (Type t in alltypes)
            {
                object[] target_attrs = t.GetCustomAttributes(typeof(StatementAttribute), false);

                if (target_attrs.Length == 0)
                    continue;

                StatementAttribute statment_attr = target_attrs[0] as StatementAttribute;

                if (t.IsSubclassOf(typeof(Action)))
                {
                    StatementObj so = new StatementObj();
                    so.className = statment_attr.className;
                    so.type = t;
                    so.recyclable = statment_attr.recyclable;
                    _actionTypes.Add(so.className, so);
                }
                else if (t.IsSubclassOf(typeof(Condition)))
                {
                    StatementObj so = new StatementObj();
                    so.className = statment_attr.className;
                    so.type = t;
                    so.recyclable = statment_attr.recyclable;
                    _conditionTypes.Add(so.className, so);
                }
                else if (t.IsSubclassOf(typeof(EventListener)))
                {
                    StatementObj so = new StatementObj();
                    so.className = statment_attr.className;
                    so.type = t;
                    so.recyclable = statment_attr.recyclable;
                    _eventListenerTypes.Add(so.className, so);
                }

            }
        }

        internal class StatementObj
        {
            public string className;
            public Type type;
            public bool recyclable;
           
        }

        static Dictionary<string, StatementObj> _actionTypes;
        static Dictionary<string, StatementObj> _conditionTypes;
        static Dictionary<string, StatementObj> _eventListenerTypes;

        internal static Action CreateActionInstance(string class_name)
        {
            if (!_actionTypes.ContainsKey(class_name))
                return null;

             return _asm.CreateInstance(_actionTypes[class_name].type.FullName) as Action;
        }

        internal static Condition CreateConditionInstance(string class_name)
        {
            if (!_conditionTypes.ContainsKey(class_name))
                return null;

            return _asm.CreateInstance(_conditionTypes[class_name].type.FullName) as Condition;
        }

        internal static EventListener CreateEventListenerInstance(string class_name)
        {
            if (!_eventListenerTypes.ContainsKey(class_name))
                return null;

            return _asm.CreateInstance(_eventListenerTypes[class_name].type.FullName) as EventListener;
        }

        internal static bool ActionIsRecyclable (string class_name)
        {
            if (!_actionTypes.ContainsKey(class_name))
                return false;

            return _actionTypes[class_name].recyclable;
        }
    }
}
