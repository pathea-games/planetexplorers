using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (BoxCollider))]
public class BSGizmoTriggerEvent : MonoBehaviour
{
	Dictionary<int, Rigidbody> m_Rigidbodys = new Dictionary<int, Rigidbody>();

	public BoxCollider boxCollider { get { return m_Collider;} }
	private BoxCollider m_Collider;

	public bool RayCast { get { return m_Rigidbodys.Count != 0; }}

	void Awake ()
	{
		m_Collider = gameObject.GetComponent<BoxCollider>();
	}

	void OnDisable ()
	{
		m_Rigidbodys.Clear();
	}

	void OnTriggerEnter(Collider other)
	{


	}

	void OnTriggerStay (Collider other)
	{
		Rigidbody body = other.gameObject.GetComponent<Rigidbody>();
		
		if (body != null)
		{
			int id = other.gameObject.GetInstanceID();

			if (!m_Rigidbodys.ContainsKey(id))
			{
#if UNITY_EDITOR
				Debug.LogWarning( " Add Rigid body " + other.gameObject.name);
#endif
			}

			m_Rigidbodys[id] = body;

		}
		else
		{
			body = other.gameObject.GetComponentInParent<Rigidbody>();
			if (body != null)
			{
				int id = other.gameObject.GetInstanceID();
				
				if (!m_Rigidbodys.ContainsKey(id))
				{
#if UNITY_EDITOR
					Debug.LogWarning( " Add Rigid body " + other.gameObject.name);
#endif
				}
				
				m_Rigidbodys[id] = body;
			}
		}
	}

	void OnTriggerExit (Collider other)
	{
		if ( m_Rigidbodys.Remove (other.gameObject.GetInstanceID()) )
		{
#if UNITY_EDITOR
			Debug.LogWarning(" Remove Rigid body " +  other.gameObject.name);
#endif
		}
	}

}
