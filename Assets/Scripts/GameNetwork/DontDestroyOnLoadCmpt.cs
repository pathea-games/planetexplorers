using UnityEngine;
using System.Collections;

public class DontDestroyOnLoadCmpt : MonoBehaviour
{
	void Awake()
	{
		DontDestroyOnLoad(this);
	}
}
