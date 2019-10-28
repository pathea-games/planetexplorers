using System;
using UnityEngine;
using WhiteCat;

public class SelectCar : SingletonBehaviour<SelectCar>
{
	Transform[] trans;
	PathDriver[] drivers;
	TweenMaterialColor[] tweens;
	int index = 1;

	TweenInterpolator interpolator;
	RaycastHit hitInfo;


	public static Transform carTransform { get { return instance.trans[instance.index]; } }
	public static PathDriver carDriver { get { return instance.drivers[instance.index]; } }


	public static void HideUnselected()
	{
		for(int i=0; i<3; i++)
		{
			if (instance.index != i) instance.drivers[i].gameObject.SetActive(false);
		}
	}


	public void ShowAll()
	{
		for (int i = 0; i < 3; i++)
		{
			drivers[i].gameObject.SetActive(true);
		}
	}


	protected override void Awake()
	{
		base.Awake();

		trans = new Transform[3];
		drivers = new PathDriver[3];
		tweens = new TweenMaterialColor[3];

		for(int i = 0; i < 3; i++)
		{
			trans[i] = transform.GetChild(i).GetChild(0);
			drivers[i] = trans[i].parent.GetComponent<PathDriver>();
			tweens[i] = trans[i].GetComponentInChildren<TweenMaterialColor>();
		}

		interpolator = GetComponent<TweenInterpolator>();
	}


	void OnEnable()
	{
		interpolator.normalizedTime = 0;
		interpolator.isPlaying = true;

		tweens[index].OnRecord();
		tweens[index].enabled = true;
	}


	void OnDisable()
	{
		tweens[index].enabled = false;
		tweens[index].OnRestore();

		interpolator.isPlaying = false;
	}


	void Update()
	{
		if(Input.GetMouseButtonDown(0) && Physics.Raycast(
			Camera.main.ScreenPointToRay(Input.mousePosition),
			out hitInfo))
		{
			tweens[index].enabled = false;
			tweens[index].OnRestore();

			index = Array.IndexOf(trans, hitInfo.transform);
			interpolator.normalizedTime = 0;

			tweens[index].OnRecord();
			tweens[index].enabled = true;
		}
	}
}
