using UnityEngine;
using System.Collections;

public class CreationUILoader : MonoBehaviour
{
	public string m_ResPath = "GUI/Prefabs/Creation/Creation UI";
	
	// Use this for initialization
	void Awake ()
	{
		GameObject go_res = Resources.Load(m_ResPath) as GameObject;
		if ( go_res != null )
		{
			GameObject uigo = GameObject.Instantiate(go_res) as GameObject;
			uigo.name = go_res.name;
			uigo.transform.parent = transform;
			uigo.transform.localPosition = Vector3.zero;
			uigo.transform.localRotation = Quaternion.identity;
			uigo.transform.localScale = Vector3.one;
		}
	}
}
