using UnityEngine;
using System.Collections;
using System;

namespace GraphMapping
{	
	public class PeBiomeMapping : GraphMap
	{
		public override void LoadTexData(Texture2D tex)
		{
			if (tex == null)
			{
				Debug.Log("Load Biome Texture falied!");
				return;
			}
			mGraphTexWidth = tex.width;
			mGraphTexHeight = tex.height;
			mDataSize_x = tex.width;
			mDataSize_y = tex.height / 2;
			NewData();
			
			for (int i=0; i<tex.width;i++)
			{
				for(int j=0;j+1<tex.height;j+=2)
				{
					int lo4,hi4;
					Color c = tex.GetPixel(i,j);
					hi4 = Mathf.RoundToInt(c.r * 255 / 20);
					c = tex.GetPixel(i,j+1);
					lo4 = Mathf.RoundToInt(c.r * 255 / 20);
					byte b = (byte)(lo4 + (hi4<< 4));
					
					mData[i][Convert.ToInt32(j/2)] = b;
				}
			}
		}
		
		public EBiome GetBiom(Vector2 postion,Vector2 worldSize)
		{
			if (mData == null)
			{
				Debug.LogError("Get Biome Faleid!");
				return EBiome.Grassland;
			}

			IntVector2 tpos = IntVector2.Tmp;
			GetTexPos (postion, worldSize, tpos);
			if (tpos.x >= mDataSize_x || (tpos.y>>1) >= mDataSize_y || tpos.x < 0 || tpos.y < 0 ) 
			{
				Debug.LogError("postion is error! GetBiom Failed ");
				return EBiome.Grassland;
			}

			byte b = mData[tpos.x][tpos.y>>1];
			int n = ((tpos.y&1) == 0) ? (b & 0xf0) >> 4 : b & 0x0f;  // 偶数取高4位  奇数取低4位
			switch (n)
			{
			case 0: return EBiome.Grassland;
			case 1: return EBiome.Desert;
			case 2: return EBiome.Mountainous;
			case 3: return EBiome.Canyon;
			case 4: return EBiome.Forest;
			case 5: return EBiome.Jungle;
			case 6: return EBiome.Marsh;
			case 7: return EBiome.Volcano;
			default: return EBiome.Grassland;
			}
		}
	}
}