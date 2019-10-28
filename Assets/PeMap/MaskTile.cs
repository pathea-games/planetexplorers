using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace PeMap
{
    public class MaskTile
    {
        public int index;
        public int forceGroup;
		public byte type; 

        public MaskTile()
        {
            index = -1;
            forceGroup = -1;
			type = 0xff;
        }

        public static void SerializeTile(uLink.BitStream stream, object obj, params object[] codecOptions)
        {
            MaskTile v = (MaskTile)(obj);
            stream.Write<int>(v.index);
            stream.Write<int>(v.forceGroup);
			stream.Write<byte>(v.type);
        }

        public static object DeserializeTile(uLink.BitStream stream, params object[] codecOptions)
        {
            MaskTile v = new MaskTile();
            v.index = stream.Read<int>();
            v.forceGroup = stream.Read<int>();
			v.type = stream.Read<byte>();
            return v;
        }

        public byte[] Serialize()
        {
            return PETools.Serialize.Export((w) =>
            {
                w.Write(index);
                w.Write(forceGroup);
				w.Write(type);
            });
        }

        public void Deserialize(byte[] data)
        {
            PETools.Serialize.Import(data, (r) =>
            {
                index = r.ReadInt32();
                forceGroup = r.ReadInt32();
				type = r.ReadByte();
            });
        }

        public class Mgr : Pathea.ArchivableSingleton<Mgr>
        {
            public const int mLenthPerArea = 128;
			public int mHalfLength {get {return mLength/2; } }
			public int mLength {get {return  Convert.ToInt32(Mathf.Max(RandomMapConfig.Instance.MapSize.x,RandomMapConfig.Instance.MapSize.y)); }}
			public int mHalfPerSide 
			{ 
				get 
				{ 
					int side = mHalfLength / mLenthPerArea; 
					return mHalfLength % mLenthPerArea == 0 ? side : side+1;  
				}
			}
			public int mNumPerSide {get {return mHalfPerSide * 2;} }

            public int GetMapIndex(Vector3 pos)
            {
                int xIndex = Mathf.FloorToInt(pos.x / mLenthPerArea) + mHalfPerSide;
                int zIndex = Mathf.FloorToInt(pos.z / mLenthPerArea) + mHalfPerSide;
                return xIndex + zIndex * mNumPerSide;
            }

            //lz-2016.06.23 通过位置判断位置是否被探索
            public bool GetIsKnowByPos(Vector3 pos)
            {
                int index=GetMapIndex(pos);
                if (null != maskTiles && maskTiles.ContainsKey(index))
                {
                    return true;
                }
                return false;
            }

			public Vector2 GetCenterPos(int index)
			{
				Vector2 pos = Vector2.zero;
				pos.x = (index % mNumPerSide - mHalfPerSide+0.5f) * mLenthPerArea;
				pos.y = (index / mNumPerSide - mHalfPerSide+0.5f) * mLenthPerArea;
				return pos;
			}

            public List<int> GetNeighborIndex(int centerIndex)
            {
                int xIndex = centerIndex % mNumPerSide;
                int zIndex = centerIndex / mNumPerSide;
                List<int> retList = new List<int>();
                for (int i = xIndex - 1; i <= xIndex + 1; i++)
                {
                    for (int j = zIndex - 1; j <= zIndex + 1; j++)
                    {
                        if (i < 0 || i >= mNumPerSide || j < 0 || j >= mNumPerSide)
                            continue;
                        int index = i + j * mNumPerSide;
                        retList.Add(index);
                    }
                }
                return retList;
            }

            Dictionary<int, MaskTile> maskTiles = new Dictionary<int, MaskTile>();

            public class Args : PeEvent.EventArg
            {
                public bool add;
                public int index;                
            }

            public PeEvent.Event<Args> eventor = new PeEvent.Event<Args>();

            public MaskTile Get(int index)
            {
                if (maskTiles.ContainsKey(index))
                {
                    return maskTiles[index];
                }
                return null;
            }

            public void Add(int index, MaskTile tile)
            {
                maskTiles[index] = tile;

                Args args = new Args();
                args.index = index;
                args.add = true;
                eventor.Dispatch(args, this);
            }

            public bool Remove(int index)
            {
                Args args = new Args();
                args.index = index;
                args.add = false;
                eventor.Dispatch(args, this);

                return maskTiles.Remove(index);
            }


			public void Index()
			{

			}

            public void Tick(Vector3 pos)
            {
                List<int> indexList = GetNeighborIndex(GetMapIndex(pos));

				foreach (int index in indexList)
				{
					MaskTile tile = Get(index);
					if (null == tile)
					{
						Vector2 tilePos = GetCenterPos(index); 
						byte type = GetType((int)tilePos.x,(int) tilePos.y);
						if (Pathea.PeGameMgr.IsMulti)
						{
//							PlayerNetwork.MainPlayer.SyncMapArea(index, type);
						}
						else 
						{
							tile = new MaskTile();
							tile.index = index;
							tile.type = type;
								
							Add(index, tile);
						}
					}
				}	
			}
            /*
			MaskTileTypeDic maskTileDic = new MaskTileTypeDic();

			public byte GetType(int pos_x , int pos_z)
			{
				maskTileDic.Reset();
				for(int x = pos_x - 60 ; x < pos_x + 60; x += 20 )
				{
					for(int z = pos_z - 60 ; z < pos_z + 60; z += 20 )
					{
						if ( VFDataRTGen.IsSea(x,z) )
							maskTileDic.CountType(MaskTileType.Sea);
						else 
						{
							RandomMapType type = VFDataRTGen.GetXZMapType(pos_x,pos_z);
							maskTileDic.CountType( (MaskTileType) type);
						}
					}
				}
				return (byte)maskTileDic.GetMostType();
			}
			*/

            public byte GetType(int pos_x, int pos_z)
            {

                if (VFDataRTGen.IsSea(pos_x, pos_z))
                    return (byte)MaskTileType.Sea;
                else
                {
                    return (byte)VFDataRTGen.GetXZMapType(pos_x, pos_z);
                }
            }

            protected override void WriteData(BinaryWriter w)
			{
				w.Write((int)maskTiles.Count);
                foreach (KeyValuePair<int, MaskTile> kv in maskTiles)
                {
                    PETools.Serialize.WriteBytes(kv.Value.Serialize(), w);
                }
            }

            protected override void SetData(byte[] data)
            {
                PETools.Serialize.Import(data, (r) =>
                {
                    int count = r.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        byte[] buff = PETools.Serialize.ReadBytes(r);
                        if (null != buff && buff.Length > 0)
                        {
                            MaskTile mt = new MaskTile();
                            mt.Deserialize(buff);

                            Add(mt.index, mt);
                        }
                    }
                });
            }

			protected override bool GetYird(){
				return false;
			}
        }
	}

	public enum MaskTileType
	{
		Default = 0,
		GrassLand = 1,
		Forest,
		Desert,
		Redstone,
		Rainforest,
        //lz-2016.08.02 新增山区，沼泽，火山类型
        Mountain,
        Swamp,
        Crater,
        //lz-2016.08.02 因为是RandomMapType类型直接强转换为MaskTileType，所以这两个枚举要往后移
        Sea,
		Max
	}
	
	public class MaskTileTypeDic
	{
		Dictionary<int , int > countDic = new Dictionary<int, int>();
		
		public void Reset()
		{
			for (int i=0;i< (int) MaskTileType.Max;i++)
				countDic[i] = 0;
		}
		
		public void CountType(MaskTileType type)
		{
			countDic[(int)type] += 1;
		}
		
		public MaskTileType GetMostType()
		{
			MaskTileType most = MaskTileType.GrassLand;
			int maxCount = 0;
			foreach (var kv in countDic)
			{
				if (kv.Value > maxCount)
				{
					most = (MaskTileType)kv.Key;
					maxCount = kv.Value;
				}
			}
			return most;
		}
	}
}