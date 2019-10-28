using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class AnimationEffect
{
	[HideInInspector] public bool LastIsPlaying = false;
	public string AnimationName;
	public GameObject EffectGroupRes;
}

public class AnimationEffectPlayer : MonoBehaviour
{
	public GameObject model;
	
	public List<AnimationEffect> AnimationEffects = new List<AnimationEffect>();

	bool islog = false;

	void OnEnable()
	{
		islog = false;
	}

	void Update()
	{
		if( model != null && model.GetComponent<Animation>() != null)
		{
			foreach ( AnimationEffect animeffcs in AnimationEffects )
			{
				if(animeffcs == null)
					continue;

				bool isplaying = model.GetComponent<Animation>().IsPlaying(animeffcs.AnimationName);
				if ( isplaying && !animeffcs.LastIsPlaying )
				    PlayEffectGroup(animeffcs.EffectGroupRes);
				animeffcs.LastIsPlaying = model.GetComponent<Animation>().IsPlaying(animeffcs.AnimationName);
			}
		}
		else
		{
			if ( model == null && !islog )
			{
				Debug.LogError("The model is null!!!");
				islog = true;
				return;

//				if ( model.GetComponent<Animation>() == null && !islog )
//				{
//					Debug.LogError("The model.animation is null!!!");
//					islog = true;
//					return;
//				}
			}
		}
	}

	void PlayEffectGroup (GameObject effgroupres)
	{
		if(effgroupres == null)
			return;

		GameObject effgroup = GameObject.Instantiate(effgroupres) as GameObject;
		effgroup.transform.parent = transform;
		if ( effgroup.GetComponent<EffectGroup>() == null )
		{
			Debug.LogError("There is no EffectGroup to get Root");
		}
		else
		{
			if(model != null)
			{
				effgroup.GetComponent<EffectGroup>().m_root = model.transform;
			}
		}
		effgroup.transform.localPosition = Vector3.zero;
		effgroup.transform.localRotation = Quaternion.identity;
		effgroup.transform.localScale = Vector3.one;
	}
}
