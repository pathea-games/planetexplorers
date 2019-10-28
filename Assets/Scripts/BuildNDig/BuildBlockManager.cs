#define BuildBlockAndVoxel
#define NoBlockTest
using ItemAsset;
using ItemAsset.PackageHelper;
using Mono.Data.SqliteClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CreatItemInfo
{
	public int			mItemId;
	public Vector3 		mPos;
	public Quaternion	mRotation;
    internal static object DeserializeItemInfo(uLink.BitStream stream, params object[] codecOptions)
    {
        CreatItemInfo itemInfo = new CreatItemInfo();
        itemInfo.mItemId = stream.Read<int>();
        itemInfo.mPos = stream.Read<Vector3>();
        itemInfo.mRotation = stream.Read<Quaternion>();
        return itemInfo;
    }

    internal static void SerializeItemInfo(uLink.BitStream stream, object value, params object[] codecOptions)
    {
        CreatItemInfo itemInfo = value as CreatItemInfo;
        stream.Write<int>(itemInfo.mItemId);
        stream.Write<Vector3>(itemInfo.mPos);
        stream.Write<Quaternion>(itemInfo.mRotation);

    }
}

public class BlockBuilding
{
	public static Dictionary<int, BlockBuilding> s_tblBlockBuildingMap;
	public int mId;
	public int mDoodadProtoId;
	public string mPath;
    string mNpcIdPosRotStand;
    string mNpcIdPosRotMove;
    public Vector2 mSize;	

	public Vector3 BoundSize = Vector3.zero;

	public static void LoadBuilding()
	{
		s_tblBlockBuildingMap = new Dictionary<int, BlockBuilding>();
		
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Townhouse");
		
		while (reader.Read())
		{
			BlockBuilding addBuilding = new BlockBuilding();
			addBuilding.mId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
			addBuilding.mDoodadProtoId = Convert.ToInt32(reader.GetString(reader.GetOrdinal ("PrototypeDoodad_Id")));
			addBuilding.mPath = reader.GetString(reader.GetOrdinal("FilePath"));
            addBuilding.mNpcIdPosRotStand = reader.GetString(reader.GetOrdinal("NPCId_Num_Stand"));
            addBuilding.mNpcIdPosRotMove = reader.GetString(reader.GetOrdinal("NPCId_Num_Move"));
            string sizeString = reader.GetString(reader.GetOrdinal("Size"));
            string[] sizeStrs = sizeString.Split(',');
            addBuilding.mSize = new Vector2(float.Parse(sizeStrs[0]), float.Parse(sizeStrs[1]));
			s_tblBlockBuildingMap[addBuilding.mId] = addBuilding;
		}
	}
	
	public static BlockBuilding GetBuilding(int id)
	{
		return s_tblBlockBuildingMap[id];
	}
	
	public static BlockBuilding GetBuilding(string fileName)
	{
		foreach(int id in s_tblBlockBuildingMap.Keys)
		{
			if(s_tblBlockBuildingMap[id].mPath.Contains(fileName))
			{
				return s_tblBlockBuildingMap[id];
			}
		}
		return null;
	}
	
	public void GetBuildingInfo(out Vector3 size, out Dictionary<IntVector3, B45Block> blocks, out List<Vector3> npcPosition
		, out List<CreatItemInfo> itemList, out Dictionary<int, BuildingNpc> npcIdPosRot)
	{
		Bounds bound = new Bounds(Vector3.zero,Vector3.zero);
		blocks = new Dictionary<IntVector3, B45Block>();
		npcPosition = new List<Vector3>();
		itemList = new List<CreatItemInfo>();
		npcIdPosRot = new Dictionary<int, BuildingNpc>();
		
		TextAsset textFile = Resources.Load(mPath) as TextAsset;
		MemoryStream ms = new MemoryStream(textFile.bytes);
        BinaryReader _in = new BinaryReader(ms);
		
		int readVersion = _in.ReadInt32();
		switch(readVersion)
		{
		case 2:
			int Size = _in.ReadInt32();
			for(int i = 0; i < Size; i++)
			{
				IntVector3 index = new IntVector3(_in.ReadInt32(),_in.ReadInt32(),_in.ReadInt32());
				B45Block block = new B45Block(_in.ReadByte(),_in.ReadByte());
				if((block.blockType >> 2) != 0)
				{
					blocks[index] = block;
                    index = new IntVector3(index);
                    index.x += 1;
                    index.z += 1;
					bound.Encapsulate(BSBlock45Data.s_Scale * index.ToVector3());
				}
			}
			break;
		}
		
		
        _in.Close();
        ms.Close();
		
		BoundSize = size = bound.size;
		UnityEngine.Object subInfo = Resources.Load(mPath + "SubInfo");
		if(null != subInfo)
		{
			textFile = subInfo as TextAsset;
			ms = new MemoryStream(textFile.bytes);
	        _in = new BinaryReader(ms);
			
			int version = _in.ReadInt32();
			int count = _in.ReadInt32();
			switch(version)
			{
			case 1:
				for(int i = 0; i < count; i++)
					npcPosition.Add(new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle()));
				break;
			case 2:
				for(int i = 0; i < count; i++)
					npcPosition.Add(new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle()));
				count = _in.ReadInt32();
				for(int i = 0; i < count; i++)
				{
					CreatItemInfo addItem = new CreatItemInfo();
					addItem.mPos = new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle());
					addItem.mRotation = Quaternion.Euler(new Vector3(_in.ReadSingle(), _in.ReadSingle(), _in.ReadSingle()));
					addItem.mItemId = _in.ReadInt32();
					itemList.Add(addItem);
				}
				break;
			}
	        _in.Close();
	        ms.Close();
		}

        //npc stand
        if (!mNpcIdPosRotStand.Equals("") && mNpcIdPosRotStand.Length > 1)
        {
            string[] npcs = mNpcIdPosRotStand.Split('_');
            for (int i = 0; i < npcs.Count(); i++)
            {
                string[] npcIdPosRotStr = npcs[i].Split('~');
                int id = Convert.ToInt32(npcIdPosRotStr[0]);
                string[] posRot = npcIdPosRotStr[1].Split(';');
                string[] posStr = posRot[0].Split(',');
                Vector3 pos = new Vector3(float.Parse(posStr[0]), float.Parse(posStr[1]), float.Parse(posStr[2]));
                float rot = float.Parse(posRot[1]);
                BuildingNpc bdnpc = new BuildingNpc(id, pos, rot, true);
                npcIdPosRot.Add(id, bdnpc);
            }
        }

        //npc move
        if (!mNpcIdPosRotMove.Equals("") && mNpcIdPosRotMove.Length > 1)
        {
            string[] npcs = mNpcIdPosRotMove.Split('_');
            for (int i = 0; i < npcs.Count(); i++)
            {
                string[] npcIdPosRotStr = npcs[i].Split('~');
                int id = Convert.ToInt32(npcIdPosRotStr[0]);
                string[] posRot = npcIdPosRotStr[1].Split(';');
                string[] posStr = posRot[0].Split(',');
                Vector3 pos = new Vector3(float.Parse(posStr[0]), float.Parse(posStr[1]), float.Parse(posStr[2]));
                float rot = float.Parse(posRot[1]);
                BuildingNpc bdnpc = new BuildingNpc(id, pos, rot, false);
                npcIdPosRot.Add(id, bdnpc);
            }
        }
    }


    public void GetNpcInfo(out List<BuildingNpc> buildingNpcs)
    {
        buildingNpcs = new List<BuildingNpc>();
        //npc stand
        if (!mNpcIdPosRotStand.Equals("") && mNpcIdPosRotStand.Length > 1)
        {
            string[] npcs = mNpcIdPosRotStand.Split('_');
            for (int i = 0; i < npcs.Count(); i++)
            {
                string[] npcIdPosRotStr = npcs[i].Split('~');
                int id = Convert.ToInt32(npcIdPosRotStr[0]);
                string[] posRot = npcIdPosRotStr[1].Split(';');
                string[] posStr = posRot[0].Split(',');
                Vector3 pos = new Vector3(float.Parse(posStr[0]), float.Parse(posStr[1]), float.Parse(posStr[2]));
                float rot = float.Parse(posRot[1]);
                BuildingNpc bdnpc = new BuildingNpc(id, pos, rot, true);
                buildingNpcs.Add(bdnpc);
            }
        }

        //npc move
        if (!mNpcIdPosRotMove.Equals("") && mNpcIdPosRotMove.Length > 1)
        {
            string[] npcs = mNpcIdPosRotMove.Split('_');
            for (int i = 0; i < npcs.Count(); i++)
            {
                string[] npcIdPosRotStr = npcs[i].Split('~');
                int id = Convert.ToInt32(npcIdPosRotStr[0]);
                string[] posRot = npcIdPosRotStr[1].Split(';');
                string[] posStr = posRot[0].Split(',');
                Vector3 pos = new Vector3(float.Parse(posStr[0]), float.Parse(posStr[1]), float.Parse(posStr[2]));
                float rot = float.Parse(posRot[1]);
                BuildingNpc bdnpc = new BuildingNpc(id, pos, rot, false);
                buildingNpcs.Add(bdnpc);
            }
        }
    }
}

//internal class AreaBlockData
//{
//	internal IntVector2 _index;
//	internal Dictionary<IntVector4, B45Block> _blocks = new Dictionary<IntVector4, B45Block>();
//	
//	internal void Update(Dictionary<IntVector4, B45Block> blocks)
//	{
//		foreach (KeyValuePair<IntVector4, B45Block> kv in blocks)
//		{
//			if (_blocks.ContainsKey(kv.Key))
//			{
//				_blocks[kv.Key].Update(kv.Value);
//			}
//			else
//			{
//				_blocks[kv.Key] = kv.Value;
//			}
//		}
//	}
//}

public class BuildBlockManager : GLBehaviour
{
	public static Vector3 BestMatchPosition(Vector3 pos)
	{
		Vector3 retPos = (pos + 0.001f * Vector3.one) / Block45Constants._scale;
		retPos.x = Mathf.RoundToInt(retPos.x);
		retPos.y = Mathf.RoundToInt(retPos.y);
		retPos.z = Mathf.RoundToInt(retPos.z);
		return retPos * Block45Constants._scale + 0.001f * Vector3.one;
	}

	public override void OnGL ()
	{

	}
}
