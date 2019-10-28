using UnityEngine;
using System.Collections;

public class AdjustGrave : MonoBehaviour {

	private float checkTime;
    private Renderer grave;
	void Start () 
    {
        checkTime = Time.time;
        grave = GetComponent<Renderer>();
        if (grave == null)
            GameObject.Destroy(this);
	}
	
	void FixedUpdate () {
        if (Time.time - checkTime > 5)
        {
            checkTime = Time.time;
            Vector3 upPos;
            if (KillNPC.IsBurried(transform.position, out upPos))
            {
                transform.position = upPos;
                grave.enabled = true;
            }
            else
                grave.enabled = false;
        }
	}
}
