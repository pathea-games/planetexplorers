using UnityEngine;
using System.Collections;
using System;

namespace GraphMapping
{	
	public class PeHeightMapping : GraphMap
	{
		private const float m_maxInHeightMap = 955;

		public override void LoadTexData(Texture2D tex)
		{
			if (tex == null)
			{
				Debug.Log("Load Height Texture falied!");
				return;
			}
			mGraphTexWidth = tex.width;
			mGraphTexHeight = tex.height;
			mDataSize_x = tex.width;
			mDataSize_y = tex.height;
			NewData();
			
			for (int i=0; i<tex.width;i++)
			{
				for(int j=0;j+1<tex.height;j++)
				{
					Color32 c32 = tex.GetPixel(i,j);
					mData[i][j] = c32.g;
				}
			}
		}
		
		public float GetHeight(Vector2 postion,Vector2 worldSize)
		{
			if (mData == null)
			{
				Debug.LogError("Get Height Faleid!");
				return 0;
			}

			IntVector2 tpos = IntVector2.Tmp;
			GetTexPos (postion, worldSize, tpos);
			if (tpos.x >= mDataSize_x || tpos.y >= mDataSize_y || tpos.x < 0 || tpos.y < 0 ) 
			{
				Debug.LogError("postion is error! GetHeight Failed ");
				return 0;
			}

			float n =   (float)((int)mData[tpos.x][tpos.y]) / 255F * m_maxInHeightMap;
			return n;
		}
	}
}