using UnityEngine;
using System.Collections;

public class UIActiveChange : MonoBehaviour 
{
	[SerializeField] GameObject mTarget = null;

	void OnEnable()
	{
		if (mTarget != null)
			mTarget.SetActive(true);
	}

	void OnDisable()
	{
		if (mTarget != null)
			mTarget.SetActive(false);
	}
}
