using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// story mode only
public class GlobalTreeInfo
{
	public GlobalTreeInfo( int index, TreeInfo treeinfo )
	{
		_terrainIndex = index;
		_treeInfo = treeinfo;
	}
	public GlobalTreeInfo( int xindex, int zindex, TreeInfo treeinfo )
	{
		_terrainIndex = LSubTerrUtils.PosToIndex(xindex,zindex);
		_treeInfo = treeinfo;
	}
	public int _terrainIndex;
	public TreeInfo _treeInfo;
	public Vector3 WorldPos
	{
		get
		{
			if (_terrainIndex < 0)
				return _treeInfo.m_pos;
			return LSubTerrUtils.TreeTerrainPosToWorldPos(_terrainIndex, _treeInfo.m_pos);
		}
	}

	Vector3 _treeCapCenterPos;
	public Vector3 TreeCapCenterPos
	{
		get{return _treeCapCenterPos;}
		set{_treeCapCenterPos = value;}
	}
	

	private  List<PickPersonInfo> mPickInfo = new List<PickPersonInfo>();
	public bool HasCreatPickPos { get{return (mPickInfo != null && mPickInfo.Count >0) ? true : false;}}
	
	public bool CreatCutPos(Vector3 center,Vector3 dir,float radiu,float pointNum = 3.0f)
	{
		//center = Vector3.ProjectOnPlane(center);
		Quaternion Trianglerota= Quaternion.LookRotation(dir);
		float angle = 360/pointNum;

		Quaternion r0= Quaternion.Euler(Trianglerota.eulerAngles.x,Trianglerota.eulerAngles.y + angle,Trianglerota.eulerAngles.z);
		Quaternion r2= Quaternion.Euler(Trianglerota.eulerAngles.x,Trianglerota.eulerAngles.y - angle,Trianglerota.eulerAngles.z);

		Vector3 f0 =  (center  + (r0 *dir) * (radiu + 0.2f));
		Vector3 f2 =  (center  + (r2 *dir) * (radiu + 0.2f));

		mPickInfo.Add(new PickPersonInfo(0,f0));
		mPickInfo.Add(new PickPersonInfo(0,f2));

		return mPickInfo.Count == pointNum-1;
	}

    //public Bounds GetWorldBounds(Bounds local, Transform trans)
    //{
    //    Vector3 size = trans.rotation * local.size;
    //    size.x = Mathf.Abs(size.x);
    //    size.y = Mathf.Abs(size.y);
    //    size.z = Mathf.Abs(size.z);
    //    return new Bounds(trans.position + trans.rotation * local.center, size);
    //}
	
	public bool AddCutter(int entityid,out Vector3 cutPos)
	{
		cutPos = Vector3.zero;
		for(int i=0;i<mPickInfo.Count;i++)
		{
			if(mPickInfo[i]._entityId == entityid)
			{
				cutPos = mPickInfo[i]._PickPos;
				mPickInfo[i]._entityId = entityid;
				return true;
			}
		}

		for(int i=0;i<mPickInfo.Count;i++)
		{
			if(mPickInfo[i]._entityId == 0)
			{
				cutPos = mPickInfo[i]._PickPos;
				mPickInfo[i]._entityId = entityid;
				return true;
			}
		}

		return false;
	}

}

public class PickPersonInfo
{
	 public int _entityId;
	 public Vector3 _PickPos;
	 public PickPersonInfo(int id,Vector3 pos)
	 {
		_entityId = id;
		_PickPos = pos;
	 }

}
