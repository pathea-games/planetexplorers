using UnityEngine;
using System.Collections;

public class SleepOjbect : MonoBehaviour {
	[SerializeField] float dalayTime;
	Rigidbody rigid;
	Collider coll;
	MeshRenderer meshRenderer;
	[SerializeField] MonoBehaviour[] scriptsToSleep;

	void Awake()
	{
		rigid = GetComponent<Rigidbody>();
		coll = GetComponent<Collider>();
		meshRenderer = GetComponent<MeshRenderer>();
		scriptsToSleep = GetComponents<MonoBehaviour>();

		rigid.useGravity = false;
		coll.enabled = false;
		meshRenderer.enabled = false;
		foreach(MonoBehaviour i in scriptsToSleep)
				i.enabled = false;
		Invoke("WakeUpObject", dalayTime);
	}

	void WakeUpObject()
	{
		rigid.useGravity = true;
		coll.enabled = true;
		meshRenderer.enabled = true;
		foreach(MonoBehaviour i in scriptsToSleep)		
			i.enabled = true;
		MonoBehaviour.Destroy(this);
	}
}
