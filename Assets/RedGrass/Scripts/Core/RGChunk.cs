using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea.Maths;

namespace RedGrass
{
	public class RGChunk 
	{
		EvniAsset mEvni;

		public int xIndex;
		public int zIndex;


		Dictionary<int, RedGrassInstance> mGrasses;

		public Dictionary<int, RedGrassInstance> grasses { get { return mGrasses;} }

		Dictionary<int, List<RedGrassInstance>> mHGrass;

		public bool isEmpty { get { return (mGrasses.Count == 0);}}

		public Dictionary<int, List<RedGrassInstance>> HGrass { get { return mHGrass; } }

		public static bool AnyDirty = false;
		private bool _dirty = false;
		public bool Dirty{
			get{ return _dirty; }
			set{
				_dirty = value;
				AnyDirty |= _dirty;
			}
		}

		int mTimeStamp = 0;
		int mTimeStampOutofdate = 0;

		public void UpdateTimeStamp () 	{ ++mTimeStampOutofdate; }
		public void SyncTimeStamp ()	{ mTimeStamp = mTimeStampOutofdate;}
		public bool IsOutOfDate ()		{ return mTimeStamp != mTimeStampOutofdate; }

		public int SIZE  { get { return (int)mEvni.CHUNKSIZE;} }
		public int SHIFT { get { return mEvni.SHIFT; } }
		public int MASK  { get { return mEvni.MASK; } }
		

		public RGChunk ()
		{

		}

		public void Init (int cx, int cz, EvniAsset evni)
		{
			mEvni = evni;
			xIndex = cx;
			zIndex = cz;

			if (mGrasses == null)
				mGrasses = new Dictionary<int, RedGrassInstance>();
			if (mHGrass == null)
				mHGrass = new Dictionary<int, List<RedGrassInstance>>();
		}

		public RedGrassInstance Read (int x, int y, int z)
		{
			int key = PosToKey(Mathf.FloorToInt(x) & MASK, Mathf.FloorToInt(z) & MASK);
			if (mGrasses.ContainsKey(key))
			{
				RedGrassInstance rgi = mGrasses[key];
				if ((int)rgi.Position.y == y)
					return rgi;
				else if (mHGrass.ContainsKey(key))
				{
					return mHGrass[key].Find(item0 => (int)item0.Position.y == y);
				}
			}
				
			return new RedGrassInstance();
		}


		public List<RedGrassInstance> Read(int x, int z, int ymin, int ymax)
		{
			int key = PosToKey(Mathf.FloorToInt(x) & MASK, Mathf.FloorToInt(z) & MASK);

			List<RedGrassInstance> output = new List<RedGrassInstance>();

			if (mGrasses.ContainsKey(key))
			{
				RedGrassInstance rgi = mGrasses[key];
				if ((int)rgi.Position.y >= ymin && (int)rgi.Position.y <= ymax)
					output.Add(rgi);

				if (mHGrass.ContainsKey(key))
				{
					foreach (RedGrassInstance inst in mHGrass[key])
					{
						if ((int)inst.Position.y >= ymin && (int)inst.Position.y <= ymax)
							output.Add(inst);
					}
				}
			}


			return output;
		}


		public void Write (RedGrassInstance grass)
		{
			Vector3 pos = grass.Position;
			int y = (int)pos.y;
			int key = PosToKey(Mathf.FloorToInt(pos.x) & MASK, Mathf.FloorToInt(pos.z) & MASK);

			if (mGrasses.ContainsKey(key))
			{
				RedGrassInstance rgi = mGrasses[key];
				if ((int)rgi.Position.y == y)
					mGrasses[key] = grass;
				else if (mHGrass.ContainsKey(key))
				{
					List<RedGrassInstance> rgis = mHGrass[key];
					int index = rgis.FindIndex(item0 => (int)item0.Position.y == y);
					if (index != -1)
						rgis[index] = grass;
					else
					{
						rgis.Add(grass);
					}

				}
				else
				{
					List<RedGrassInstance> rgis = RGPoolSig.GetRGList();
					rgis.Add(grass);
					mHGrass[key] = rgis;
				}
			}
			else
			{
				mGrasses.Add(key, grass);
			}

			Dirty = true;

		}

		public bool Remove (int x, int y, int z)
		{
			int key = PosToKey(Mathf.FloorToInt(x) & MASK, Mathf.FloorToInt(z) & MASK);

			if (mGrasses.ContainsKey(key))
			{
				RedGrassInstance rgi = mGrasses[key];
				if ((int)rgi.Position.y == y)
				{
					if (mHGrass.ContainsKey(key))
					{
						List<RedGrassInstance> rgis = mHGrass[key];
						mGrasses[key] = rgis[0];
						rgis.RemoveAt(0);
						if (rgis.Count == 0)
						{
							RGPoolSig.RecycleRGList(rgis);
							mHGrass.Remove(key);
						}

						Dirty = true;
						return true;
					}
					else
					{
						Dirty = true;
						mGrasses.Remove(key);
						return true;
					}
				
				}
				else if (mHGrass.ContainsKey(key))
				{
					int index = mHGrass[key].FindIndex(item0 => (int)item0.Position.y == y);
					if (index != -1)
					{
						mHGrass[key].RemoveAt(index);
						if (mHGrass[key].Count == 0)
							mHGrass.Remove(key);

						Dirty = true;
						return true;
					}
				}

			}

			return false;
		}
	

		public void Free ()
		{
			lock (this) 
			{
				mGrasses.Clear();

				if(mHGrass.Count > 0){
					foreach(KeyValuePair<int, List<RedGrassInstance>> pair in mHGrass){
						pair.Value.Clear();
						RGPoolSig.RecycleRGList(pair.Value);
					}
					mHGrass.Clear();
				}

				Dirty = true;
			}
		}


		public void DrawAreaInEditor ()
		{
			INTVECTOR3 size = new INTVECTOR3(mEvni.CHUNKSIZE, 0, mEvni.CHUNKSIZE);
			INTVECTOR3 pos = new INTVECTOR3(xIndex, 0, zIndex);
			Color color = Color.yellow;
			
			
			Pathea.Graphic.EditorGraphics.DrawXZRect((Vector3)(pos * size), (Vector3)size, color); 
		}

	

		#region HELP_FUNC

		public int PosToKey (int dx, int dy)
		{
			return dx << mEvni.SHIFT | dy;
		}


		#endregion
	}
}
