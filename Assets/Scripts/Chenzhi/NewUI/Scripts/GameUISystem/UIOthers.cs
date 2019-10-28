using UnityEngine;
using System.Collections;

public class UIOthers : MonoBehaviour 
{

	private static UIOthers mInctence;
	public static UIOthers Inctence{get { return mInctence; }}
	

	
	void Awake()
	{
		mInctence = this;
	}

	void TestRevive()
	{
		GameUI.Instance.mRevive.Show();
	}



//	GameObject AddUIPrefab(GameObject prefab, Transform parentTs )
//	{
//		GameObject o = GameObject.Instantiate(prefab) as GameObject;
//		o.transform.parent = parentTs;  
//		o.layer = parentTs.gameObject.layer;
//		o.transform.localPosition = Vector3.zero;
//		o.transform.localScale = Vector3.one;
//		return o;
//	}

}
