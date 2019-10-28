using UnityEngine;
using System.Collections;

public class CSGuardRangeEffect : MonoBehaviour 
{
	[SerializeField] Projector m_Projector;
	[SerializeField] LocateCubeEffectHanlder m_CubeEffect;

	public float Radius  
	{ 
		get { return m_Projector.orthographicSize; } 

		set 
		{ 
			m_Projector.orthographicSize = value;
			m_Projector.farClipPlane = value * 2;
			m_Projector.transform.localPosition = new Vector3(0, value, 0);
		} 
	}

//	public Vector3 CubeEffectPos 
//	{
//		get { return m_CubeEffect.transform.position; }
//		set {
//			m_CubeEffect.transform.position = new Vector3(value.x, value.y + m_CubeEffect.m_MaxHeightScale * 0.5f, value.z);
//		}
//	}

	public Transform CubeEffectFollower;

	public void DelayDestroy (float delay_time)
	{
		StartCoroutine(_delayDestroy (delay_time));
	}

	IEnumerator _delayDestroy (float delay_time)
	{
		yield return new WaitForSeconds(delay_time);
		GameObject.Destroy(gameObject);
	}

	public void StopDestroy ()
	{
		StopAllCoroutines();
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (CubeEffectFollower != null)
		{
			Vector3 pos = CubeEffectFollower.position;
			m_CubeEffect.transform.position = new Vector3(pos.x, pos.y + m_CubeEffect.m_MaxHeightScale * 0.5f, pos.z);
		}
	}
}
