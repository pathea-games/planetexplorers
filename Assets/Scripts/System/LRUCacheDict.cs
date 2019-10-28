using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LRUCacheDict<TKey,TValue>// : IDictionary<TKey,TValue>
{
    private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dict= 
        new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();

    private LinkedList<KeyValuePair<TKey, TValue>> _list = 
        new LinkedList<KeyValuePair<TKey, TValue>>();
	
	private TKey _lastRemovedKey;
	
	public TKey LastRemoved{ get{	return _lastRemovedKey;	} }
    public int MaxSize { get; set; }

    public LRUCacheDict(int maxsize = 1)
    {
        MaxSize = maxsize;
    }

    public void Add(TKey key, TValue value)
    {
        lock (_dict)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (_dict.TryGetValue(key, out node))
            {
                _list.Remove(node);
                _list.AddFirst(node);
            }
            else
            {
                node = new LinkedListNode<KeyValuePair<TKey, TValue>>(
                					  new KeyValuePair<TKey, TValue>(key, value));
                _dict.Add(key, node);
                _list.AddFirst(node);
            }
			
			_lastRemovedKey = default(TKey);
            if (_dict.Count > MaxSize)
            {
                var nodetoremove = _list.Last;
                if (nodetoremove != null)
				{
					_lastRemovedKey = nodetoremove.Value.Key;
                    Remove(_lastRemovedKey);
				}
            }
        }
    }

    public bool Remove(TKey key)
    {
        lock (_dict)
        {
            LinkedListNode<KeyValuePair<TKey, TValue>> removednode;
            if (_dict.TryGetValue(key, out removednode))
            {
                _dict.Remove(key);
                _list.Remove(removednode);
                return true;
            }
            else
                return false;
        }
    }
	
	public void Clear()
	{
		lock (_dict)
		{
			_dict.Clear();
			_list.Clear();
		}
	}

    public bool TryGetValue(TKey key, out TValue value)
    {
		lock (_dict)
		{
			LinkedListNode<KeyValuePair<TKey, TValue>> node;
			bool result = _dict.TryGetValue(key, out node);
			if (node != null)
			{
				value = node.Value.Value;
				_list.Remove(node);
				_list.AddFirst(node);
			}
			else
				value = default(TValue);
			
			return result;
		}
    }

  //[rest of IDictionary not implemented yet]
}