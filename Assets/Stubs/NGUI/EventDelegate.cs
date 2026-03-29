using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// NGUI stub: delegate wrapper for UI event callbacks.
/// Provides Add/Remove/Execute for event delegate lists.
/// Will be replaced with functional uGUI equivalent in Phase 8.
/// </summary>
[System.Serializable]
public class EventDelegate
{
    public delegate void Callback();

    public MonoBehaviour target;
    public string methodName;

    [System.NonSerialized]
    public Callback mCachedCallback;

    public bool oneShot;

    public EventDelegate() { }

    public EventDelegate(Callback callback)
    {
        mCachedCallback = callback;
    }

    public EventDelegate(MonoBehaviour target, string methodName)
    {
        this.target = target;
        this.methodName = methodName;
    }

    public bool isValid
    {
        get { return mCachedCallback != null || (target != null && !string.IsNullOrEmpty(methodName)); }
    }

    public bool Execute()
    {
        if (mCachedCallback != null)
        {
            mCachedCallback();
            return true;
        }
        return false;
    }

    public void Clear()
    {
        target = null;
        methodName = null;
        mCachedCallback = null;
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return !isValid;
        if (obj is Callback callback) return mCachedCallback == callback;
        if (obj is EventDelegate del) return target == del.target && methodName == del.methodName;
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static void Add(List<EventDelegate> list, EventDelegate.Callback callback)
    {
        Add(list, callback, false);
    }

    public static void Add(List<EventDelegate> list, EventDelegate.Callback callback, bool oneShot)
    {
        if (list != null)
        {
            var del = new EventDelegate(callback);
            del.oneShot = oneShot;
            list.Add(del);
        }
    }

    public static void Add(List<EventDelegate> list, EventDelegate del)
    {
        Add(list, del, false);
    }

    public static void Add(List<EventDelegate> list, EventDelegate del, bool oneShot)
    {
        if (list != null && del != null)
        {
            del.oneShot = oneShot;
            list.Add(del);
        }
    }

    public static bool Remove(List<EventDelegate> list, Callback callback)
    {
        if (list != null)
        {
            for (int i = list.Count - 1; i >= 0; --i)
            {
                if (list[i] != null && list[i].mCachedCallback == callback)
                {
                    list.RemoveAt(i);
                    return true;
                }
            }
        }
        return false;
    }

    public static void Remove(List<EventDelegate> list, EventDelegate del)
    {
        if (list != null && del != null)
        {
            list.Remove(del);
        }
    }

    public static void Set(List<EventDelegate> list, Callback callback)
    {
        if (list != null)
        {
            list.Clear();
            Add(list, callback);
        }
    }

    public static void Set(List<EventDelegate> list, EventDelegate del)
    {
        if (list != null)
        {
            list.Clear();
            list.Add(del);
        }
    }

    public static void Execute(List<EventDelegate> list)
    {
        if (list == null) return;
        for (int i = 0; i < list.Count; )
        {
            var del = list[i];
            if (del != null)
            {
                del.Execute();
                if (del.oneShot)
                {
                    list.RemoveAt(i);
                    continue;
                }
            }
            ++i;
        }
    }

    public static bool IsValid(List<EventDelegate> list)
    {
        if (list != null)
        {
            for (int i = 0; i < list.Count; ++i)
                if (list[i] != null && list[i].isValid)
                    return true;
        }
        return false;
    }

    public static EventDelegate Set(List<EventDelegate> list, int index, Callback callback)
    {
        if (list == null) return null;
        while (list.Count <= index) list.Add(null);
        var del = new EventDelegate(callback);
        list[index] = del;
        return del;
    }

    public static void Copy(List<EventDelegate> src, List<EventDelegate> dst)
    {
        if (dst != null)
        {
            dst.Clear();
            if (src != null)
            {
                for (int i = 0; i < src.Count; ++i)
                    dst.Add(src[i]);
            }
        }
    }
}
