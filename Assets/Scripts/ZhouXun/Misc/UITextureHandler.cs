using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class UITextureHandler : MonoBehaviour
{
	public Material m_Material;
	private Material m_MatInst;

	// Use this for initialization
	void OnEnable ()
	{
		if (m_Material != null)
		{
			if (m_MatInst != null)
				Material.DestroyImmediate(m_MatInst);
			m_MatInst = Material.Instantiate(m_Material) as Material;
			UITexture uit = GetComponent<UITexture>();
			if (uit != null)
			{
				uit.material = m_MatInst;
			}
		}
	}

	void OnDestroy ()
	{
		if (m_MatInst != null)
		{
			Material.DestroyImmediate(m_MatInst);
			m_MatInst = null;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
