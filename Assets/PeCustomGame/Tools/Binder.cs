using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace PeCustom
{
	public class HashBinder
	{
		Dictionary<System.Type, object> mBinders = new Dictionary<System.Type, object>();

		public void Bind<T>(T obj) where T : class
		{
			System.Type type = obj.GetType();
			if (mBinders.ContainsKey(type))
			{
				Debug.LogError("this type is already exist");
				return;
			}
			mBinders.Add(type, obj);
		}

		public T Get<T>() where T : class
		{
			System.Type type = typeof(T);
			if (mBinders.ContainsKey(type))
			{
				return mBinders[type] as T;
			}
			return null;
		}

		public void Unbind<T>() where T : class
		{
			mBinders.Remove(typeof(T));

		}
	}


}