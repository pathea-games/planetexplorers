using UnityEngine;
using System.Collections;

public class VCEUIColorPick : MonoBehaviour
{
	private float m_Hue = 0;
	private float m_Sat = 0;
	private float m_Brt = 0;
	
	private Color m_FinalColor = new Color (0,0,0,1);
	private bool m_InitialLock = true;
	
	public Color FinalColor
	{
		get { return m_FinalColor; }
		set
		{
			m_FinalColor = value;
			m_FinalColor.a = 1;
			Vector3 hsb = VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
			m_Hue = hsb.x;
			m_Sat = hsb.y;
			m_Brt = hsb.z;
		}
	}
	public float R
	{
		get { return m_FinalColor.r; }
		set
		{
			m_FinalColor.r = value;
			Vector3 hsb = VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
			m_Hue = hsb.x;
			m_Sat = hsb.y;
			m_Brt = hsb.z;
		}
	}
	public float G
	{
		get { return m_FinalColor.g; }
		set
		{
			m_FinalColor.g = value;
			Vector3 hsb = VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
			m_Hue = hsb.x;
			m_Sat = hsb.y;
			m_Brt = hsb.z;
		}
	}
	public float B
	{
		get { return m_FinalColor.b; }
		set
		{
			m_FinalColor.b = value;
			Vector3 hsb = VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat);
			m_Hue = hsb.x;
			m_Sat = hsb.y;
			m_Brt = hsb.z;
		}
	}
	public Vector3 HSB
	{
		get { return VCUtils.RGB2HSB(m_FinalColor, m_Hue, m_Sat); }
		set
		{
			m_Hue = value.x;
			m_Sat = value.y;
			m_Brt = value.z;
			
			m_FinalColor = VCUtils.HSB2RGB(m_Hue, m_Sat, m_Brt);
		}
	}
	
	public Transform m_HSCircle;
	public UISprite m_HSPad;
	public UISlider m_BrtSlider;
	public UISprite m_BrtSliderColor;
	public UISlider m_RSlider;
	public UISlider m_GSlider;
	public UISlider m_BSlider;
	public UILabel m_RValLabel;
	public UILabel m_GValLabel;
	public UILabel m_BValLabel;
	public UISprite m_ColorRect;
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		m_InitialLock = false;
		UpdateHSPadLogic();
		UpdateUI();
	}
	
	public void UpdateUI ()
	{
		m_ColorRect.color = m_FinalColor;
		m_HSCircle.localPosition = new Vector3 (m_Hue/360, m_Sat, 0);
		m_BrtSlider.sliderValue = m_Brt;
		m_BrtSliderColor.color = VCUtils.HSB2RGB(m_Hue, m_Sat, 1);
		m_RSlider.sliderValue = m_FinalColor.r;
		m_GSlider.sliderValue = m_FinalColor.g;
		m_BSlider.sliderValue = m_FinalColor.b;
		m_RValLabel.text = (m_FinalColor.r*100).ToString("0") + "%";
		m_GValLabel.text = (m_FinalColor.g*100).ToString("0") + "%";
		m_BValLabel.text = (m_FinalColor.b*100).ToString("0") + "%";
	}
	
	void OnRChange (float r)
	{
		if ( m_InitialLock ) return;
		R = r;
	}
	void OnGChange (float g)
	{
		if ( m_InitialLock ) return;
		G = g;
	}
	void OnBChange (float b)
	{
		if ( m_InitialLock ) return;
		B = b;
	}
	bool bFocusedHS = false;
	void UpdateHSPadLogic()
	{
		if ( m_InitialLock ) return;
		if ( Input.GetMouseButtonDown(0) )
		{
			RaycastHit rch;
			if ( Physics.Raycast(VCEInput.s_UIRay, out rch, 100, VCConfig.s_UILayerMask) )
			{
				if ( rch.collider.gameObject == m_HSPad.gameObject )
				{
					bFocusedHS = true;
				}
				else
				{
					bFocusedHS = false;
				}
			}
			else
			{
				bFocusedHS = false;
			}
		}
		if ( Input.GetMouseButtonUp(0) )
		{
			bFocusedHS = false;
		}
		
		if ( Input.GetMouseButton(0) && bFocusedHS )
		{
			RaycastHit rch;
			if ( Physics.Raycast(VCEInput.s_UIRay, out rch, 100, VCConfig.s_UILayerMask) )
			{
				if ( rch.collider.gameObject == m_HSPad.gameObject )
				{
					Vector3 point = m_HSPad.transform.InverseTransformPoint(rch.point);
					point.x = Mathf.Clamp(point.x, 0, 0.9995f);
					point.y = Mathf.Clamp01(point.y);
					HSB = new Vector3 (point.x*360, point.y, m_Brt);
				}
			}
		}
	}
	void OnBrtChange (float brt)
	{
		if ( m_InitialLock ) return;
		HSB = new Vector3 (m_Hue, m_Sat, brt);
	}
}
