using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSCommonObject : CSEntityObject 
{ 

	public LocateCubeEffectHanlder m_LocEffectPrefab;

	private Dictionary<int, LocateCubeEffectHanlder> m_LocEffects = new Dictionary<int, LocateCubeEffectHanlder>();

	public void ShowWorkSpaceEffect()
	{
		if (m_LocEffectPrefab == null)
			return;

		if (m_Entity != null)
		{
			CSCommon csc = m_Entity as CSCommon;
			if (csc.WorkPoints == null)
				return;

            for (int i = 0; i < csc.WorkPoints.works.Length; i++)
			{
				if (csc.WorkPoints.works[i] != null
				    && !m_LocEffects.ContainsKey(i))
				{
					LocateCubeEffectHanlder lce = Instantiate(m_LocEffectPrefab) as LocateCubeEffectHanlder;
					lce.transform.parent = this.transform;
                    Vector3 pos = csc.WorkPoints.works[i].transform.position;
					pos.y += (lce.m_CubeLen * 0.5f * lce.m_MaxHeightScale) + 0.05f;
					lce.transform.position = pos;
					lce.transform.localRotation = Quaternion.identity;
					m_LocEffects.Add(i, lce);
				}
			}
		}
		else
		{
			for (int i = 0; i < m_WorkTrans.Length; i++)
			{
				if (!m_LocEffects.ContainsKey(i))
				{
					LocateCubeEffectHanlder lce = Instantiate(m_LocEffectPrefab) as LocateCubeEffectHanlder;
					lce.transform.parent = this.transform;
					Vector3 pos = m_WorkTrans[i].position;
					pos.y += (lce.m_CubeLen * 0.5f * lce.m_MaxHeightScale) + 0.05f;
					lce.transform.position = pos;
					lce.transform.localRotation = Quaternion.identity;
					m_LocEffects.Add(i, lce);
				}
			}
		}

	}

	public void HideWorkSpaceEffect()
	{
		if (m_LocEffectPrefab == null)
			return;
		foreach (LocateCubeEffectHanlder lce in m_LocEffects.Values)
		{
			if (lce != null)
				Destroy(lce.gameObject);
		}

		m_LocEffects.Clear();
	}

	// Use this for initialization
	protected new void Start () 
	{
		base.Start();

	}
	
	// Update is called once per frame
	protected new void Update ()
	{
		base.Update();
	}
}
