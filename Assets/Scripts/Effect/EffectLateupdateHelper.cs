using UnityEngine;
using System.Collections;

public class EffectLateupdateHelper : MonoBehaviour 
{
    public Vector3 local;

	protected Transform m_ParentTrans;
	protected Vector3 m_LateUpdatePos;
	protected Quaternion m_LateUpdateRot;

	Vector3 m_UpdatePos;

	Pathea.PeEntity m_Entity;

	public virtual void Init(Transform parentTrans)
	{
		m_ParentTrans = parentTrans;
		m_LateUpdatePos = m_ParentTrans.position;
		m_LateUpdateRot = m_ParentTrans.rotation;
		m_UpdatePos = Vector3.zero;
		m_Entity = parentTrans.GetComponentInParent<Pathea.PeEntity>();
	}

	protected virtual void Update()
	{
		if(null == m_ParentTrans)
		{
			GameObject.Destroy(gameObject);
			return;
		}

		bool activeState = m_ParentTrans.gameObject.activeInHierarchy;

		if (null != m_Entity)
			activeState = activeState && !m_Entity.IsDeath ();
		if(activeState != gameObject.activeSelf)
			gameObject.SetActive(activeState);

		Vector3 updateDir = m_ParentTrans.position - ((m_UpdatePos != Vector3.zero)?m_UpdatePos:m_ParentTrans.position);
		m_UpdatePos = m_ParentTrans.position;
		transform.position = m_LateUpdatePos + updateDir;
		transform.rotation = m_LateUpdateRot;
	}
	
	// Update is called once per frame
	protected virtual void LateUpdate ()
	{
		if(null == m_ParentTrans)
		{
			GameObject.Destroy(gameObject);
			return;
		}
		m_LateUpdatePos = m_ParentTrans.position + local;
		m_LateUpdateRot = m_ParentTrans.rotation;
	}
}
