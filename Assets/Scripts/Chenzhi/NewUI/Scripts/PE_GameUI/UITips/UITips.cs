using UnityEngine;
using System.Collections;

public class UITips : MonoBehaviour 
{
	public UITipsWndCtrl tipWnds;
	public UITipListControl  tipList;


	void Awake ()
	{
		PeTipsMsgMan.Instance.onAddTipMsg += OnAddNewTipsMsg;
	}

	void OnDestroy()
	{
		PeTipsMsgMan.Instance.onAddTipMsg -= OnAddNewTipsMsg;
	}

	void OnAddNewTipsMsg (PeTipMsg tipMsg)
	{
        tipList.AddMsg(tipMsg);
	}
}
