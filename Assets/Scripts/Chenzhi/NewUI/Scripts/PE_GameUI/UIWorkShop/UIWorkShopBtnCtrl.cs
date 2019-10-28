using UnityEngine;
using System.Collections;

public class UIWorkShopBtnCtrl : MonoBehaviour 
{
	UIWorkShopCtrl mWorkShopCtrl = null;
	public GameObject mWorkShopPrefab;
	public GameObject mCenterAuthor;
	// Use this for initialization
	void Start () 
	{
		if (!GameConfig.IsMultiMode)
		{
			this.gameObject.SetActive(false);
			return;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void BtnWorkShopOnClick()
	{
		if (mWorkShopCtrl == null && GameConfig.IsMultiMode)
		{
			GameObject workShop =  GameObject.Instantiate(mWorkShopPrefab) as GameObject;
			workShop.transform.transform.parent = mCenterAuthor.transform;
			workShop.transform.localPosition = new Vector3(0,40,0);
			workShop.transform.localScale = Vector3.one;
			mWorkShopCtrl = workShop.GetComponent<UIWorkShopCtrl>();
			if (mWorkShopCtrl == null)
				return;
			mWorkShopCtrl.e_BtnClose += WorkShopOnClose;
			workShop.SetActive(true);
		}
		else
		{
			mWorkShopCtrl.gameObject.SetActive(true);
		}
	}
	void WorkShopOnClose()
	{
		if (mWorkShopCtrl == null)
			return;
		GameObject.Destroy(mWorkShopCtrl.gameObject);
		mWorkShopCtrl = null;
	}
}
