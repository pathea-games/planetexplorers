using Pathea;
public class DragItemMousePickCarrier : DragItemMousePickCreation
{
    public override bool CanCmd()
    {
        if (!base.CanCmd())
        {
            return false;
        }

		if(Pathea.PeGameMgr.IsMulti)
		{
			ItemScript itemScript = GetScript();
			if(null != itemScript && null != itemScript.netLayer)
			{
				if(ForceSetting.Instance.Conflict(BaseNetwork.MainPlayer.Id, itemScript.netLayer.TeamId))
					return false;
			}
		}

        ItemScript_Carrier carrier = GetComponent<ItemScript_Carrier>();
        if (carrier == null)
        {
            return false;
        }

        return !carrier.IsPlayerOnCarrier();
    }

    protected override void InitCmd(CmdList cmdList)
    {
        cmdList.Add("Get On", GetOnCarrier);
        //if (GameConfig.IsMultiMode)
       // {
            //cmdList.Add("Repair", OnRepair);
            //cmdList.Add("Charge", OnCharge);
       // }

        cmdList.Add("Get", OnGetBtn);
    }

    public override void DoGetItem()
    {
        ItemScript_Carrier carrier = GetComponent<ItemScript_Carrier>();
        if (carrier == null)
        {
            return;
        }

        if (carrier.PassengerCountOnSeat() > 0)
        {
            return;
        }

        base.DoGetItem();
    }

    void GetOnCarrier()
    {
        ItemScript_Carrier carrier = GetComponent<ItemScript_Carrier>();
        if (carrier != null)
        {
            carrier.GetOn();
        }

        HideItemOpGui();
    }

    void OnRepair()
    {
        ItemScript_Carrier carrier = GetComponent<ItemScript_Carrier>();
        if (carrier != null)
        {
            carrier.Repair();
        }

        HideItemOpGui();
    }
    void OnCharge()
    {
        ItemScript_Carrier carrier = GetComponent<ItemScript_Carrier>();
        if (carrier != null)
        {
            carrier.Charge();
        }

        HideItemOpGui();
    }

    protected override string tipsText
    {
        get
        {
            return base.tipsText + "\n" + PELocalization.GetString(8000130);
        }
    }
}
