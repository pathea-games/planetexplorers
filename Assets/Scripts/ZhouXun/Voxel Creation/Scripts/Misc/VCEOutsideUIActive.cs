using UnityEngine;
using System.Collections;

public class VCEOutsideUIActive : MonoBehaviour
{
	private bool lastVCEditorActive = false;
	public bool revertEnable = false;
	
	// Use this for initialization
	void Start ()
	{
		lastVCEditorActive = false;
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
		if ( VCEditor.s_Active && !lastVCEditorActive )
		{
			revertEnable = GetComponent<Camera>().enabled;
			GetComponent<Camera>().enabled = false;
			GetComponent<UICamera>().enabled = false;
		}
		if ( !VCEditor.s_Active && lastVCEditorActive )
		{
			GetComponent<Camera>().enabled = revertEnable;
			GetComponent<UICamera>().enabled = revertEnable;
		}
		lastVCEditorActive = VCEditor.s_Active;
	}
}
