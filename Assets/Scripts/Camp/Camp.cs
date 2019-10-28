using UnityEngine;
using Mono.Data.SqliteClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pathea;

// so Npc or others can find each other easily.
// TODO : Implement an interface based on NPC or other AI's request

public enum EPosType
{
	Medical =1,
	Repair,
	Computer,
	Tent,
	Max
}

public class SleepPostion
{
	public Vector3 _Doorpos;
	public Vector3 _Pos;
	public float _Rate;

	int id;
	public int _Id
	{
		set{id = value;}
		get {return id;}
	}
	
	bool m_Occpyied;
	public bool Occpyied
	{
		get{return m_Occpyied;}
		set{m_Occpyied = value;}
	}
}

public struct TimeSlot
{
	public float MaxTime;
	public float MinTime;

	public TimeSlot(float _MinTime,float _MaxTime)
	{
		MaxTime = _MaxTime + UnityEngine.Random.Range(0.1f,0.2f);
		MinTime = _MinTime + UnityEngine.Random.Range(0.1f,0.2f);
	}

	public bool InTheRange(float slot)
	{
		return slot >= MinTime && slot <= MaxTime;
	}
	
}

public struct RandIntDb
{
	int minTimes;
	int maxTimes;
	public RandIntDb(int min,int max)
	{
		minTimes = min;
		maxTimes = max;
	}

	public int Random()
	{
		return UnityEngine.Random.Range(minTimes, maxTimes);
	}
}


public class EatDesc
{
	public int assesID;
	public List<TimeSlot> Eattimes;
	public CircleTrans mEatCircleTrans;
	public EatDesc()
	{
		Eattimes = new List<TimeSlot>();
	}

	public bool InTheRange(float slot)
	{
		if(Eattimes == null || Eattimes.Count <=0)
			return false;

		for (int i = 0; i < Eattimes.Count; i++) {
			if (Eattimes [i].InTheRange (slot))
				return true;
		}
		return false;
	}

	public void SetCircleTrans(Vector3 center,float radiu = 3.0f)
	{
		mEatCircleTrans = new CircleTrans(center,radiu);

		if(mEatCircleTrans.mCircleTransList == null || mEatCircleTrans.mCircleTransList.Count <=0)
			mEatCircleTrans.DisCircletarns(4,radiu);
	}

//	public Vector3 GetEmptyPostion(int enityId)
//	{
//		foreach(CircleTran cir in mEatCircleTrans.mCircleTransList)
//		{
//			if(!cir.IsOccpied)
//			{
//				cir.Occpy(enityId);
//				return cir.Postion;
//			}
//		}
//		return Vector3.zero;
//	}

	public Vector3 GetEmptyPostion(int _enityId)
	{
		if(mEatCircleTrans != null)
			return mEatCircleTrans.CalculateEmptyPostion(_enityId);
		else
			return Vector3.zero;
	}
}

#region CircleTran
public class CircleTran
{
	public Vector3 CenterPos{get{return _centerPos;}}
	public Vector3 Postion{get{return _Postion;}}
	public Quaternion Rotation{get{return _Rotation;}}
	public bool IsOccpied{get{return _IsOccpied;}set{_IsOccpied = value;}}
	public int OccpiedId{get{return _OccpiedId;}}
	Vector3 _centerPos;
	Vector3 _Postion;
	Quaternion _Rotation;
	bool _IsOccpied;
	int _OccpiedId;

	public CircleTran(Vector3 Center, Vector3 postion,Quaternion rotation,int occpiedId = 0,bool isOccpied = false)
	{
		_centerPos = Center;
		_Postion = postion;
		_Rotation = rotation;
		_OccpiedId = occpiedId;
		_IsOccpied = isOccpied;
	}

	public void Occpy(int Id)
	{
		_OccpiedId = Id;
		_IsOccpied = true;
	}

	public void Release()
	{
		_OccpiedId = 0;
		_IsOccpied = false;
	}

	public bool CantainId(int Id)
	{
		return _OccpiedId == Id;
	}

}

public class CircleTrans
{
	List<CircleTran> _mCircleTransList = new List<CircleTran>();
	Vector3 _center = new Vector3();
	public List<CircleTran> mCircleTransList
	{
		get
		{
//			if(_mCircleTransList.Count ==0)
//			{
//				List<Vector3> Postions =new List<Vector3>();
//				Postions=GetCirclePosition(_center,UnityEngine.Random.Range(3, 8),3.0f);
//				foreach(Vector3 pos in Postions)
//				{
//					Quaternion rota = new Quaternion();
//					rota.SetFromToRotation(pos,_center);
//					
//					CircleTran circleTran = new CircleTran(pos,rota);
//					mCircleTransList.Add(circleTran);
//				}
//			}
			return _mCircleTransList;
		}
	}
	List<Vector3> Postions;
	public bool DisCircletarns(int Num,float radius = 3.0f)
	{

		  if(Postions== null)
			Postions =new List<Vector3>();

		Postions=GetCirclePosition(_center,Num,radius);
		foreach(Vector3 pos in Postions)
		{
			Quaternion rota = new Quaternion();
			rota.SetFromToRotation(_center,pos);
			
			CircleTran circleTran = new CircleTran(_center,pos,rota);
			mCircleTransList.Add(circleTran);
		}
		return Postions.Count >0;
	}
	
	public CircleTrans(Vector3 center,float radius = 3.0f)
	{
		_center = center;
	}

	Vector3 GetHorizontalDir() 
	{
		float r = UnityEngine.Random.Range(0f, 2 * Mathf.PI);
		return new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r));
	}
	
	List<Vector3> GetCirclePosition(Vector3 center,int num,float radius)
	{
		List<Vector3> result = new List<Vector3>();
		float closeDistance = Mathf.Sin(Mathf.PI / num) * radius * 2;
		int n = 0;
		while (result.Count < num)
		{
			n++;
			Vector3 tmp = GetHorizontalDir();
			bool isClose = false;
			foreach (var item in result)
			{
				if (Vector3.Distance(item, center + Vector3.up + tmp * radius) < closeDistance * 0.5f)
				{
					isClose = true;
					break;
				}
			}
			if (isClose)
				continue;
			if (!Physics.Raycast(center + Vector3.up, tmp + Vector3.up * 0.5f, radius))
			{
				if (Physics.Raycast(center + (Vector3.up * radius / 2) + tmp * radius, Vector3.down, 3))
					result.Add(center + Vector3.up + tmp * radius);
			}
			if (n >= 100)
				break;
		}
		
		if(result.Count == 0)
			return result;
		
		while(result.Count < num)
		{
			for (int i = 0; i < result.Count; i++)
			{
				result.Add((center + Vector3.up + result[i]) / 2);
				if (result.Count >= num)
					break;
			}
		}
		return result;
	}

	public bool OccupyPostion(int enityId)
	{
		foreach(CircleTran tran in mCircleTransList)
		{
			if(!tran.IsOccpied)
			{
				tran.Occpy(enityId);
				return true;
			}
		}
		return false;
	}

	public bool ReleasePostion(int enityid)
	{
		foreach(CircleTran tran in mCircleTransList)
		{
			if(tran.CantainId(enityid))
			{
				tran.Release();
				return true;
			}
		}
		return false;
	}

	public bool CantainEnityID(int enityID)
	{
		
		foreach(CircleTran tran in mCircleTransList)
		{
			if(tran.CantainId(enityID))
				return true;
		}
		return false;
	}

	public Vector3 CalculateEmptyPostion(int enityId)
	{
		foreach(CircleTran cir in mCircleTransList)
		{
			if(!cir.IsOccpied)
			{
				cir.Occpy(enityId);
				return cir.Postion;
			}
		}
		return Vector3.zero;
	}

}

#endregion

#region Camp
public class Camp
{
	public const int UndefinedId = 99999;
	public const string UndefinedName = "Undefined";

	int _id;

	int _nameID;
	Vector3 _pos;
	float _radius;
	//List<int> _assetIds;
	// Patrol path
	//List<SceneAssetDesc> _assets;
	List<SleepPostion> _LayDatas;

	public List<SleepPostion> LayDatas {get{return _LayDatas;}}
	public EatDesc mEatInfo = new EatDesc();
	public Vector3 mTalkCenterPos = new Vector3();
	public List<TimeSlot> mTalkTime = new List<TimeSlot>();
	public float mSleepTime;
	public float mWakeupTime;
	public List<int> m_PreLimit = new List<int>();
	public List<int> m_TalkList = new List<int>();
	public  string[] mPaths;
	public int[] MedicalType;
	public int[] RepairType;
	public int[] ComputerType;
	public int[] TentType;

	bool mCampIsActive = false;
	bool mCampInAlert =false;
	List<int> _NpcentityIds = new List<int>();
	public  List<int> NpcentityIds {get{return _NpcentityIds;}}

	public int Id { 		get { return _id; } 
							set{ _id = value; } }
	public float Radius { 	get { return _radius; } 
							set{ _radius = value; } }
	public Vector3 Pos { 	get { return _pos; } 
							set{ _pos = value; } }
	public bool CampIsActive
	{
		get{return mCampIsActive;}
		set{mCampIsActive = value;}
	}

    public string Name
    {
        get { return PELocalization.GetString(_nameID); }
    }
	public Camp(){}
	public Camp(int id, int nameID, Vector3 pos, float radius, List<int> assetIds)
	{
		_id = id;
		_nameID = nameID;
		_pos = pos;
		_radius = radius;
		//_assetIds = assetIds;
	}
//	public void Add(SceneAssetDesc obj)
//	{
//		_assets.Add(obj);
//	}

	public string GetPath(int index)
	{
		if(mPaths == null || index >= mPaths.Length)
			return null;

		return mPaths[index];
	}
	public Vector3 GetObjectPostion(int asseId)
	{
		if(asseId == 0)
			return Vector3.zero;

		if(StoryDoodadMap.s_dicDoodadData != null)
		   return StoryDoodadMap.s_dicDoodadData[asseId]._pos;
		else
			return Vector3.zero;
	}

	public  void AddNpcIntoCamp(int enityId)
	{
		if(_id == 1)
			return ;

		if(_NpcentityIds.Contains(enityId))
			return;
		//test Player as campiseNPc
		if(PeCreature.Instance != null && PeCreature.Instance.mainPlayer !=null && PeCreature.Instance.mainPlayer.Id == enityId)
			return;

		PeEntity npc = EntityMgr.Instance.Get(enityId);
		if(npc == null||  npc.entityProto == null || npc.entityProto.proto == EEntityProto.Player 
		   || npc.entityProto.proto ==EEntityProto.Monster || npc.entityProto.proto ==EEntityProto.Tower)
			return ;

		_NpcentityIds.Add(enityId);
		UpdateNpcAlert(mCampInAlert);
		return;
	}

	public Vector3 CalculatePostion(int SelfId,Vector3 SelfPos,float Radius)
	{
		foreach(int NpcId in _NpcentityIds)
		{
			if(NpcId == SelfId)
				continue;

			PeEntity npc = EntityMgr.Instance.Get(NpcId);
			if(npc != null && !npc.Equals(this))
			{
				if((SelfPos -npc.position).sqrMagnitude >= 1.0f*1.0f && (SelfPos -npc.position).sqrMagnitude < Radius * Radius )
					return npc.position;
			}
		}
		return Vector3.zero;
	}

	public bool CalculatePostion(PeEntity Self,float Radius)
	{
		PeEntity _target = null;
		foreach(int NpcId in _NpcentityIds)
		{
			if(NpcId == Self.Id)
				continue;
			
			if(GetRoundNpc(Self.Id,NpcId,Radius,out _target))
			{
				Self.NpcCmpt.ChatTarget = _target;
				return true;
			}
		}
		Self.NpcCmpt.ChatTarget =null;
		return false;
	}
	public bool CalculatePostion(PeEntity Self,float Radius,out PeEntity _target)
	{
		foreach(int NpcId in _NpcentityIds)
		{
			if(NpcId == Self.Id)
				continue;

			if(GetRoundNpc(Self.Id,NpcId,Radius,out _target))
			{
				return true;
			}
		}
		_target = null;
		return false;
	}

	bool GetRoundNpc(int slefId,int targetId,float radius,out PeEntity Target)
	{
		Target = null; 
		PeEntity self = EntityMgr.Instance.Get(slefId);
		if(null == self)
			return false;

		PeEntity target = EntityMgr.Instance.Get(targetId);
		if(null == target)
			return false;


		//Vector3 dir = new Vector3(.x, 0.0f, direction.z);
//		Vector3 dir = Quaternion.AngleAxis(UnityEngine.Random.Range(-30.0f, 180), Vector3.up) * self.peTrans.forward;
//

		float sqrDistanceH = PETools.PEUtil.SqrMagnitudeH(self.peTrans.position, target.peTrans.position);
		if(sqrDistanceH < radius *radius)
		{
			float angle =  Vector3.Angle(self.peTrans.trans.forward,target.peTrans.trans.forward);
			if(angle > -90.0f && angle< 90.0f)
			{
				Target = target;
				return true;
			}
		}
//		if((self.position -target.position).sqrMagnitude >= 1.0f*1.0f && (self.position -target.position).sqrMagnitude < radius * radius )
//		{
//			Target = target;
//			return true;
//		}

		return false;
	}

	public bool CantainTarget(float _radiu,PeEntity self,PeEntity target)
	{
		if((self.position -target.position).sqrMagnitude >= 1.0f*1.0f && (self.position -target.position).sqrMagnitude < _radiu * _radiu )
		{
			return true;
		}
		return false;
	}

	public  void RemoveFromCamp(int enityId)
	{
		if(_NpcentityIds == null)
			return;
		if(_NpcentityIds.Contains(enityId))
		  _NpcentityIds.Remove(enityId);

		return ;

	}

	public SleepPostion HasSleep(int enityid)
	{
		for(int i=0;i<LayDatas.Count;i++)
		{
			if(LayDatas[i]._Id == enityid)
				return LayDatas[i];
		}
		return null;
	}

	public int[] GetPosByType(EPosType type)
	{
		switch(type)
		{
		case EPosType.Medical: return MedicalType;
		case EPosType.Repair:  return RepairType;
		case EPosType.Computer:return ComputerType;
		case EPosType.Tent : return TentType;
		default:
				return null;
		}
		//return null;
	}

	public void SetCampNpcAlert(bool value)
	{
		mCampInAlert = value;
		UpdateNpcAlert(value);
	}

	public void UpdateNpcAlert(bool value)
	{
		if(_NpcentityIds == null)
			return;
		
		for(int i=0;i<_NpcentityIds.Count;i++)
		{
			PeEntity peentity = EntityMgr.Instance.Get(_NpcentityIds[i]);
			if(peentity != null)
			{
				peentity.SetNpcAlert(value);
			}
		}
	}

	// Camp desc solver and camp management
	static List<Camp> _camps = new List<Camp>();
	public List<Camp> Camps{ 			get{ return _camps; 	} }
	public static Camp GetCamp(int id)
	{
		int n = _camps.Count;
		for (int i = 0; i < n; i++) {
			if(_camps[i].Id == id){
				return _camps[i];
			}
		}
		return null;
	}
    public static Camp GetCamp(Vector3 pos)
    {
		int n = _camps.Count;
		for (int i = 0; i < n; i++) {
			if(_camps[i].CampIsActive && (_camps[i].Pos - pos).sqrMagnitude < _camps[i].Radius * _camps[i].Radius){
				return _camps[i];
			}
		}
		return null;
    }

	public static bool SetCampActive(int campid,bool isActive)
	{
		Camp camp = GetCamp(campid);
		if(camp == null)
			return false;
		
		camp.CampIsActive = isActive;
		return true;
	}

	public static void LoadData()
	{
		_camps.Clear();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("sceneCampList");
		//reader.Read(); // skip title if needed
		while (reader.Read())
		{
			int id = Convert.ToInt32(reader.GetString(0));
            //lz-2016.09.23 这个字段只有蒲及用了，可以直接改成翻译映射ID
			int nameID = Convert.ToInt32((reader.GetString(1)));
			string[] pos = reader.GetString(2).Split(',');
			Vector3 vPos = new Vector3(
				Convert.ToSingle(pos[0]),
				Convert.ToSingle(pos[1]),
				Convert.ToSingle(pos[2]));
			float radius = Convert.ToSingle(reader.GetString(3));
			List<int> assetIds = StrToCampAssetDesc(reader.GetString(4));
			Camp _camp = new Camp(id, nameID, vPos, radius, assetIds);

			string strTmp = reader.GetString(reader.GetOrdinal("Sleep"));
			string[] tmpList;
			tmpList = strTmp.Split('_');
			if (tmpList.Length == 2)
			{
				_camp.mSleepTime = Convert.ToSingle(tmpList[0])  + UnityEngine.Random.Range(0.1f,0.2f);
				_camp.mWakeupTime = Convert.ToSingle(tmpList[1]) + UnityEngine.Random.Range(0.1f,0.2f);
			}
			string[] tmpList1;
			strTmp = reader.GetString(reader.GetOrdinal("Eat"));
			if(strTmp != "0")
			{
				tmpList = strTmp.Split(':');
				_camp.mEatInfo.assesID = Convert.ToInt32(tmpList[0]);
				tmpList1 = tmpList[1].Split(',');
				string[] tmpList1_1;
				for (int i = 0; i < tmpList1.Length; i++)
				{
					if (tmpList1[i] == "0")
						continue;
					
					tmpList1_1 = tmpList1[i].Split('_');
					if (tmpList1_1.Length != 2)
						continue;
					
					_camp.mEatInfo.Eattimes.Add(new TimeSlot(Convert.ToSingle(tmpList1_1[0]),Convert.ToSingle(tmpList1_1[1])));
				}
			}


			strTmp = reader.GetString(reader.GetOrdinal("SleepPos"));
			_camp._LayDatas = StrToSleepPostion(strTmp);

			strTmp = reader.GetString(reader.GetOrdinal("PatrolPos"));
			if(strTmp != "0")
			{
			  _camp.mPaths = strTmp.Split(';');
			}
		
			_camp.MedicalType  = PETools.PEUtil.ToArrayInt32(reader.GetString(9),',');
			_camp.RepairType   = PETools.PEUtil.ToArrayInt32(reader.GetString(10),',');
			_camp.ComputerType = PETools.PEUtil.ToArrayInt32(reader.GetString(11),',');
			_camp.TentType = PETools.PEUtil.ToArrayInt32(reader.GetString(12),',');
			_camp.CampIsActive = Convert.ToBoolean(PETools.PEUtil.ToArrayByte(reader.GetString(13),',')[0]);
			_camps.Add(_camp);

			CampPathDb.LoadData(_camp.mPaths);
		}
	}


	static List<SleepPostion>  StrToSleepPostion(string _str)
	{
		string[] tmpList = _str.Split(';');
		string[] doorPos ;
		string[] tmpList1;
		string[] tmpList2;
		string[] posStr;
		List<SleepPostion> _Datas = new List<SleepPostion>();

		for(int i=0; i<tmpList.Length; i++)
		{
			tmpList1 = tmpList[i].Split(':');
		    if(tmpList1.Length != 2)
			   continue;

			doorPos = tmpList1[0].Split(',');
			tmpList2 = tmpList1[1].Split('_');
			if(tmpList2.Length != 2)
				continue;
			posStr = tmpList2[0].Split(',');

			SleepPostion lay = new SleepPostion();
			lay._Doorpos = new Vector3(Convert.ToSingle(doorPos[0]),Convert.ToSingle(doorPos[1]),Convert.ToSingle(doorPos[2]));
			lay._Pos = new Vector3(Convert.ToSingle(posStr[0]),Convert.ToSingle(posStr[1]),Convert.ToSingle(posStr[2]));
			lay._Rate = Convert.ToSingle(tmpList2[1]);
			lay.Occpyied = false;
			lay._Id =0;
			_Datas.Add(lay);
		}
		return _Datas;
	}


	static Vector4 StrToCampPosScope(string strPosScopeDesc)
	{
		Vector4 ret = new Vector4();
		string[] strNums = strPosScopeDesc.Split(',');
		if(strNums.Length < 4)
		{
			Debug.LogError("Unexpected CampPosScopeDesc string." + strPosScopeDesc);
			return ret;
		}
		ret.x = Convert.ToSingle(strNums[0]);
		ret.y = Convert.ToSingle(strNums[1]);
		ret.z = Convert.ToSingle(strNums[2]);
		ret.w = Convert.ToSingle(strNums[3]);
		return ret;
	}
	static List<int> StrToCampAssetDesc(string strAssetsDescs)
	{
		List<int> ret = new List<int>();
		string[] strIds = strAssetsDescs.Split(',');
		foreach(string strId in strIds)
		{
			ret.Add(Convert.ToInt32(strId));
		}
		return ret;
	}
}
#endregion
