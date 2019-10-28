using UnityEngine;
using System.Collections;

public class UIBgAlphaCtrl : MonoBehaviour 
{
	[SerializeField] BoxCollider mCollider = null;
	[SerializeField] UISprite mBg_1;
	[SerializeField] UISprite mBg_2;
	[SerializeField] UISprite mBtnClose;
	[SerializeField] GameObject mSpecular;

	// Use this for initialization
	void Start () 
	{


	}
	
	// Update is called once per frame
	void Update () 
	{
		if (mCollider == null || UICamera.currentCamera == null || mBg_1 == null || mBg_2 == null)
		{
			return;
		}
		
		Ray ray = UICamera.currentCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		RaycastHit rayHit;
		
		bool ok = mCollider.Raycast(ray,out rayHit,100);

		mBg_1.enabled = ok;
		mBg_2.enabled = ok;
		mBtnClose.enabled = ok;
		mSpecular.SetActive(ok);
	}
}
