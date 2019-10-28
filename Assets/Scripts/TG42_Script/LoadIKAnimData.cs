using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class LoadIKAnimData : MonoBehaviour
{    
    // Use this for initialization
    public int m_monsterID = 1009;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
		
    }
	
	Transform RecursionFindTransform (Transform _trans, string _strName)
	{
		Transform _transFind = _trans.Find (_strName);
		if (_transFind != null)
			return _transFind;
		
		for (int _i = 0; _i != _trans.childCount; ++_i) {
			_transFind = RecursionFindTransform (_trans.GetChild (_i), _strName);
			if (_transFind != null)
				break;
		}
		
		return _transFind;
	}
	
    public bool OnLoadIKAnimData()
    {

		//设置默认动画为idle
		GetComponent<Animation>().clip = GetComponent<Animation>().GetClip("idle");
		
		//设置spider的ik信息
		//ground info and root bone
		LegController legController = GetComponent<LegController>();
		legController.groundPlaneHeight = 0.0f;
		legController.groundedPose = GetComponent<Animation>().GetClip ("idle");
		legController.rootBone = transform.Find ("Bone02");
		
		//legs info
		legController.legs = new LegInfo[4];
		
		LegInfo _legInfo = new LegInfo ();
		_legInfo.hip = RecursionFindTransform (transform, "Bone83");
		_legInfo.ankle = RecursionFindTransform (transform, "Bone90");
		_legInfo.toe = RecursionFindTransform (transform, "Bone90");
		_legInfo.footLength = 0.5f;
		_legInfo.footWidth = 0.5f;
		_legInfo.footOffset = new Vector2 (0.0f, 0.0f);
		legController.legs[0] = _legInfo;
		
		_legInfo = new LegInfo ();
		_legInfo.hip = RecursionFindTransform (transform, "Bone43");
		_legInfo.ankle = RecursionFindTransform (transform, "Bone50");
		_legInfo.toe = RecursionFindTransform (transform, "Bone50");
		_legInfo.footLength = 0.5f;
		_legInfo.footWidth = 0.5f;
		_legInfo.footOffset = new Vector2 (0.0f, 0.0f);
		legController.legs[1] = _legInfo;
		
		_legInfo = new LegInfo ();
		_legInfo.hip = RecursionFindTransform (transform, "Bone26");
		_legInfo.ankle = RecursionFindTransform (transform, "Bone33");
		_legInfo.toe = RecursionFindTransform (transform, "Bone33");
		_legInfo.footLength = 0.5f;
		_legInfo.footWidth = 0.5f;
		_legInfo.footOffset = new Vector2 (0.0f, 0.0f);
		legController.legs[2] = _legInfo;
		
		_legInfo = new LegInfo ();
		_legInfo.hip = RecursionFindTransform (transform, "Bone34");
		_legInfo.ankle = RecursionFindTransform (transform, "Bone41");
		_legInfo.toe = RecursionFindTransform (transform, "Bone41");
		_legInfo.footLength = 0.5f;
		_legInfo.footWidth = 0.5f;
		_legInfo.footOffset = new Vector2 (0.0f, 0.0f);
		legController.legs[3] = _legInfo;
		

        return true;
    }



}
