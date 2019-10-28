using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Pathea;

public class DragItemMousePickColony : DragItemMousePick
{
    public CSBuildingLogic csbl;
    protected override void InitCmd(CmdList cmdList)
	{
		cmdList.Add("Open", OnOpen);
		cmdList.Add("Get", OnGetBtn);
		cmdList.Add("Turn", Turn90Degree);
	}

    public override bool CanCmd()
    {
        if(csbl==null)
            csbl = GetComponentInParent<CSBuildingLogic>();
        if (PeGameMgr.IsMulti&&csbl!=null)
            if (BaseNetwork.MainPlayer.TeamId != csbl.network.TeamId)
                return false;
        return base.CanCmd();
    }
	public override void DoGetItem ()
	{
		if(null == pkg || null == itemObj || !pkg.CanAdd(itemObj))
		{	
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
			return;
		}
		CSEntityObject ceo = GetComponent<CSEntityObject>();
		if (ceo != null)
		{
			float dura = ceo.m_Entity.BaseData.m_Durability;
			float maxDura = ceo.m_Entity.m_Info.m_Durability;
			ceo.m_Entity.BaseData.m_Durability = dura - Mathf.Floor(maxDura * 0.1f); 
			if ( ceo.m_Creator.RemoveEntity(ceo.m_Entity.ID, false) != null)
			{
				base.DoGetItem();
			}
		}
        if (!PeGameMgr.IsMulti)
        {
            SendMessage("OnRemoveGo", itemObjectId, SendMessageOptions.DontRequireReceiver);
        }
	}

	public override void OnGetBtn ()
	{
		//base.OnGetBtn ();
        //if (!GameConfig.IsMultiMode)
        //{
			CSEntityObject ceo = GetComponent<CSEntityObject>();
			if (ceo == null)
				return;

			if(EntityMonsterBeacon.IsRunning())
			{
				PeTipMsg.Register(PELocalization.GetString(8000622), PeTipMsg.EMsgLevel.Warning);
				CloseOn();
				return;
			}
			
			if (ceo.m_Entity.BaseData.m_Durability < ceo.m_Entity.m_Info.m_Durability * 0.15f)
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000084));
			else
			{
				if (ceo as CSDwellingsObject != null)
					MessageBox_N.ShowYNBox(PELocalization.GetString(8000085), GetOn, CloseOn);
				else
					MessageBox_N.ShowYNBox(PELocalization.GetString(8000086), GetOn, CloseOn);
			}
        //}
	}

	public override void Turn90Degree ()
	{
        base.Turn90Degree();
        if (!GameConfig.IsMultiMode)
        {
            CSEntityObject ceo = GetComponent<CSEntityObject>();
            if (ceo != null)
            {
                csbl = GetComponentInParent<CSBuildingLogic>();
                if (csbl != null)
                {
                    ceo.Init(csbl, ceo.m_Creator, false);

                    csbl.m_Entity.AfterTurn90Degree();
                }
                
                //else{
                //        ceo.Init(itemObjectId, ceo.m_Creator, false);
                //    }
            }
        }
            
        OnItemOpGUIHide();
	}

	public void OnOpen()
	{
		CSEntityObject ceo = GetComponent<CSEntityObject>();
        if (CSUI_MainWndCtrl.Instance != null && ceo != null)
		{
            //CSUI_MainWndCtrl.Instance.AwakeWindow();
            CSUI_MainWndCtrl.Instance.ShowWndPart(ceo.m_Entity);
		}

        HideItemOpGui();

		OnItemOpGUIHide();
	}


	public void OnItemOpGUIActive()
	{
        //delegate maybe call after destroy.
        if (this == null)
        {
            return;
        }

		CSCommonObject csco = GetComponent<CSCommonObject>();
		if (csco != null)
		{
			csco.ShowWorkSpaceEffect();
		}
	}

	public void OnItemOpGUIHide()
	{
        //delegate maybe call after destroy.
        if (this == null)
        {
            return;
        }

		CSCommonObject csco = GetComponent<CSCommonObject>();
		if (csco != null)
		{
			csco.HideWorkSpaceEffect();
		}
	}

	private void CloseOn()
	{
        HideItemOpGui();

		OnItemOpGUIHide();
	}

	private void GetOn()
	{
		base.OnGetBtn();
	}

    protected override void CheckOperate()
	{
        if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && CanCmd())
        {
            GameUI.Instance.mItemOp.ListenEvent(OnItemOpGUIHide, OnItemOpGUIActive);
        }

		base.CheckOperate ();

		if(PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
		{
			OnOpen();
		}
	}


    //--to do: OnTowerDeath
    //void OnTowerDeath(AiObject aiObject)
    //{
    //    SendMessage("OnRemoveGo", mItemObj.instanceId);
    //}

    protected override string tipsText
    {
		get {
			return base.tipsText  + "\n" + PELocalization.GetString(8000141);
		}
	}
}
