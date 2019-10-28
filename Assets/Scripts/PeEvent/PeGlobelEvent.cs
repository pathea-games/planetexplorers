using UnityEngine;
using System.Collections.Generic;

namespace PeEvent
{
    public static class Globle
    {
        public static Event<GeneralArg> general
        {
            get;
            set;
        }

        public static Event<AttributeChangedArg> attribute
        {
            get;
            set;
        }

        public static Event<KillEventArg> kill
        {
            get;
            set;
        }
    }
}