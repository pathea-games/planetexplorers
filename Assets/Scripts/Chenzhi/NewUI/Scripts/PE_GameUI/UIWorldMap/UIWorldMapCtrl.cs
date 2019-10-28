using UnityEngine;
using System.Collections;
using PeMap;
using System.Collections.Generic;
using PeUIMap;
using Pathea;

public class UIWorldMapCtrl : UIStaticWnd 
{
	[SerializeField] UIMap mStroyMap;
	[SerializeField] UIMap mRandomMap;
	UIMap mMap;

    //lz-2016.07.26 获取当前使用的地图，保证访问的时候不为null
    public UIMap CurMap
    {
        get
        {
            if (null == mMap)
            {
                mMap = bRandomMap ? mRandomMap : mStroyMap;
            } 
            return mMap;
        }
    }

	bool bRandomMap { get {return PeGameMgr.IsAdventure||PeGameMgr.IsBuild;}}

	public override void Show ()
	{
        if (PeGameMgr.IsCustom)
            return;
		base.Show ();
        CurMap.Show();
        //lz-2016.10.25 打开地图的时候隐藏锁定UI
        if (null != WhiteCat.LockUI.instance)
            WhiteCat.LockUI.instance.HideWhenUIPopup();
    }

    protected override void OnHide()
	{
		base.OnHide();
        CurMap.Hide();
        //lz-2016.10.25 关闭地图的时候显示锁定UI
        if (null != WhiteCat.LockUI.instance)
            WhiteCat.LockUI.instance.ShowWhenUIDisappear();
    }



	#region btn event

	void MaskYes()
	{
        CurMap.MaskYes();
	}

	void WarpYes()
	{
        CurMap.OnWarpYes();
	}

	void OnSelfMask0()
	{
        CurMap.ChangeMaskIcon(0);
	}

	void OnSelfMask1()
	{
        CurMap.ChangeMaskIcon(1);
	}

	void OnSelfMask2()
	{
        CurMap.ChangeMaskIcon(2);
	}

	void OnSelfMask3()
	{
        CurMap.ChangeMaskIcon(3);
	}

	#endregion

}
