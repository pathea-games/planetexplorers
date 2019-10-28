using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
// A Layered-subterrain node game object
public class LSubTerrainGo : MonoBehaviour
{
	LSubTerrain _nodeData;

	// Debug vars only !! Can NOT be used out of unity editor !!
	public int _Index;
	public bool _Finished;
	public int _DataLength;
	public int _TreeCount;
	public int _MapKeyCount;

	private LSubTerrainGo(){}
	void Update ()
	{
		if ( Application.isEditor )
		{
			_Index = _nodeData.Index;
			_Finished = _nodeData.FinishedProcess;
			_DataLength = _nodeData.DataLen;			
			_TreeCount = _nodeData.TreeCnt;
			_MapKeyCount = _nodeData.MapKeyCnt;
			if(_DataLength == 0){
				Destroy(gameObject);
			}
		}		
		#if false
		if ( Application.isEditor )
		{
			// Draw a square in the scene
			Vector3 _va = _nodeData.wPos + (Vector3.right + Vector3.forward) * 4F;
			Vector3 _vb = _nodeData.wPos + Vector3.right * (LSubTerrConstant.SizeF - 4F) + Vector3.forward * 4F;
			Vector3 _vc = _nodeData.wPos + (Vector3.right + Vector3.forward) * (LSubTerrConstant.SizeF - 4F);
			Vector3 _vd = _nodeData.wPos + Vector3.forward * (LSubTerrConstant.SizeF - 4F) + Vector3.right * 4F;
			Color _linec = _nodeData.FinishedProcess ? Color.green : Color.white;
			Debug.DrawLine(_va, _vb, _linec);
			Debug.DrawLine(_vb, _vc, _linec);
			Debug.DrawLine(_vc, _vd, _linec);
			Debug.DrawLine(_vd, _va, _linec);
		}
		#endif
	}

	public static LSubTerrainGo CreateNodeGo(LSubTerrain nodeData)
	{
		if (nodeData != null) {
			GameObject go = new GameObject ();
			LSubTerrainGo nodeGo = go.AddComponent<LSubTerrainGo> ();
			nodeGo._nodeData = nodeData;
			nodeGo.transform.parent = LSubTerrainMgr.GO.transform;
			nodeGo.name = "L SubTerrain Node [" + nodeData.xIndex + "," + nodeData.zIndex + "]";
			nodeGo.transform.position = nodeData.wPos;
			nodeGo.transform.rotation = Quaternion.identity;
			return nodeGo;
		}
		return null;
	}
}
#endif
