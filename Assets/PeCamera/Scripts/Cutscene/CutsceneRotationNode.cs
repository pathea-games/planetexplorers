using UnityEngine;
using System.Collections;

[System.Serializable]
public class CutsceneRotationNode
{
	public float time;
	public Transform rotation;

	public static int Compare (CutsceneRotationNode lhs, CutsceneRotationNode rhs)
	{
		return Mathf.RoundToInt((lhs.time - rhs.time) * 10000f);
	}
}
