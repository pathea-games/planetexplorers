using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DestroyTimer))]
public class EffectGroup : MonoBehaviour
{
	public List<EffectDesc> Effect = new List<EffectDesc>();

	[HideInInspector] public Transform m_root = null;

//	protected virtual void DestroyEffectDesc( EffectDesc effect )
//	{
//		Destroy(effect.EffectObject,effect.DestroyTime);
//		Effect.Remove(effect);
//	}

	float CurrentTime = 0;

	void OnEnable ()
	{
		CurrentTime = 0;
	}

	void Update()
	{
		CurrentTime += Time.deltaTime;
		if( m_root != null )
		{
			foreach ( EffectDesc eff in Effect )
			{
				if ( CurrentTime > eff.DelayTime )
				{
					eff.m_effroot = AiUtil.GetChild( m_root, eff.Parent);
					eff.Play();
				}
			}
		}
		else
		{
			Debug.LogError("Can't find the total parent or the model");
		}
	}
}

[Serializable]
public  class EffectDesc
{
	private bool IsPlayed = false;
	[HideInInspector] public Transform m_effroot = null;
	public GameObject EffectObject;
	public float DelayTime;
	public float DestroyTime;
	public string Parent;
	public Vector3 Position;
	public Vector3 Rotation;

	public void Play()
	{
		if ( !IsPlayed )
		{
			IsPlayed = true;
			GameObject eff = GameObject.Instantiate(EffectObject) as GameObject;
			if( m_effroot == null )
			{	
				Debug.LogError("Can't find the parent to create effect");
			}
			else
			{
				eff.transform.parent = m_effroot;
			}
			eff.transform.localPosition = Position;
			eff.transform.localRotation = Quaternion.Euler(Rotation);
			eff.AddComponent<DestroyTimer>().m_LifeTime = DestroyTime;
		}
	}
}
