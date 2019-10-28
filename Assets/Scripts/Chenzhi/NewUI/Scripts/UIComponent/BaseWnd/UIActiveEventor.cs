using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class UIActiveEventor : MonoBehaviour 
{
	void OnEnable()
	{
		OnActive.Invoke();
	}

	[SerializeField] UnityEvent OnActive;
}
