using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScanningEffectHandler : MonoBehaviour
{
	public float m_EmitRate = 10;
	public GameObject m_Resource;
	public Vector3 m_From;
	public Vector3 m_To;

	List<ScanningCircleHandler> m_Circles;
	public float m_LifeTime = 2;

	// Use this for initialization
	void Start ()
	{
		m_Circles = new List<ScanningCircleHandler> ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		EmitLogic();
		MotionLogic();
	}

	void EmitLogic ()
	{
		m_LifeTime -= Time.deltaTime;
		if ( m_LifeTime > 0 )
		{
			float emit_cnt = m_EmitRate * Time.deltaTime;
			int emit_icnt = Mathf.FloorToInt(emit_cnt);
			for ( int i = 0; i < emit_icnt; ++i )
				Emit();
			if ( Random.value < (emit_cnt - emit_icnt) )
				Emit();
		}
		if ( m_LifeTime < -10 )
		{
			if ( m_Circles.Count > 0 )
			{
				foreach ( ScanningCircleHandler circle in m_Circles )
				{
					GameObject.Destroy(circle.gameObject);
				}
				m_Circles.Clear();
			}
		}
	}

	void MotionLogic ()
	{
		float dist = Vector3.Distance(m_From, m_To);
		foreach ( ScanningCircleHandler circle in m_Circles )
		{
			circle.transform.localPosition = Vector3.Lerp(circle.transform.localPosition, m_To, 0.02f);
			float dist_from = Vector3.Distance(circle.transform.localPosition, m_From);
			float dist_to = Vector3.Distance(circle.transform.localPosition, m_To);
			circle.m_CircleBrightness = Mathf.Pow(Mathf.Min(dist_from, dist_to) / dist * 2, 1.5f)*0.2f;
		}
	}

	void Emit ()
	{
		if ( m_Resource != null )
		{
			GameObject go = GameObject.Instantiate(m_Resource) as GameObject;
			go.transform.parent = transform;
			go.transform.localPosition = m_From;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			ScanningCircleHandler circle = go.GetComponent<ScanningCircleHandler>();
			circle.m_Brightness = Random.value * 0.8f + 0.2f;
			circle.m_CircleBrightness = 0;
			circle.TakeEffect();
			m_Circles.Add(circle);
		}
	}
}
