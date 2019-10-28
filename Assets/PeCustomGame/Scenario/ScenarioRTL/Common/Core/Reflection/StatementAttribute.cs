using System;

namespace ScenarioRTL
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StatementAttribute : Attribute
    {
		public StatementAttribute (string _classname)
		{
			className = _classname;
		}
		public StatementAttribute (string _classname, bool _recyclable)
		{
			className = _classname;
			recyclable = _recyclable;
		}

        public string className;
        public bool recyclable;
    }
}
