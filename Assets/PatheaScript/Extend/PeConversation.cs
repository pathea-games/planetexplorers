using PatheaScript;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Event = PatheaScript.Event;

namespace PatheaScriptExt
{
    public class PeConversation : Action
    {
        List<Sentence> mList;
        int mCurIndex;

        public override bool Parse()
        {
            mList = new List<Sentence>(5);

            //TODO:replace with real data
            for (int i = 0; i < 10; i++)
            {
                Sentence sentence = new Sentence();
                sentence.mAnimation = "talk "+i;
                sentence.mSoundId = 123+i;
                sentence.mText = "sentence "+i;
                sentence.mSpeaker = new Sentence.Speaker();
                sentence.mSpeaker.mId = i;
                sentence.mSpeaker.mType = Sentence.Speaker.EType.Npc;

                mList.Add(sentence);
            }

            return true;
        }

        protected override bool OnInit()
        {
            if (false == base.OnInit())
            {
                return false;
            }

            mCurIndex = 0;
            
            PeEventMgr.Instance.SubscribeEvent(PeEventMgr.EEventType.MouseClicked, MouseEventHandler);
            return true;
        }

        protected override TickResult OnTick()
        {
            if (TickResult.Finished == base.OnTick())
            {
                return TickResult.Finished;
            }

            if (null == mList || mCurIndex >= mList.Count)
            {
                return TickResult.Finished;
            }

            return TickResult.Running;
        }

        protected override void OnReset()
        {
            base.OnReset();

            PeEventMgr.Instance.UnsubscribeEvent(PeEventMgr.EEventType.MouseClicked, MouseEventHandler);
            ShowCurSentence();
        }

        void MouseEventHandler(PeEventMgr.EventArg arg)
        {
            PeEventMgr.MouseEvent mouseEvent = arg as PeEventMgr.MouseEvent;
            if (null == mouseEvent || PeEventMgr.MouseEvent.Type.LeftClicked != mouseEvent.mType)
            {
                return;
            }

            ShowNextSentence();
        }

        bool ShowCurSentence()
        {
            if(mCurIndex >= mList.Count)
            {
                Sentence.Mgr.Instance.Close();
                PeEventMgr.Instance.EmitEvent(PeEventMgr.EEventType.Conversation, null);
                return false;
            }

            Sentence.Mgr.Instance.Show(mList[mCurIndex]);

            return true;
        }

        void ShowNextSentence()
        {
            mCurIndex++;
            if (!ShowCurSentence())
            {
                
            }
        }

        public override void Store(System.IO.BinaryWriter w)
        {
            base.Store(w);
            w.Write(mCurIndex);
        }

        public override void Restore(System.IO.BinaryReader r)
        {
            base.Restore(r);
            mCurIndex = r.ReadInt32();

            ShowCurSentence();
        }
    }
}

public class Sentence
{
    public class Speaker
    {
        public enum EType
        {
            Player,
            Npc,
            Monster,
            Alien,
            Max
        }

        public EType mType;
        public int mId;

        public override string ToString()
        {
            return string.Format("Speaker[{0},{1}]", mType, mId);
        }
    }

    public Speaker mSpeaker;
    public string mText;
    public string mAnimation;
    public int mSoundId;

    public override string ToString()
    {
        return string.Format("Sentence[{0},{1},{2},{3}]", mSpeaker, mText, mAnimation, mSoundId);
    }

    public class Mgr
    {
        static Mgr instance;
        public static Mgr Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new Mgr();
                }

                return instance;
            }
        }

        public void Show(Sentence sentence)
        {
            //TODO:show in gui
            Debug.Log(sentence);
        }

        public void Close()
        {
            //TODO:close gui
        }
    }
}

public class PeEventMgr
{
    static PeEventMgr instance;
    public static PeEventMgr Instance
    {
        get
        {
            if (null == instance)
            {
                instance = new PeEventMgr();
            }

            return instance;
        }
    }

    public class EventArg
    {
        public object mSender;
    }

    public enum EEventType
    {
        Conversation,
        MouseClicked,
        NpcDead,
        StatsChanged,
        Max
    }

    public class MouseEvent : EventArg
    {
        public enum Type
        {
            LeftClicked,
            RightClicked,
            MiddleClicked,
            Max
        }

        public MouseEvent(Type type)
        {
            mType = type;
        }

        public Type mType;
    }

    public delegate void EventHandler(EventArg arg);

    class Event
    {
        event EventHandler mEventDispatch;

        public void SubscribeEvent(EventHandler handler)
        {
            mEventDispatch += handler;
        }

        public void UnsubscribeEvent(EventHandler handler)
        {
            mEventDispatch -= handler;
        }

        public void PublishEvent(EventArg arg)
        {
            if (null == mEventDispatch)
            {
                return;
            }

            mEventDispatch(arg);
        }
    }

    Dictionary<EEventType, Event> mDicEvent;

    public void EmitEvent(EEventType eventType, EventArg arg)
    {
        if(null == mDicEvent)
        {
            return;
        }

        if (!mDicEvent.ContainsKey(eventType))
        {
            return;
        }

        mDicEvent[eventType].PublishEvent(arg);
    }

    public void SubscribeEvent(EEventType eventType, EventHandler eventHandler)
    {
        if (null == mDicEvent)
        {
            mDicEvent = new Dictionary<EEventType, Event>(20);
        }

        if (!mDicEvent.ContainsKey(eventType))
        {
            mDicEvent[eventType] = new Event();
        }

        mDicEvent[eventType].SubscribeEvent(eventHandler);
    }

    public void UnsubscribeEvent(EEventType eventType, EventHandler eventHandler)
    {
        if (null == mDicEvent)
        {
            return;
        }

        if (!mDicEvent.ContainsKey(eventType))
        {
            return;
        }

        mDicEvent[eventType].UnsubscribeEvent(eventHandler);
    }
}