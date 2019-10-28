using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class BiLookup<TFirst, TSecond>
{
	System.Object obj4Lock = new System.Object();
    Dictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
    Dictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();
	List<TFirst> indexByFirst = new List<TFirst>(1024);
	
	public void Lock()  {	Monitor.Enter(obj4Lock);}
	public void UnLock(){	Monitor.Exit(obj4Lock);	}
	public void Add(TFirst first, TSecond second)
    {
		lock(obj4Lock)
		{
	        TSecond oldSecond;
	        if (firstToSecond.TryGetValue(first, out oldSecond))
	        {
				firstToSecond.Remove(first);
				secondToFirst.Remove(oldSecond);
				indexByFirst.Remove(first);
	        }
			TFirst oldFirst;
	        if (secondToFirst.TryGetValue(second, out oldFirst))
	        {
				firstToSecond.Remove(oldFirst);
				secondToFirst.Remove(second);
				indexByFirst.Remove(oldFirst);
				Debug.Log("BiLookup::Add--------");
	        }		
			firstToSecond.Add(first, second);
			secondToFirst.Add(second, first);
			indexByFirst.Add(first);
		}
    }
	public void TryAdd(TFirst first, TSecond second)
    {
		lock(obj4Lock)
		{
	        TSecond oldSecond;
	        if (firstToSecond.TryGetValue(first, out oldSecond))
	        {
				if(oldSecond.Equals(second))		return;
				
				firstToSecond.Remove(first);
				secondToFirst.Remove(oldSecond);
				indexByFirst.Remove(first);
	        }
			firstToSecond.Add(first, second);
			secondToFirst.Add(second, first);
			indexByFirst.Add(first);
		}
    }
	public void Insert(TFirst first, TSecond second, int index)
    {
		lock(obj4Lock)
		{
	        TSecond oldSecond;
	        if (firstToSecond.TryGetValue(first, out oldSecond))
	        {
				firstToSecond.Remove(first);
				secondToFirst.Remove(oldSecond);
				indexByFirst.Remove(first);
	        }
			firstToSecond.Add(first, second);
			secondToFirst.Add(second, first);
			indexByFirst.Insert(index, first);
		}
    }
	
	public bool RemoveByKey(TFirst first)
	{
		lock(obj4Lock)
		{
			TSecond second;
	        if (firstToSecond.TryGetValue(first, out second))
	        {
				firstToSecond.Remove(first);
				secondToFirst.Remove(second);
				indexByFirst.Remove(first);
				return true;
	        }
			return false;
		}
	}
	public bool RemoveByValue(TSecond second)
	{
		lock(obj4Lock)
		{
			TFirst first;
	        if (secondToFirst.TryGetValue(second, out first))
	        {
				firstToSecond.Remove(first);
				secondToFirst.Remove(second);
				indexByFirst.Remove(first);
				return true;
	        }
			return false;
		}
	}	
	public void RemoveAt(int index)
	{
		lock(obj4Lock)
		{
			TFirst first = indexByFirst[index];
			secondToFirst.Remove(firstToSecond[first]);
			firstToSecond.Remove(first);
			indexByFirst.RemoveAt(index);
		}
	}	
	
	public void Clear()
	{
		lock(obj4Lock)
		{
			firstToSecond.Clear();
			secondToFirst.Clear();
			indexByFirst.Clear();
		}
	}
	
	public void Append(BiLookup<TFirst, TSecond> listToAppend)
	{
		lock(obj4Lock)
		{
			int n = listToAppend.indexByFirst.Count;
			for(int i = 0; i < n; i++)
			{
				TFirst first = listToAppend.indexByFirst[i];
				TSecond second = listToAppend.firstToSecond[first];
				Add(first, second);
			}
		}
	}
	public BiLookup<TFirst, TSecond> CutPaste()
	{
		lock(obj4Lock)
		{
			BiLookup<TFirst, TSecond> ret = new BiLookup<TFirst, TSecond>();
			int n = indexByFirst.Count;
			for(int i = 0; i < n; i++)
			{
				TFirst first = indexByFirst[i];
				TSecond second = firstToSecond[first];
				ret.Add(first, second);
			}
			Clear();
			return ret;
		}	
	}
	
    // Note potential ambiguity using indexers (e.g. mapping from int to int)
    // Hence the methods as well...
	public TSecond GetValueByKey_Unsafe(TFirst first)	{	return firstToSecond[first];				}
	public TSecond GetValueByIdx_Unsafe(int index)		{	return firstToSecond[indexByFirst[index]];	}
	public TFirst  GetKeyByValue_Unsafe(TSecond second)	{	return secondToFirst[second];			    }
	public TFirst  GetKeyByIdx_Unsafe(int index)		{	return indexByFirst[index];				    }
	public TSecond this[TFirst first]
    {
        get { lock(obj4Lock)	return firstToSecond[first]; }
    }
    public TFirst this[TSecond second]
    {
        get { lock(obj4Lock)	return secondToFirst[second]; }
    }
	public int Count
	{
		get { lock(obj4Lock)	return indexByFirst.Count;		}
	}
	public List<TFirst> ToKeyList()
	{
		lock(obj4Lock)			return firstToSecond.Keys.ToList();
	}
	public List<TSecond> ToValueList()
	{
		lock(obj4Lock)			return firstToSecond.Values.ToList();
	}	
	public bool ContainsKey(TFirst first)
	{
		lock(obj4Lock)			return firstToSecond.ContainsKey(first);
	}
	public bool ContainsValue(TSecond second)
	{
		lock(obj4Lock)			return secondToFirst.ContainsKey(second);
	}
	public bool Contains(TFirst first, TSecond second)
	{
		lock(obj4Lock)
		{
	        TSecond oldSecond;
	        if (firstToSecond.TryGetValue(first, out oldSecond))
	        {
				if(oldSecond.Equals(second))		return true;
	        }
			return false;
		}
	}
}
