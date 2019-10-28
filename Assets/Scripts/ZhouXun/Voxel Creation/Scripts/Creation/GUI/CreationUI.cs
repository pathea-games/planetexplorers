using UnityEngine;
using System.Collections;

public class CreationUI : MonoBehaviour
{
	public static CreationUI Instance { get { return s_Instance; } }
	private static CreationUI s_Instance = null;

	public Transform m_MissileLockerGroup;
	public MissileLockerUI m_MissileLockerPrefab;

	// Use this for initialization
	void Awake ()
	{
		s_Instance = this;
	}

	void OnDestroy ()
	{
		s_Instance = null;
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	public MissileLockerUI MissileLockObject (Transform target)
	{
		MissileLockerUI ml = MissileLockerUI.Instantiate(m_MissileLockerPrefab) as MissileLockerUI;
		ml.transform.parent = m_MissileLockerGroup;
		ml.transform.localScale = Vector3.one;
		ml.UpdatePos();
		ml.m_TargetObject = target;
		ml.FadeIn();
		return ml;
	}
}
