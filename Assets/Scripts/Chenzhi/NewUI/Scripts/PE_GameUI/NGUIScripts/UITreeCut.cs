using UnityEngine;
using System.Collections;

public class UITreeCut : UIStaticWnd
{
    static UITreeCut mInstance;
    public static UITreeCut Instance { get { return mInstance; } }
    public UISlider mProgressBar;

    bool mAttached = false;

	TreeInfo mOpTree;

    public override void OnCreate()
    {
        base.OnCreate();
        mInstance = this;
        //Pathea.MainPlayer.Instance.mainPlayerCreatedEventor.Subscribe((sender, arg) =>
        //{
        //    AttachEvent();
        //});
        //if (null != Pathea.MainPlayer.Instance.entity)
        //{
        //    AttachEvent();
        //}
    }

	public void SetSliderValue(TreeInfo treeInfo, float sValue)
    {
		if(treeInfo == mOpTree)
	        mProgressBar.sliderValue = sValue;
    }

    void AttachEvent()
    {
        Pathea.Action_Fell actionFell = Pathea.PeCreature.Instance.mainPlayer.motionMgr.GetAction<Pathea.Action_Fell>();
        if (null != actionFell)
        {
            mAttached = true;
            actionFell.startFell += StartFell;
			actionFell.endFell += EndFell;
            actionFell.hpChange += SetSliderValue;
        }
		Pathea.ServantLeaderCmpt servantLeaderCmpt = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.ServantLeaderCmpt>();
		if(null != servantLeaderCmpt)
			servantLeaderCmpt.changeEventor.Subscribe(OnServantChange);
    }

    public void StartFell(TreeInfo treeInfo)
    {
		mOpTree = treeInfo;
		SetSliderValue(mOpTree, 1f);
        Show();
    }

	void EndFell()
	{
		mOpTree = null;
		Hide();
	}

    void Update()
    {
        if (!mAttached)
        {
            TryAttachEvent();
        }
    }

    void TryAttachEvent()
    {

        Pathea.MainPlayer.Instance.mainPlayerCreatedEventor.Subscribe((sender, arg) =>
        {
            AttachEvent();
        });
        if (null != Pathea.MainPlayer.Instance.entity)
        {
            AttachEvent();
        }
    }

	void OnServantChange(object sender, Pathea.ServantLeaderCmpt.ServantChanged arg)
	{
		Pathea.Action_Fell actionFell = arg.servant.motionMgr.GetAction<Pathea.Action_Fell>();
		if(null == actionFell)
			return;
		if(arg.isAdd)
			actionFell.hpChange += SetSliderValue;
		else
			actionFell.hpChange -= SetSliderValue;
	}

}
