using UnityEngine;
using System.Collections;

public class VCEHoloBoard : GLBehaviour
{
	public Vector3 m_Position;
	private float m_FadeFactor = 0;
	private float m_FadeFactorWanted = 0;
	private bool m_MaterialReplaced = false;

	public void ReplaceMat ()
	{
		if ( !m_MaterialReplaced )
		{
			m_Material = Material.Instantiate(m_Material) as Material;
			m_MaterialReplaced = true;
		}
	}

	public void FadeIn ()
	{
		this.gameObject.SetActive(true);
		m_FadeFactorWanted = 1;
		if ( m_FadeFactor < 0.001f )
			m_FadeFactor = 0.0011f;
	}
	public void FadeOut ()
	{
		m_FadeFactorWanted = 0;
	}

	// Use this for initialization
	void Start ()
	{

	}

	void OnEnable()
	{
		Update();
	}

	// Update is called once per frame
	void Update ()
	{
		ReplaceMat();
		if ( VCEditor.Instance != null )
		{
			Vector3 dir = (m_Position - VCEditor.Instance.m_MainCamera.transform.position).normalized;
			transform.position = VCEditor.Instance.m_MainCamera.transform.position + dir*2;
			transform.eulerAngles = VCEditor.Instance.m_MainCamera.transform.eulerAngles;
			transform.localScale = new Vector3(1,2,1);
		}
		if ( m_FadeFactor > m_FadeFactorWanted )
			m_FadeFactor = Mathf.Lerp(m_FadeFactor, m_FadeFactorWanted, 0.3f);
		else
			m_FadeFactor = Mathf.Lerp(m_FadeFactor, m_FadeFactorWanted, Time.deltaTime * 3f);
		m_Material.SetFloat("_Fade", m_FadeFactor);
		if ( m_FadeFactor < 0.001f )
		{
			this.gameObject.SetActive(false);
		}
	}

	public override void OnGL ()
	{
		GL.Begin(GL.QUADS);
		GL.Color(Color.white);
		GL.TexCoord2(0,1);
		GL.Vertex(transform.position - transform.right * 0.55f);
		GL.TexCoord2(1,1);
		GL.Vertex(transform.position + transform.right * 1.45f);
		GL.TexCoord2(1,0);
		GL.Vertex(transform.position + transform.right * 1.45f - transform.up * 2);
		GL.TexCoord2(0,0);
		GL.Vertex(transform.position - transform.right * 0.55f - transform.up * 2);
		GL.End();
	}
}
