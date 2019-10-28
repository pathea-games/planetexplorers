using UnityEngine;

namespace WhiteCat
{
	/// <summary>
	/// 单例脚本组件
	/// </summary>
	public class SingletonBehaviour<T> : BaseBehaviour where T : SingletonBehaviour<T>
	{
		static T _instance;


		public static T instance
		{
			get { return _instance ? _instance : _instance = GetInstance(); }
		}


		static T GetInstance()
		{
			T[] instances = FindObjectsOfType<T>();

			if (instances.Length == 0)
			{
				Debug.Log("There is no instance of singleton type " + typeof(T) + ", a new instance will be created immediately.");

				GameObject go = new GameObject(typeof(T).ToString());
				DontDestroyOnLoad(go);
				return go.AddComponent<T>();
			}

			if (instances.Length > 1)
			{
				Debug.LogError("There are more than one instance of singleton type " + typeof(T) + ", the first found one will be returned.");
			}

			return instances[0];
		}


		protected virtual void Awake()
		{
			if (_instance) Debug.LogError("Already exist a instance of singleton type " + typeof(T) + ", you should not create it again.");
			else _instance = this as T;
		}

	}
}