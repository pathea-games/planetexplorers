using UnityEngine;
using System.Collections;

public class VCEMsgBox : MonoBehaviour
{
	public VCEMsgBoxType m_Type;
	public int m_PopupFrame;
	public GameObject m_ButtonL;
	public GameObject m_ButtonC;
	public GameObject m_ButtonR;
	public UILabel m_Title;
	public UILabel m_Message;
	public UISprite m_Icon;
	public UITweener m_MsgBoxTween;
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	private VCEMsgBoxButton m_RespButton;
	public void OnLBtnClick()
	{
		m_ButtonL.SetActive(false);
		m_ButtonR.SetActive(false);
		m_ButtonC.SetActive(false);
		m_RespButton = VCEMsgBoxButton.L;
		m_MsgBoxTween.Play(false);
	}
	public void OnCBtnClick()
	{
		m_ButtonL.SetActive(false);
		m_ButtonR.SetActive(false);
		m_ButtonC.SetActive(false);
		m_RespButton = VCEMsgBoxButton.C;
		m_MsgBoxTween.Play(false);
	}
	public void OnRBtnClick()
	{
		m_ButtonL.SetActive(false);
		m_ButtonR.SetActive(false);
		m_ButtonC.SetActive(false);
		m_RespButton = VCEMsgBoxButton.R;
		m_MsgBoxTween.Play(false);
	}
	public void OnCollapse()
	{
		if ( this.m_MsgBoxTween.transform.localScale.magnitude < 0.1f )
		{
			VCEMsgBoxResponse.Response(m_Type, m_RespButton, m_PopupFrame);
			GameObject.Destroy(this.gameObject);
		}
	}
	
	//
	// ------------- Static ------------------------------------
	//
	public static string s_MsgBoxResPath = "GUI/Prefabs/VCE Message Box";
	public static string s_MsgBoxGroupName = "VCE Message Box Group";
	public static void Show (VCEMsgBoxType type)
	{
		GameObject group = GameObject.Find(s_MsgBoxGroupName);
		if ( group != null )
		{
			GameObject msgbox_go = GameObject.Instantiate(Resources.Load(s_MsgBoxResPath) as GameObject) as GameObject;
			Vector3 scale = msgbox_go.transform.localScale;
			msgbox_go.name = "Message Box " + Time.frameCount.ToString() + " (" + type.ToString() + ")";
			msgbox_go.transform.parent = group.transform;
			msgbox_go.transform.localPosition = Vector3.zero;
			msgbox_go.transform.localRotation = Quaternion.identity;
			msgbox_go.transform.localScale = scale;
			VCEMsgBoxDesc desc = new VCEMsgBoxDesc (type);
			VCEMsgBox msgbox = msgbox_go.GetComponent<VCEMsgBox>();
			msgbox.m_Type = type;
			msgbox.m_PopupFrame = Time.frameCount;
			msgbox.m_Title.text = desc.Title;
			msgbox.m_Message.text = desc.Message;
			msgbox.m_Icon.spriteName = desc.Icon;
			msgbox.m_ButtonL.GetComponentsInChildren<UILabel>(true)[0].text = desc.ButtonL;
			msgbox.m_ButtonR.GetComponentsInChildren<UILabel>(true)[0].text = desc.ButtonR;
			msgbox.m_ButtonC.GetComponentsInChildren<UILabel>(true)[0].text = desc.ButtonC;
			if ( desc.ButtonL.Length > 0 )
				msgbox.m_ButtonL.SetActive(true);
			else
				msgbox.m_ButtonL.SetActive(false);
			if ( desc.ButtonR.Length > 0 )
				msgbox.m_ButtonR.SetActive(true);
			else
				msgbox.m_ButtonR.SetActive(false);
			if ( desc.ButtonC.Length > 0 )
				msgbox.m_ButtonC.SetActive(true);
			else
				msgbox.m_ButtonC.SetActive(false);
			if ( type == VCEMsgBoxType.DELETE_ISO )
				msgbox.m_ButtonL.GetComponentsInChildren<UILabel>(true)[0].color = Color.red;
			msgbox_go.SetActive(true);
		}
	}
}
