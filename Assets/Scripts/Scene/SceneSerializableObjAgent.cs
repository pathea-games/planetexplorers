using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public interface ISceneSerializableObjAgent : ISceneObjAgent, ISerializable{}

public class SceneObjAdditionalSaveData : ISerializable
{
	/// save data struct
	/// --nData--------number of additional datas
	/// --then each type data block
	/// ----typekey(may be typename or typename+goname)
	/// ----nbytes for this type
	/// ----datas for this type
	Dictionary<int,byte[]> _datas = new Dictionary<int, byte[]>();
	public void Serialize(BinaryWriter bw)
	{
		int nData = _datas.Count;
		bw.Write(nData);
		foreach(KeyValuePair<int,byte[]> it in _datas)
		{
			bw.Write(it.Key);
			
			PETools.Serialize.WriteBytes(it.Value, bw);
		}
	}
	public void Deserialize(BinaryReader br)
	{
		_datas.Clear();
		int nData = br.ReadInt32();
		while(nData-- > 0)
		{
			int key = br.ReadInt32();
			
			_datas[key] = PETools.Serialize.ReadBytes(br);
		}
	}
	public void CollectData(GameObject rootGo)
	{
		if(rootGo != null)
		{
			MonoBehaviour[] lstDatas = rootGo.GetComponentsInChildren<MonoBehaviour>();
			foreach(MonoBehaviour m in lstDatas)
			{
				ISaveDataInScene s = m as ISaveDataInScene;
				if(s != null)
				{
					string strKey = m.gameObject.name + s.GetType().Name;
					int key = strKey.GetHashCode();
					_datas[key] = s.ExportData();
				}
			}
		}
	}
	public void DispatchData(GameObject rootGo)
	{
		if(rootGo != null)
		{
			MonoBehaviour[] lstDatas = rootGo.GetComponentsInChildren<MonoBehaviour>();
			foreach(MonoBehaviour m in lstDatas)
			{
				ISaveDataInScene s = m as ISaveDataInScene;
				if(s != null)
				{
					string strKey = m.gameObject.name + s.GetType().Name;
					int key = strKey.GetHashCode();
					byte[] buff;
					if(_datas.TryGetValue(key, out buff))
					{
						s.ImportData(buff);
					}
				}
			}
		}
	}
}

public class SceneSerializableObjAgent : SceneBasicObjAgent, ISceneSerializableObjAgent //save load
{
	protected SceneObjAdditionalSaveData _additionalData = new SceneObjAdditionalSaveData();

	public SceneSerializableObjAgent(){}
	public SceneSerializableObjAgent(string pathPreAsset, string pathMainAsset, Vector3 pos, Quaternion rotation, Vector3 scale, int id = SceneMan.InvalidID)
		: base(pathPreAsset, pathMainAsset, pos, rotation, scale, id){}	

	public virtual void Serialize(BinaryWriter bw)
	{
		bw.Write(_id);
		bw.Write(_pos.x);
		bw.Write(_pos.y);
		bw.Write(_pos.z);
		bw.Write(_scl.x);
		bw.Write(_scl.y);
		bw.Write(_scl.z);
		bw.Write(_rot.x);
		bw.Write(_rot.y);
		bw.Write(_rot.z);
		bw.Write(_rot.w);
		bw.Write(_pathPreAsset);
		bw.Write(_pathMainAsset);
		_additionalData.CollectData(_go);
		_additionalData.Serialize(bw);
	}
	public virtual void Deserialize(BinaryReader br)
	{
		_id = br.ReadInt32();
		_pos.x = br.ReadSingle();
		_pos.y = br.ReadSingle();
		_pos.z = br.ReadSingle();
		_scl.x = br.ReadSingle();
		_scl.y = br.ReadSingle();
		_scl.z = br.ReadSingle();
		_rot.x = br.ReadSingle();
		_rot.y = br.ReadSingle();
		_rot.z = br.ReadSingle();
		_rot.w = br.ReadSingle();
		_pathPreAsset = br.ReadString();
		_pathMainAsset = br.ReadString();
		_additionalData.Deserialize(br);
		if(_go == null)	TryLoadPreGo();
		_additionalData.DispatchData(_go);
	}	

	public override void OnMainGoLoaded()
	{
		base.OnMainGoLoaded ();
		_additionalData.DispatchData(_mainGo);
	}
	public override void OnMainGoDestroy()
	{
		base.OnMainGoDestroy ();
		_additionalData.CollectData(_mainGo);
	}
}

