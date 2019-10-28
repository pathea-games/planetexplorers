using Pathea;

public class ItemScript_Carrier : ItemScript
{
    Pathea.PassengerCmpt mPlayerPassengerCmpt = null;
    public Pathea.PassengerCmpt playerPassengerCmpt
    {
        get
        {
            if (null == mPlayerPassengerCmpt)
            {
                if (null != Pathea.PeCreature.Instance.mainPlayer)
                {
                    mPlayerPassengerCmpt = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PassengerCmpt>();
                }
            }

            return mPlayerPassengerCmpt;
        }
    }

    public bool IsPlayerOnCarrier()
    {
        if(playerPassengerCmpt == null)
        {
			return false;
        }

        return playerPassengerCmpt.IsOnCarrier();
    }

    public int PassengerCountOnSeat()
    {
        var controller = GetComponent<WhiteCat.CarrierController>();

        if (controller == null)
        {
            return 0;
        }

        return controller.passengerCount;
    }

    public void GetOn()
    {
        if (playerPassengerCmpt == null)
        {
            return;
        }

		var controller = GetComponent<WhiteCat.CarrierController>();
		int seatIndex = controller.FindEmptySeatIndex();

		if(seatIndex < -1) return;

        if (GameConfig.IsMultiMode)
        {
			MotionMgrCmpt mmc = PeCreature.Instance.mainPlayer.GetCmpt<MotionMgrCmpt>();
			if(null != mmc)
			{
				PEActionParamDrive param = PEActionParamDrive.param;
				param.controller = controller;
				param.seatIndex = seatIndex;
				if(mmc.CanDoAction(PEActionType.Drive, param))
					if (null != mNetlayer && null != PlayerNetwork.mainPlayer && !ForceSetting.Instance.Conflict(mNetlayer.TeamId, PlayerNetwork.mainPlayerId))
						PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_GetOnVehicle, mNetlayer.Id);
			}
        }
        else
        {
			if(null != PeCreature.Instance.mainPlayer)
			{
				PassengerCmpt passenger = PeCreature.Instance.mainPlayer.GetCmpt<PassengerCmpt>();
				if(null != passenger)
					passenger.GetOn(controller, seatIndex, true);
			}
//			controller.GetOn(Pathea.PeCreature.Instance.mainPlayer, seatIndex);
        }
    }

    public void Repair()
    {
        if (null != mNetlayer && null != PlayerNetwork.mainPlayer)
            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RepairVehicle, mNetlayer.Id);
    }

    public void Charge()
    {
        if (null != mNetlayer && null != PlayerNetwork.mainPlayer)
            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ChargeVehicle, mNetlayer.Id);
    }
}
