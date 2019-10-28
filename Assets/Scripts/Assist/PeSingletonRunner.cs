using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathea
{
    public class PeSingletonRunner : MonoBehaviour
    {
        void Update()
        {
            MonoLikeSingletonMgr.Instance.Update();
        }

        void OnDestroy()
        {
            MonoLikeSingletonMgr.Instance.OnDestroy();
        }

        public static void Launch()
        {
            new GameObject("PeSingletonRunner").AddComponent<PeSingletonRunner>();
        }
    }

    public interface IPesingleton
    {
        void Init();
    }

    public abstract class PeSingleton<T> where T : class, new()
    {
        static T instance;
        public static T Instance
        {
            get
            {
                if (null == instance)
                {
                    instance = new T();
                    IPesingleton i = instance as IPesingleton;
                    if (i != null)
                    {
                        i.Init();
                    }
                }

                return instance;
            }

            protected set
            {
                instance = value;
            }
        }
    }

    public class MonoLikeSingletonMgr : PeSingleton<MonoLikeSingletonMgr>
    {
        public interface IMonoLike
        {
            void Update();
			void OnDestroy();
        }

        List<IMonoLike> mList = new List<IMonoLike>(40);

        public void Update()
        {
			int n = mList.Count;
			for (int i = 0; i < n; i++) {
				mList[i].Update();
			}
        }
        public void OnDestroy()
        {
            Clear();
        }

        public void Clear()
        {
            //Debug.Log("-----------------------------clear singleton, count:" + mList.Count);
			int n = mList.Count;
			for (int i = 0; i < n; i++) {
				mList[i].OnDestroy();
			}
            mList.Clear();
        }

        public void Register(IMonoLike m)
        {
            //Debug.Log("-----------------------------register singleton:"+m.GetType());
            mList.Add(m);
        }
    }

    public abstract class MonoLikeSingleton<T> : PeSingleton<T>, IPesingleton, MonoLikeSingletonMgr.IMonoLike where T : class, new()
    {
		protected virtual void OnInit(){}
        void IPesingleton.Init()
        {
            MonoLikeSingletonMgr.Instance.Register(this);
            OnInit();
        }

		#region MonoLikeSingletonMgr.IMonoLike
        public virtual void Update(){}
		public virtual void OnDestroy(){	Instance = null;    }
		#endregion
    }
}