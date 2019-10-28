using UnityEngine;
using System.Collections;

public class CSUI_AlphaTween : MonoBehaviour 
{
	public static CSUI_AlphaTween Self { get { return m_Self;} }
	private static CSUI_AlphaTween m_Self = null;
	#region UI_WIDGET
	[SerializeField] UILabel	m_TipLbUI;
	#endregion

//	public float From;
//
//	public float To;

	public bool Reverse;

	public float FadeSpeed = 0.5f;

	public string Text  { get { return m_TipLbUI.text; } set { m_TipLbUI.text = value; } }

	private UIWidget[] m_Wighets;
	private bool m_Play;
	private bool m_Forwad;

	public void Play (bool play)
	{
		if (play)
		{

//			for (int i = 0; i < m_Wighets.Length; i++)
//			{
//				m_Wighets[i].alpha = From;
//			}

			m_Forwad = true;

			m_Play = true;
		}
		else
		{
			for (int i = 0; i < m_Wighets.Length; i++)
			{
				m_Wighets[i].alpha = 0;
			}
			m_Play = false;
		}

	}

	public void Play (float duration)
	{
		StopAllCoroutines();
		StartCoroutine(_play(duration));
	}

	IEnumerator _play (float duration)
	{
		Play (true);
		yield return new WaitForSeconds (duration);
		Play(false);
	}

	void Awake ()
	{
		m_Wighets = gameObject.GetComponentsInChildren<UIWidget>();

		m_Self = this;
	}

	// Use this for initialization
	void Start () 
	{
		for (int i = 0; i < m_Wighets.Length; i++)
		{
			m_Wighets[i].alpha = 0;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (!m_Play)
			return;

		if (m_Forwad)
		{
			for (int i = 0; i < m_Wighets.Length; i++)
			{
				m_Wighets[i].alpha += FadeSpeed * Time.deltaTime;

				if (m_Wighets[i].alpha >= 1)
					m_Forwad = false;
			}
		}
		else if (Reverse)
		{
			for (int i = 0; i < m_Wighets.Length; i++)
			{
				m_Wighets[i].alpha -= FadeSpeed * Time.deltaTime;

				if (m_Wighets[i].alpha <= 0)
					m_Forwad = true;

			}
		}

//		if (m_Forwad)
//		{
//			for (int i = 0; i < m_Wighets.Length; i++)
//			{
////				m_Wighets[i].alpha += FadeSpeed * Time.deltaTime;
//				m_Wighets[i].alpha = Mathf.Lerp(m_Wighets[i].alpha, To, FadeSpeed);
//				
//				if ( Mathf.Abs(m_Wighets[i].alpha - To) < 0.01f)
//					m_Forwad = false;
//			}
//		}
//		else if (Reverse)
//		{
//			for (int i = 0; i < m_Wighets.Length; i++)
//			{
//				m_Wighets[i].alpha = Mathf.Lerp(m_Wighets[i].alpha, From, 1 - FadeSpeed);
//				
//				if (Mathf.Abs(m_Wighets[i].alpha - From) < 0.01F)
//					m_Forwad = true;
//				
//			}
//		}
	}
}
