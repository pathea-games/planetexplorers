using UnityEngine;
using System.Collections;

public class CSUI_PopupHint : MonoBehaviour 
{
	public Vector3 m_Velocity;
	public Vector3 m_Pos;
	public Vector3 m_LocalScale;

	public float m_DuringTime = 5;
	public float m_MaxScale = 1.5f;
	public float m_ScaleRate = 0.1f;

	private float m_StartTime = 0;

	private float m_Scale = 1;
	

	bool m_ScaleDir = true;
	float m_ScaleStayDuring = 1f;
	float m_CurStarTime = 0;
	
	[SerializeField]
	private UILabel		m_TextLabGreen;
	[SerializeField]
	private UILabel		m_TextLabRed;

	public string Text 
	{
		get { return m_TextLabGreen.text; }
		set { m_TextLabGreen.text = value; m_TextLabRed.text = value; }
	}

	public bool bGreen;

	public void Tween()
	{
		transform.position = m_Pos;
		transform.localScale = m_LocalScale;

		m_Scale = 1;
		m_StartTime = 0;
		m_ScaleDir = true;
		m_CurStarTime = 0;

		gameObject.SetActive(true);
	}

	void Awake ()
	{
		gameObject.SetActive(false);
	}

	// Use this for initialization
	void Start () 
	{

	}

	// Update is called once per frame
	void Update () 
	{
		if (bGreen)
		{
			m_TextLabGreen.enabled = true;
			m_TextLabRed.enabled = false;
		}
		else
		{
			m_TextLabGreen.enabled = false;
			m_TextLabRed.enabled = true;
		}

		m_StartTime += Time.deltaTime;
		if (m_StartTime < m_DuringTime)
		{
			float factor = Mathf.Pow( 1 - m_StartTime / m_DuringTime, 8f);
			float val =  2.5f * factor;
			m_Velocity = new Vector3(0, val, 0);
			transform.localPosition += m_Velocity;
		}


		// Scale
		if (m_ScaleDir)
		{
			m_Scale = Mathf.Lerp(m_Scale, m_MaxScale, m_ScaleRate);
			Vector3 scale = m_LocalScale * m_Scale;
			transform.localScale = new Vector3(scale.x, scale.y, 1);

			if (Mathf.Abs(m_MaxScale - m_Scale) < 0.01f)
				m_ScaleDir = false;
		}
		else
		{
			m_CurStarTime += Time.deltaTime;
			if (m_CurStarTime > m_ScaleStayDuring)
			{
				m_Scale = m_Scale - (m_Scale - 0.1f) * (m_ScaleRate);
				Vector3 scale = m_LocalScale * m_Scale;
				transform.localScale = new Vector3(scale.x, scale.y, 1);

				if (scale.x <= 0.1f)
				{
					Destroy(gameObject);
					//gameObject.SetActive(false);
				}
			}
		}

	}
}
