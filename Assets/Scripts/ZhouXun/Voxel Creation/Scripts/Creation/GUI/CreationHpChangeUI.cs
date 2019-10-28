using UnityEngine;
using System.Collections;

public class CreationHpChangeUI : MonoBehaviour
{
	private static CreationHpChangeUI s_Instance;
	public static CreationHpChangeUI Instance { get { return s_Instance; } }
	public GameObject m_Res;
	public int m_Queue = 0;
	public int m_DebugValue = 0;
	public bool m_DebugTest = false;

	// Use this for initialization
	void Awake ()
	{
		s_Instance = this;
	}

	void OnDestroy ()
	{
		s_Instance = null;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if ( Time.frameCount % 10 == 0 )
		{
			if ( m_Queue > 0 )
				m_Queue--;
		}
		if ( m_DebugTest )
		{
			m_DebugTest = false;
			Popup(m_DebugValue);
		}
	}

	public void Popup (int val)
	{
		if ( this.gameObject.activeInHierarchy && this.enabled )
		{
			GameObject ui = GameObject.Instantiate(m_Res) as GameObject;
			ui.transform.parent = transform;
			ui.layer = this.gameObject.layer;
			Vector3 from = ui.GetComponent<TweenPosition>().from;
			from.y += (80 * m_Queue);
			ui.GetComponent<TweenPosition>().from = from;
			ui.transform.localPosition = from;
			ui.GetComponent<UILabel>().text = val.ToString();
			ui.GetComponent<UILabel>().depth = 100;
			Vector3 scale = Mathf.Log10(Mathf.Abs(val)+500) * 60 * Vector3.one;
			ui.GetComponent<TweenScale>().from = scale;
			m_Queue++;
			if ( m_Queue == 3 )
				m_Queue = 0;
		}
	}
}
