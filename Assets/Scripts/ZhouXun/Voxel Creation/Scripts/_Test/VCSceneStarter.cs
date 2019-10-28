using UnityEngine;
using System.Collections;

public class VCSceneStarter : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		LocalDatabase.LoadAllData();
		SurfExtractorsMan.CheckGenSurfExtractor();
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		SurfExtractorsMan.PostProc();
	}

	void OnDestroy()
	{
		SurfExtractorsMan.CleanUp();
		LocalDatabase.FreeAllData();
	}
}
