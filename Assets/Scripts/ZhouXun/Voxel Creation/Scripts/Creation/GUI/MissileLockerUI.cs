using UnityEngine;
using System.Collections;

public class MissileLockerUI : MonoBehaviour
{
	public Transform m_TargetObject;

	public Transform m_TAGroup;
	public UISprite m_TA0;
	public UISprite m_TA90;
	public UISprite m_TA180;
	public UISprite m_TA270;
	public UISprite m_Deco0;
	public UISprite m_Deco1;
	public UISprite m_ProgressFSpriteBG;
	public UIFilledSprite m_ProgressFSprite;
	public UILabel m_LockProgressText;
	public AudioClip m_LockingSound0;
	public AudioClip m_LockingSound1;
	public AudioClip m_LockedSound;

	public float m_TASpan = 20;
	public float m_TAWidth = 16;
	public float m_TAHeight = 16;
	public float m_TARotSpeed = 90;
	public float m_Deco0RotSpeed = 59;
	public float m_Deco1RotSpeed = -83;
	public float m_Alpha = 0.6f;
	public Color m_UnlockColor;
	public Color m_LockedColor;

	private float m_LockProgress = 0;
	private float m_LastProgress;
	public float LockProgress
	{
		get { return m_LockProgress; }
		set
		{
			m_LockProgress = value;
			m_ProgressFSprite.fillAmount = value;
		}
	}
	private float m_LockCompleteEffectTime = 0;
	private bool m_LockComplete = false;
	private void LockComplete ()
	{
		m_LockComplete = true;
		m_LockCompleteEffectTime = 0;
		NGUITools.PlaySound(m_LockedSound, 0.5f, 1.4f);
	}

	private float m_Fade = 0;
	private float m_FadeDir = 1;
	public void FadeIn ()
	{
		m_FadeDir = 1;
	}
	public void FadeOut ()
	{
		m_FadeDir = -1;
	}
	public bool Alive { get { return m_FadeDir >= 0; } }

	// Use this for initialization
	void Start ()
	{
		FadeIn();
	}

	float ta_rot;
	// Update is called once per frame
	void Update ()
	{
		UpdatePos();

		if ( m_ProgressFSprite.fillAmount < 0.999f && m_ProgressFSprite.fillAmount > 0.001f )
			ta_rot += Time.deltaTime * m_TARotSpeed * (3f + m_ProgressFSprite.fillAmount*8.0f);
		else
			ta_rot += Time.deltaTime * m_TARotSpeed;
		m_TAGroup.localEulerAngles = new Vector3 (0,0,ta_rot);
		m_Deco0.transform.localEulerAngles = new Vector3 (0,0,Time.time*m_Deco0RotSpeed);
		m_Deco1.transform.localEulerAngles = new Vector3 (0,0,Time.time*m_Deco1RotSpeed);
		m_TA0.transform.localPosition = Vector3.up * m_TASpan;
		m_TA90.transform.localPosition = Vector3.left * m_TASpan;
		m_TA180.transform.localPosition = Vector3.down * m_TASpan;
		m_TA270.transform.localPosition = Vector3.right * m_TASpan;
		m_TA0.transform.localScale = 
		m_TA90.transform.localScale = 
		m_TA180.transform.localScale =
		m_TA270.transform.localScale = new Vector3 (m_TAWidth, m_TAHeight, 1);

		if ( m_LockComplete )
		{
			m_LockCompleteEffectTime += Time.deltaTime;
		}

		m_Fade += (m_FadeDir * Time.deltaTime * 6);
		m_Fade = Mathf.Clamp01(m_Fade);
		if ( m_Fade == 0 && m_FadeDir < 0 )
			GameObject.Destroy(this.gameObject);

		if ( m_LastProgress <= 0.999f && m_ProgressFSprite.fillAmount > 0.999f )
			LockComplete();

		for ( float p = 0; p < 0.75f; p += 0.125f )
		{
			if ( m_LastProgress <= p && m_ProgressFSprite.fillAmount > p )
				NGUITools.PlaySound(m_LockingSound0, p*0.2f + 0.1f, 1 + p*0.15f);
		}
		float step = 0.15f;
		for ( float p = 0; p < 0.98f; p += step )
		{
			if ( m_LastProgress <= p && m_ProgressFSprite.fillAmount > p )
				NGUITools.PlaySound(m_LockingSound1, 0.5f, 1 + p*0.15f);
			step *= 0.87f;
			if ( step < 0.03f )
				step = 0.03f;
		}

		m_LastProgress = m_ProgressFSprite.fillAmount;

		float size = Mathf.Lerp(20,1,Mathf.Pow(m_Fade, 0.4f));
		float alpha = m_Alpha;
		if ( m_Fade < 1 )
			alpha = Mathf.Lerp(0,1,Mathf.Pow(m_Fade, 3)) * m_Alpha * 0.5f;
		transform.localScale = new Vector3 (size,size,1);

		Color color;
		if ( m_ProgressFSprite.fillAmount < 0.999f )
		{
			color = m_UnlockColor;
		}
		else
		{
			float effect_time = 0.75f;
			if ( m_LockCompleteEffectTime < effect_time )
			{
				float effect_pi = effect_time / 5;
				float angle = m_LockCompleteEffectTime / effect_pi * Mathf.PI;
				color = Color.Lerp(m_LockedColor, m_UnlockColor, Mathf.Cos(angle)*0.5f + 0.5f);
			}
			else
			{
				color = m_LockedColor;
			}
		}

		float circle_size = (m_ProgressFSprite.fillAmount < 0.999f) ? 40 : Mathf.Lerp(40,90,Mathf.Pow(m_LockCompleteEffectTime*5f, 2f));
		float circle_alpha = (m_ProgressFSprite.fillAmount < 0.999f) ? 1 : Mathf.Lerp(0.6f,0,Mathf.Pow(m_LockCompleteEffectTime*5f, 0.4f));

		if ( m_ProgressFSprite.fillAmount > 0.001f && m_ProgressFSprite.fillAmount < 0.999f )
		{
			m_LockProgressText.text = (m_ProgressFSprite.fillAmount * 100).ToString("0") + " %";
		}
		else if ( m_ProgressFSprite.fillAmount < 0.002f )
		{
			m_LockProgressText.text = "'X'-" + "Lockon".ToLocalizationString();
		}
		else if ( m_ProgressFSprite.fillAmount > 0.998f )
		{
			if ( m_LockCompleteEffectTime < 1 )
				m_LockProgressText.text = "Locked on".ToLocalizationString();
			else
				m_LockProgressText.text = "'C'-" + "Fire".ToLocalizationString();
		}

		m_TA0.color = color;
		m_TA90.color = color;
		m_TA180.color = color;
		m_TA270.color = color;
		m_Deco0.color = color;
		m_Deco1.color = color;
		m_ProgressFSpriteBG.color = color;

		m_TA0.alpha = alpha;
		m_TA90.alpha = alpha;
		m_TA180.alpha = alpha;
		m_TA270.alpha = alpha;
		m_ProgressFSpriteBG.alpha = alpha * 0.3f;
		m_ProgressFSprite.alpha = alpha * circle_alpha;
		m_Deco0.alpha = alpha;
		m_Deco1.alpha = alpha;
		m_LockProgressText.alpha = alpha;

		m_ProgressFSprite.transform.localScale = new Vector3 (circle_size, circle_size, 1);
	}

	public void UpdatePos ()
	{
		if ( m_TargetObject != null && PETools.PEUtil.MainCamTransform != null )
		{
			Vector3 pos = m_TargetObject.transform.position + Vector3.up;
			if ( m_TargetObject.GetComponent<Rigidbody>() != null )
				pos = m_TargetObject.GetComponent<Rigidbody>().worldCenterOfMass;

			if ( Vector3.Dot(PETools.PEUtil.MainCamTransform.forward, (pos - PETools.PEUtil.MainCamTransform.position).normalized) > 0.2f )
				transform.localPosition = Camera.main.WorldToScreenPoint(pos);
			else
				transform.localPosition = new Vector3 (-300,-300,0);
			NGUITools.MakePixelPerfect(transform);
		}
	}
}
