//#define FillCell4AutoFillCheck

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Runtime.InteropServices;
using NaturalResAsset;

// TreeInfo has the same structure as UnityEngine.TreeInstance
public class TreeInfo
{
	//static int _n = 0;
	static Stack<TreeInfo> _tiPool = new Stack<TreeInfo> ();
	static TreeInfo()
	{
		// count in test > 57000, so preserve 65536(3M bytes)
		for (int i = 0; i < 65536; i++) {
			_tiPool.Push(new TreeInfo());
		}
	}
	public static TreeInfo GetTI(){
		if (_tiPool.Count > 0) {
			TreeInfo ti;
			lock(_tiPool){	ti = _tiPool.Pop ();	}
			ti._next = null;
			return ti;
		}
		//Debug.Log ("[TreeInfo]:" + (++_n));
		return new TreeInfo ();
	}
	public static void FreeTI(TreeInfo ti){
		lock (_tiPool)	{	_tiPool.Push (ti);		}
	}
	public static void FreeTIs(List<TreeInfo> tis)
	{
		int n = tis.Count;
		lock (_tiPool){
			for (int i = 0; i < n; i++) {
				_tiPool.Push (tis [i]);
			}
		}
	}

	private TreeInfo(){}	// Prohibit public operator new 

    public Color m_clr; //颜色
    public float m_heightScale; //高度scale
    public Vector3 m_pos; //位置---[0-1)
    public int m_protoTypeIdx; //所属的tree原型编号
    public float m_widthScale; //宽度scale
    public Color m_lightMapClr; //光照图颜色

	#region TreeInfoNextExt
	// use next to avoid from too many lists used in treemap
	private TreeInfo _next = null;
	public TreeInfo Next{ get { return _next; } }
	public void AttachTi(TreeInfo ti)
	{
		TreeInfo cur = this;
		while (cur._next != null) {
			cur = cur._next;
		}
		cur._next = ti;
	}
	public TreeInfo RemoveTi(TreeInfo ti)	// Return left treeinfo
	{
		if (ti == this) {
			return this._next;
		} 
		TreeInfo cur = this;
		while (cur._next != null) {
			if(cur._next == ti){
				cur._next = ti._next;
				break;
			}
			cur = cur._next;
		}
		return this;
	}
	public TreeInfo FindTi(Vector3 posInTile)
	{
		TreeInfo cur = this;
		while(cur != null){
			Vector3 pos = cur.m_pos;
			pos.x *= LSubTerrConstant.SizeF;
			pos.y *= LSubTerrConstant.HeightF;
			pos.z *= LSubTerrConstant.SizeF;
			float diff = (posInTile - pos).magnitude;
			if (diff < 0.1f) {
				return cur;
			}
			cur = cur._next;
		}
		return null;
	}
	public static void AddTiToList(List<TreeInfo> tis, TreeInfo ti)
	{
		TreeInfo cur = ti;
		while(cur != null){
			tis.Add(cur);
			cur = cur._next;
		}
	}
	public static bool RemoveTiFromDict(Dictionary<int, TreeInfo> dict, int key, TreeInfo ti)
	{
		TreeInfo tmpTi;
		if ( dict.TryGetValue(key, out tmpTi) )
		{
			TreeInfo leftTi = tmpTi.RemoveTi(ti);
			if(leftTi == null){
				dict.Remove(key);
			} else if(leftTi != tmpTi){
				dict[key] = leftTi;
			}
			return true;
		}
		return false;
	}

	#endregion
	
	public bool IsTree()
	{
		GameObject o = null;
        if (Pathea.PeGameMgr.IsStory)
            o = LSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_protoTypeIdx];
        else if (Pathea.PeGameMgr.IsAdventure)
            o = RSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_protoTypeIdx];

        if (o == null)
            return false;

        CapsuleCollider capCol = o.GetComponent<CapsuleCollider>();
        if (capCol != null)
            return true;
        return false;
    }

    public bool IsDoubleFoot(out Vector3[] footsPos,Vector3 worldPos,Vector3 localScale)
    {
        footsPos = new Vector3[2];

        GameObject o = null;
        if (Pathea.PeGameMgr.IsStory)
            o = LSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_protoTypeIdx];
        else if (Pathea.PeGameMgr.IsAdventure)
            o = RSubTerrainMgr.Instance.GlobalPrototypePrefabList[m_protoTypeIdx];

        if (o == null)
            return false;

        CapsuleCollider capCol = o.GetComponent<CapsuleCollider>();
        BoxCollider boxCol = o.GetComponent<BoxCollider>();
        footsPos[0] = worldPos + new Vector3(capCol.center.x * localScale.x, capCol.center.y * localScale.y, capCol.center.z * localScale.z);
        if (boxCol != null)
        {
            footsPos[1] = worldPos + new Vector3(boxCol.center.x * localScale.x, boxCol.center.y * localScale.y, boxCol.center.z * localScale.z);
            return true;
        }
        return false;
    }
};
