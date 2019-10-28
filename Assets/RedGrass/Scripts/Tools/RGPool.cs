using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RedGrass
{
	public class RGPoolSig
	{
		const int CHUNK_POOL_COUNT = 1280; 
		const int RGLIST_POO_COUNT = 100;

		static RGPoolSig _self;
		public static RGPoolSig Self
		{
			get 
			{
				if (_self == null)
				{
					_self = new RGPoolSig();
				}

				return _self;
			}
		}


		public static void Init ()
		{
			if (_self == null)
				_self = new RGPoolSig();
		}

		public static void Destroy()
		{
			if (_self != null)
			{
				_self.mRGListPool.Destroy();
			}
		}

		public static RGChunk GetChunk()
		{

			return _self.mChunkPool.Get();
//			return new RGChunk();
		}
	
		public static void RecycleChunk (RGChunk chunk)
		{
			chunk.Free();
			_self.mChunkPool.Recycle(chunk);	
		}

		public static List<RedGrassInstance> GetRGList ()
		{
			return _self.mRGListPool.Get();
		}

		public static void RecycleRGList (Dictionary<int, List<RedGrassInstance>> grasses)
		{
			_self.mRGListPool.Recycle(grasses);
		}

		public static void RecycleRGList(List<RedGrassInstance> list)
		{
			_self.mRGListPool.Recycle(list);
		}


		RGSingletonPool<RGChunk> mChunkPool;
		RGListPool mRGListPool;

		public RGPoolSig ()
		{
			mChunkPool = new RGSingletonPool<RGChunk> (CHUNK_POOL_COUNT);
			mRGListPool = new RGListPool(RGLIST_POO_COUNT, 10);
		}

	}


	/// <summary>
	/// Red grass instance pool.
	/// </summary>
//	public class RGPool
//	{
//		public RGPool(int capacity, string name = "None")
//		{
//			mPool = new Stack<RedGrassInstance>(capacity);
//			for (int i = 0; i < capacity; i++) 
//			{
//				mPool.Push(new RedGrassInstance());
//			}
//
//			if (mThread == null)
//			{
//				mRunning = true;
//				mThread = new Thread(new ThreadStart(Run));
//				mThread.Name = name.ToString() + " Pool ";
//				mThread.Start();
//				
//			}
//		}
//
//		public void Destroy()
//		{
//			mRunning = false;
//		}
//
//		~RGPool()
//		{
//			mRunning = false;
//		}
//
//
//		public RedGrassInstance Get()
//		{
//			lock (mPool) 
//			{
//				if (mPool.Count == 0)
//					return new RedGrassInstance();
//
//				return mPool.Pop();
//			}
//		}
//
//		public void Recycle(List<RedGrassInstance>[] grasses)
//		{
//			lock (mTempPool) 
//			{
//				mTempPool.Enqueue(grasses);
//			}
//		}
//
//		public void Recycle(RedGrassInstance rgi)		
//		{
//			lock (mPool) 
//			{
//				mPool.Push(rgi);
//			}
//		}
//
//
//		Queue<List<RedGrassInstance>[]> mTempPool = new Queue<List<RedGrassInstance>[]> ();
//		Stack<RedGrassInstance> mPool = new Stack<RedGrassInstance>();
//		bool mRunning = false;
//
//		Thread mThread;
//
//		void Run()
//		{
//			try
//			{
//				while (mRunning) 
//				{
//					while(mTempPool.Count != 0)
//					{
//						List<RedGrassInstance>[] rgis = null;
//						lock (mTempPool)
//						{
//							rgis = mTempPool.Dequeue();
//						}
//						foreach (List<RedGrassInstance> list in rgis)
//						{
//							if (list == null)
//								continue;
//							foreach (RedGrassInstance rgi in list)
//							{
//								lock(mPool)
//								{
//									mPool.Push(rgi);
//								}
//							}
//						}
//
//					}
//
//					Thread.Sleep(10);
//				}
//			}
//			catch (System.SystemException ex)
//			{
//				Debug.LogError("<<<< Data Pool Thread error >>>> \r\n" + ex);
//			}
//		}
//	}


	/// <summary>
	/// Red grass  Singleton pool.
	/// </summary>

	public sealed class RGSingletonPool<T>  where T : class, new()
	{
		public RGSingletonPool(int capacity = 100)
		{
			
			mPool = new Stack<T>(capacity);
			for (int i = 0; i < capacity; i++)
			{
				mPool.Push(new T());
			}
		}

		public T Get()
		{
			lock (mPool) 
			{
				if (mPool.Count == 0)
					return new T();
				
				return mPool.Pop();
			}
		}

		public void Recycle(T item)		
		{
			lock (mPool) 
			{
				mPool.Push(item);
			}
		}


		#region MEMBER
		Stack<T> mPool;
		#endregion
	}

	public sealed class RGListPool<T>  where T : class, new()
	{
		public RGListPool(int capacity = 20, int per_list_cap = 100)
		{
			mPool = new Stack<List<T>> (capacity);
			for (int i = 0; i < mPool.Count; i++) 
			{
				mPool.Push (new List<T>(per_list_cap));
			}
		}

		public List<T> Get()
		{
			lock (mPool) 
			{
				if (mPool.Count == 0)
					return new List<T>();
				
				return mPool.Pop();
			}
		}
		
		public void Recycle(List<T> item)		
		{
			lock (mPool) 
			{
				mPool.Push(item);
			}
		}


		#region MEMBER
		Stack<List<T>> mPool;
		#endregion
	}

	public sealed class RGListPool
	{
		public RGListPool(int capacity, int per_list_cap,  string name = "None")
		{
			mPool = new Stack<List<RedGrassInstance>>(capacity);
			for (int i = 0; i < capacity; i++) 
			{
				mPool.Push(new List<RedGrassInstance>(per_list_cap));
			}

			mTempPool= new Queue<Dictionary<int, List<RedGrassInstance>>>();
			mCapacity = per_list_cap;

			if (mThread == null)
			{
				mRunning = true;
				mThread = new Thread(new ThreadStart(Run));
				mThread.Name = name.ToString() + " Pool ";
				mThread.Start();
				
			}
		}

		public void Destroy()
		{
			mRunning = false;
		}
		
		~RGListPool()
		{
			mRunning = false;
		}
		
		
		public List<RedGrassInstance> Get()
		{
			lock (mPool) 
			{
				if (mPool.Count == 0)
					return new List<RedGrassInstance>(mCapacity);
				
				return mPool.Pop();
			}
		}
		
		public void Recycle(Dictionary<int, List<RedGrassInstance>> grasses)
		{
			lock (mTempPool) 
			{
				mTempPool.Enqueue(grasses);
			}
		}
		
		public void Recycle(List<RedGrassInstance> rgi)		
		{
			lock (mPool) 
			{
				mPool.Push(rgi);
			}
		}


		void Run()
		{
			try
			{
				while (mRunning) 
				{
					while(mTempPool.Count != 0)
					{
						Dictionary<int, List<RedGrassInstance>> rgis = null;
						lock (mTempPool)
						{
							rgis = mTempPool.Dequeue();
						}

						foreach (List<RedGrassInstance> list in rgis.Values)
						{
							if (list == null)
								continue;
							lock (mPool)
							{
								mPool.Push(list);
							}
						}

						
					}
					
					Thread.Sleep(10);
				}
			}
			catch (System.SystemException ex)
			{
				Debug.LogError("<<<< Data Pool Thread error >>>> \r\n" + ex);
			}
		}

		#region MEMBER
		Queue<Dictionary<int, List<RedGrassInstance>>> mTempPool;
		Stack<List<RedGrassInstance>> mPool;
		bool mRunning = false;

		int mCapacity = 10;
		
		Thread mThread;
		#endregion
	}
}
