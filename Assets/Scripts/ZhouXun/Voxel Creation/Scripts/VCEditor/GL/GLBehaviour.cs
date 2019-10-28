using UnityEngine;
using System.Collections;

public abstract class GLBehaviour : MonoBehaviour
{
	public int m_RenderOrder = 0;
	public Material m_Material = null;
	public abstract void OnGL();
}
