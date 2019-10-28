using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class DirectionHandler : MonoBehaviour
{
	public bool RuntimeOnly = true;
	public Transform DirTrans;
	public string PropName;
	public bool FilpDir = false;
	
	void Update ()
	{
		if (RuntimeOnly && !Application.isPlaying)
			return;

		if (Application.isPlaying)
		{
			if (GetComponent<Renderer>() != null && GetComponent<Renderer>().material != null && DirTrans != null && !string.IsNullOrEmpty(PropName))
			{
				if (GetComponent<Renderer>().material.HasProperty(PropName))
				{
					Vector3 forward = FilpDir ? -DirTrans.forward : DirTrans.forward;
					Vector4 oldvec4 = GetComponent<Renderer>().material.GetVector(PropName);
					GetComponent<Renderer>().material.SetVector(PropName, new Vector4 (forward.x, forward.y, forward.z, oldvec4.w));
				}
			}
		}
		else
		{
			if (GetComponent<Renderer>() != null && GetComponent<Renderer>().sharedMaterial != null && DirTrans != null && !string.IsNullOrEmpty(PropName))
			{
				if (GetComponent<Renderer>().sharedMaterial.HasProperty(PropName))
				{
					Vector3 forward = FilpDir ? -DirTrans.forward : DirTrans.forward;
					Vector4 oldvec4 = GetComponent<Renderer>().sharedMaterial.GetVector(PropName);
					GetComponent<Renderer>().sharedMaterial.SetVector(PropName, new Vector4 (forward.x, forward.y, forward.z, oldvec4.w));
				}
			}
		}
	}
}
