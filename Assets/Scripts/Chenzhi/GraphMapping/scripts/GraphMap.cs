using UnityEngine;
using System.Collections;

namespace GraphMapping
{
	public abstract class GraphMap
	{
		protected byte[][] mData = null;
		protected int mDataSize_x;
		protected int mDataSize_y;
		
		protected int mGraphTexWidth;
		protected int mGraphTexHeight;
		
		public abstract void LoadTexData(Texture2D tex);

		// Out: tpos
		protected void GetTexPos(Vector2 postion , Vector2 worldSize, IntVector2 tpos)
		{
			float fx =  postion.x / worldSize.y * mGraphTexWidth;
			float fy =  postion.y / worldSize.y * mGraphTexHeight;
			tpos.x = Mathf.RoundToInt(fx);
			tpos.y = Mathf.RoundToInt(fy);
		}
		
		protected virtual void NewData()
		{
			if (mData == null)
			{	
				mData = new byte[mDataSize_x][];
				for (int i=0;i<mDataSize_x;i++)
					mData[i] = new byte[mDataSize_y];
			}
		}
		
		public virtual byte[] Serialize()
		{
			if (mData == null) 
				return null;
			
			try
			{
				System.IO.MemoryStream ms = new System.IO.MemoryStream(200);
				using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms))
				{
					bw.Write(mGraphTexWidth);
					bw.Write(mGraphTexHeight);
					bw.Write(mDataSize_x);
					bw.Write(mDataSize_y);
					
					for (int i=0;i<mDataSize_x;i++)
						bw.Write(mData[i]);
				}
				
				return ms.ToArray();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e);
				return null;
			}
		}
		
		public virtual bool Deserialize(byte[] buf)
		{
			try
			{
				System.IO.MemoryStream ms = new System.IO.MemoryStream(buf, false);
				using (System.IO.BinaryReader br = new System.IO.BinaryReader(ms))
				{
					
					mGraphTexWidth =  br.ReadInt32();
					mGraphTexHeight = br.ReadInt32();
					mDataSize_x = br.ReadInt32();
					mDataSize_y = br.ReadInt32();
					NewData();
					
					for (int i=0;i<mDataSize_x;i++)
						mData[i] = br.ReadBytes(mDataSize_y);
					
					return true;
				}
			}
			catch (System.Exception e)
			{
				Debug.LogWarning(e);
				return false;
			}
		}
	}
}