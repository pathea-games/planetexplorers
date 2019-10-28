using System;

namespace WhiteCat
{
	public enum SyncAction
	{
		none,		// 不同步
		sync,		// 同步
		freeze		// 冻结
	}


	public class NetData<T> where T : struct
	{
		Func<T, bool> needSync;
		Func<T> getData;
		Action<T> setData;
		
		T last;

		bool finalSynced = false;


		public NetData(Func<T, bool> needSync, Func<T> getData, Action<T> setData)
		{
			this.needSync = needSync;
			this.getData = getData;
			this.setData = setData;
			last = getData();
        }


		public SyncAction GetSyncAction()
		{
			if(needSync(last))
			{
				finalSynced = false;
				return SyncAction.sync;
			}
			else
			{
				if (finalSynced)
				{
					return SyncAction.none;
				}
				else
				{
					finalSynced = true;
					return SyncAction.freeze;
				}
			}
		}


		public T GetData()
		{
			if (finalSynced) return getData();
			else return last = getData();
		}


		public void SetData(T data)
		{
			setData(last = data);
		}


        public T lastData
        {
            get { return last; }
        }
	}
}