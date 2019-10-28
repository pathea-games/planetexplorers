using UnityEngine;
using System.Collections;

public class WorldMapBG : MonoBehaviour
{
	public GameObject mMsgtarget;
	
	void OnDrag (Vector2 delta)
	{
		if(Input.GetMouseButton(0))
			mMsgtarget.SendMessage("OnMapDrag",delta, SendMessageOptions.DontRequireReceiver);
	}
}
