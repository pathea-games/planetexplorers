//------------------------------------------------------------------------------
// by Pugee, 2016-01-22
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea;
using System.IO;


public class ProcessingResultObj:RandomItemObj,ISceneSerializableObjAgent
{
	#region ISerializable implementation
	public void Serialize (BinaryWriter bw)
	{
		BufferHelper.Serialize(bw,genPos);
		BufferHelper.Serialize(bw,position);
		BufferHelper.Serialize(bw,rotation);
		BufferHelper.Serialize(bw,id);
		BufferHelper.Serialize(bw,path);
		BufferHelper.Serialize(bw,isNew);
		int count = items.Length;
		BufferHelper.Serialize(bw,count);
		for(int i=0;i<count;i++)
			BufferHelper.Serialize(bw,items[i]);
	}
	public void Deserialize (BinaryReader br)
	{
		BufferHelper.ReadVector3(br,out genPos);
		BufferHelper.ReadVector3 (br,out position);
		BufferHelper.ReadQuaternion(br,out rotation);
		id = BufferHelper.ReadInt32(br);
		path = BufferHelper.ReadString(br);
		isNew = BufferHelper.ReadBoolean(br);
		int count = BufferHelper.ReadInt32(br);
		items = new int[count];
		for(int i=0;i<count;i++){
			items[i]=BufferHelper.ReadInt32(br);
		}
		
		RandomItemMgr.Instance.AddItemToManager(this);
	}
	#endregion

	public ProcessingResultObj(){}

	public ProcessingResultObj(Vector3 pos, int[] itemIdNum)
	{
		id = 0;
		path = ProcessingConst.RESULT_MODEL_PATH;

		genPos = pos;
		position = pos;
		rotation = Quaternion.Euler(0, (new System.Random()).Next(360), 0);
		isNew = true;

		items = itemIdNum;
	}
	public ProcessingResultObj(Vector3 pos, Quaternion rot, int[] itemIdNum)
	{
		id = 0;
		path = ProcessingConst.RESULT_MODEL_PATH;
		genPos = pos;
		position = pos;
		rotation = rot;
		isNew = true;
		
		items = itemIdNum;
	}
	public new void TryGenObject()
	{
//		if (PeGameMgr.IsMulti) 
//		{
//		}
//		else
//		{
			RandomItemMgr.Instance.AddItemToManager(this);
			SceneMan.AddSceneObj(this);
//		}
	}
}
