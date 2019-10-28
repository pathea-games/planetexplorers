using UnityEngine;
using System.Collections;
using System.IO;

public class VCEUITips : MonoBehaviour
{
	public float showTime = 10;
	public float lifeTime = 0;

	public void Show ()
	{
		lifeTime = 0;
		gameObject.SetActive(true);
	}

	public void Show (float time)
	{
		showTime = time;
		lifeTime = 0;
		gameObject.SetActive(true);
	}

	public void Hide ()
	{
		lifeTime = 0;
		gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update ()
	{
		lifeTime += Time.deltaTime;
		if (lifeTime > showTime)
			gameObject.SetActive(false);
	}
}
