namespace PatheaScript
{
    public class InternalEvent
    {
        static InternalEvent instance;
        public static InternalEvent Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new InternalEvent();
                }

                return instance;
            }
        }

        public delegate void ScriptChanged(PsScript script);

        public event ScriptChanged eventScriptBegin;
        public void EmitScriptBegin(PsScript script)
        {
            if (null != eventScriptBegin)
            {
                eventScriptBegin(script);
            }
        }

        public event ScriptChanged eventScriptEnd;
        public void EmitScriptEnd(PsScript script)
        {
            if (null != eventScriptEnd)
            {
                eventScriptEnd(script);
            }
        }
    }
}