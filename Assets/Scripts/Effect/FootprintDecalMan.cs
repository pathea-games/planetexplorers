using UnityEngine;
using System;
using System.Collections;
using NaturalResAsset;

public class FootprintDecalMan : MonoBehaviour
{
	const int CntFootPrintPlayer = 5;
	const int CntFootPrintNPC = 2;
	static GameObject FootPrintGrp = null;
	public Transform FootPrintsParent{
		get{
			if(FootPrintGrp != null || (FootPrintGrp = GameObject.Find("FootPrints")) != null)	
				return FootPrintGrp.transform;
			
			FootPrintGrp = new GameObject("FootPrints");
			FootPrintGrp.transform.parent = transform.parent;
			FootPrintGrp.transform.position = Vector3.zero;
			FootPrintGrp.transform.rotation = Quaternion.identity;
			return FootPrintGrp.transform;
		}
	}

    public Transform[] _lrFoot = new Transform[2];
	public GameObject _fpSeedGoLR;	// lr use the same seed(and texture)
	public float _thresVelOfMove = 0.5f;
	public float _rayLength = 0.5f;

	public HumanPhyCtrl _ctrlr;
	[HideInInspector]public Pathea.MotionMgrCmpt _mmc;

	int _cntFootPrint = 3;
	[HideInInspector]public int _curFoot = 0;	// 0: left; 1: right
	[HideInInspector]public int[] _curFpIdx = new int[2];
	[HideInInspector]public FootprintDecal[,] _fpGoUpdates = null;

	[HideInInspector]public float _fpLastLRFootDistance = 0f;
	[HideInInspector]public bool _fpbPlayerInMove = false;
	[HideInInspector]public bool[] _fpbFootInMove = new bool[2];
	[HideInInspector]public Vector3[] _fpLastFootsPos = new Vector3[2];

	void Start()
	{
		if(_fpSeedGoLR == null)
			return;

		try{
			_cntFootPrint = GetComponentInParent<Pathea.MainPlayerCmpt>() != null ? CntFootPrintPlayer : CntFootPrintNPC;
			_fpGoUpdates = new FootprintDecal[2,_cntFootPrint];

			_mmc = GetComponentInParent<Pathea.MotionMgrCmpt>();
			int sex = _mmc.GetComponent<Pathea.CommonCmpt>().sex == Pathea.PeSex.Male ? 0 : 1;
			Texture tex = Resources.Load(sex==1 ? "Texture2D/footprint_f" : "Texture2D/footprint_m") as Texture;
			_fpSeedGoLR.GetComponent<Renderer>().sharedMaterial.mainTexture = tex;
			FootprintDecalMgr.Instance.Register(this);
		}
		catch{}
	}

	public void UpdateDecals()
	{
		for (int i = 0; i < 2; i++) {
			for(int j = 0; j < _cntFootPrint; j++){
				if(_fpGoUpdates[i,j] != null){
					_fpGoUpdates[i,j].UpdateDecal();
				}
			}
		}
	}
}