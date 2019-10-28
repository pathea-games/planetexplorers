using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

namespace GraphMapping
{	
	public class PeAiSpawnMapping : GraphMap
	{
		private const float m_maxInHeightMap = 955;
		private Dictionary<int,byte> mColorMap;

		// load database, init color map
		private bool LoadAiSpawnDb()
		{
			if (LocalDatabase.Instance == null)
			{
				Debug.LogError("Load AiSpawn Texture falied! LocalDatabase is null!");
				return false;
			}

			mColorMap = new Dictionary<int, byte>();

			SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("aispawn_story");
			while (reader.Read())
			{
				Color32 c32 = new Color32 (
					reader.GetByte(reader.GetOrdinal("R")) ,
					reader.GetByte(reader.GetOrdinal("G")) ,
					reader.GetByte(reader.GetOrdinal("B")) ,
				    reader.GetByte(reader.GetOrdinal("A")) );
				mColorMap[Color32toInt(c32)] = reader.GetByte(reader.GetOrdinal("Id"));    

			}
			return true;
		}

		private int Color32toInt(Color32 c32 )
		{
			return (int)(c32.r | (c32.g<<8) | (c32.b<<16) /*| c32.a<<24*/); 
		}

        private Color32 IntToColor32(int value)
        {
            byte r = (byte)((value & (0xFF << 0 )) >> 0 );
            byte g = (byte)((value & (0xFF << 8 )) >> 8 );
            byte b = (byte)((value & (0xFF << 16)) >> 16);
            //byte a = (byte)((value & (0xFF << 24)) >> 24);

            return new Color32(r, g, b, 0xFF);
        }
	
        private int GetColorDifference(int key1, int key2)
        {
            byte r1 = (byte)((key1 & (0xFF << 0 )) >> 0 );
            byte g1 = (byte)((key1 & (0xFF << 8 )) >> 8 );
            byte b1 = (byte)((key1 & (0xFF << 16)) >> 16);

            byte r2 = (byte)((key2 & (0xFF << 0 )) >> 0 );
            byte g2 = (byte)((key2 & (0xFF << 8 )) >> 8 );
            byte b2 = (byte)((key2 & (0xFF << 16)) >> 16);

            return Mathf.Abs(r1 - r2) + Mathf.Abs(g1 - g2) + Mathf.Abs(b1 - b2);
        }

		// 无效点时 获取颜色最近似的有效点
		private byte GetNearMapID(int _key)
		{
			int key = 0;
			int dis = int.MaxValue;
			foreach(var item in mColorMap)
			{
				int temp = GetColorDifference(_key, item.Key);
				if ( temp < dis )
				{
					key = item.Key;
					dis = temp;
				}
			}
			return mColorMap[key];
		}

		public override void LoadTexData(Texture2D tex)
		{
			if (tex == null)
			{
				Debug.LogError("Load AiSpawn Texture falied! texture is null!");
				return;
			}

			if (!LoadAiSpawnDb())
				return;

			mGraphTexWidth = tex.width;
			mGraphTexHeight = tex.height;
			mDataSize_x = tex.width;
			mDataSize_y = tex.height;
			NewData();

			int count = 0;

			for (int i=0; i<tex.width;i++)
			{
				for(int j=0;j<tex.height;j++)
				{
					Color32 c32 = tex.GetPixel(i,j);
					int key =  Color32toInt(c32);
					if (mColorMap.ContainsKey(key))
						mData[i][j] = mColorMap[key];
					else 
					{
						mData[i][j] = GetNearMapID(key);
						count ++ ;
					}
				}
			}

			Debug.Log("(Load AiSpawn) Error point count:" + count.ToString());
		}
		
		public int  GetAiSpawnMapId(Vector2 postion,Vector2 worldSize)
		{
			if (mData == null)
			{
				Debug.LogError("[Error]Get AiSpawn Faleid!");
				return -1;
			}

			IntVector2 tpos = IntVector2.Tmp;
			GetTexPos (postion, worldSize, tpos);
			if (tpos.x >= mDataSize_x || tpos.y >= mDataSize_y || tpos.x < 0 || tpos.y < 0 ) 
			{
				Debug.LogError("postion is error! GetAiSpawnMapId Failed ");
				return 0;
			}

			return (int) mData[tpos.x][tpos.y];
		}
	}
}