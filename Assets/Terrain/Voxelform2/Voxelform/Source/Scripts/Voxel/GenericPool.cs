using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenericPool<T> where T : new()
{
	private Stack<T> _items;
	private object _sync = new object ();
#if UNITY_EDITOR
	private int nSum = 0;
#endif
	public GenericPool(int preserved = 8)
	{
		_items = new Stack<T> (preserved);
		for (int i = 0; i < preserved; i++) {
#if UNITY_EDITOR
			nSum++;
#endif
			_items.Push(new T());
		}
	} 
	public T Get ()
	{
		lock (_sync) {
			if (_items.Count == 0) {
#if UNITY_EDITOR
				nSum++;
#endif
				return new T();
			} else {
				return _items.Pop();
			}
		}
	}	
	public void Free (T item)
	{
		lock (_sync) {
			_items.Push (item);
		}
	}
}

public class GenericArrayPool<T>
{
	private Stack<T[]> _items;
	private object _sync = new object ();	
	private int _size;
#if UNITY_EDITOR
	private int nSum = 0;
#endif
	public GenericArrayPool(int arraySize, int preserved = 8)
	{
		_size = arraySize;	
		_items = new Stack<T[]> (preserved);
		for (int i = 0; i < preserved; i++) {
#if UNITY_EDITOR
			nSum++;
#endif
			_items.Push(new T[_size]);
		}
	} 
	public T[] Get ()
	{
		lock (_sync) {
			if (_items.Count == 0) {
#if UNITY_EDITOR
				nSum++;
#endif
				return new T[_size];
			} else {
				return _items.Pop();
			}
		}
	}	
	public void Free (T[] item)
	{
		lock (_sync) {
			_items.Push (item);
		}
	}
}
