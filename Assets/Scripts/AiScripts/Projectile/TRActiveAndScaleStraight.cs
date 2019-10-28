using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ActiveAndScale
{
	[System.Serializable]
	public class Action
	{
		public float startTime;
		public float endTime;
		public float colScaleStart;
		public float colScaleEnd;
	}
	[SerializeField] List<Action> m_Actions;
	Collider[] m_Cols;
	Vector3[] m_DefaultScale;
	Action m_CurrentAttack;
	float m_Time;

	public void Init(MonoBehaviour mono)
	{
		m_Cols = mono.GetComponentsInChildren<Collider>(true);
		m_DefaultScale = new Vector3[m_Cols.Length];
		for(int i = 0; i < m_Cols.Length; i++)
			m_DefaultScale[i] = m_Cols[i].transform.localScale;
		ActiveCols(false);
	}

	public void UpdateAttackState(float deltaTime)
	{
		m_Time += deltaTime;
		if(m_Actions.Count > 0)
		{
			if(m_Time > m_Actions[0].startTime)
			{
				if(null == m_CurrentAttack)
				{
					m_CurrentAttack = m_Actions[0];
					ActiveCols(true);
				}
				
				if(m_Time > m_Actions[0].endTime)
				{
					m_CurrentAttack = null;
					if(m_Actions.Count <= 1 || m_Actions[1].startTime > m_Actions[0].endTime)
						ActiveCols(false);
					m_Actions.RemoveAt(0);
				}
				
				if(null != m_CurrentAttack)
					ResetColScale(Mathf.Lerp(m_CurrentAttack.colScaleStart, m_CurrentAttack.colScaleEnd,
					                         (m_Time - m_CurrentAttack.startTime)/(m_CurrentAttack.endTime - m_CurrentAttack.startTime)));
			}
		}
	}

	void ActiveCols(bool active)
	{
		for(int i = 0; i < m_Cols.Length; i++)
			m_Cols[i].gameObject.SetActive(active);
	}
	
	void ResetColScale(float scale)
	{
		for(int i = 0; i < m_Cols.Length; i++)
			m_Cols[i].transform.localScale = scale * m_DefaultScale[i];
	}
}

public class TRActiveAndScaleStraight : TRStraight
{
	[SerializeField] ActiveAndScale m_ActiveAndScale;

	public override void SetData (Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData (caster, emitter, target, targetPosition, index);
		m_ActiveAndScale.Init(this);
	}

	public override Vector3 Track (float deltaTime)
	{
		m_ActiveAndScale.UpdateAttackState(deltaTime);
		return base.Track(deltaTime);
	}
}
