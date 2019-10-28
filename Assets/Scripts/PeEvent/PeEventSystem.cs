using UnityEngine;
using System.Collections.Generic;

namespace PeEvent
{
    public class EventArg
    {
    }

    public class Event<T> where T : EventArg
    {
        public delegate void Handler(object sender, T arg);

        event Handler mHandler;
        object mSender;

        public Event(object sender)
        {
            mSender = sender;
        }

        public Event()
        {
            mSender = null;
        }

        public void Subscribe(Handler handler)
        {
            mHandler += handler;
        }

        public void Unsubscribe(Handler handler)
        {
            mHandler -= handler;
        }

        public void Dispatch(T arg)
        {
            Dispatch(arg, null);
        }

        public void Dispatch(T arg, object sender)
        {
            if (null == mHandler)
            {
                return;
            }

            if (null != sender)
            {
                mHandler(sender, arg);
            }
            else
            {
                mHandler(mSender, arg);
            }
        }
    }
}