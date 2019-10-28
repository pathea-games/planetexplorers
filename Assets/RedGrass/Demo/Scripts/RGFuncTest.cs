using UnityEngine;
using System.Collections;
using RedGrass;

public class RGFuncTest : MonoBehaviour 
{
	public RGScene scene;

	public Transform tracer;

	void OnGUI ()
	{
		if (GUI.Button(new Rect(100, 100, 100, 50), "Dirty"))
		{
			Vector3 pos = tracer.transform.position;

//			int cx = Mathf.FloorToInt(pos.x) >> scene.evniAsset.SHIFT;
//			int cz = Mathf.FloorToInt(pos.z) >> scene.evniAsset.SHIFT;
//
//			int key = Utils.PosToIndex(cx, cz);

//			scene.data.Remove((int)pos.x, (int)pos.y + 1, (int)pos.z);
//			scene.data.Remove((int)pos.x, (int)pos.y, (int)pos.z);
//			scene.data.Remove((int)pos.x, (int)pos.y - 1, (int)pos.z);
//			scene.data.Chunks[key].Free();

			for (int y = Mathf.RoundToInt(pos.y) - 10; y < Mathf.RoundToInt(pos.y) + 10;  y ++)
			{
				scene.data.Remove(Mathf.RoundToInt(pos.x), y, Mathf.RoundToInt(pos.z));
			}
		}
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
