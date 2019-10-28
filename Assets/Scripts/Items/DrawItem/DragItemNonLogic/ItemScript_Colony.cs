using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;


public class ItemScript_Colony : ItemScript
{
	// Colony System use
//	public CSSimulatorAttr m_SimulatorAttr;

    public CSBuildingLogic csbl;
    public override void OnConstruct()
    {
        base.OnConstruct();
        csbl = GetComponentInParent<CSBuildingLogic>();
        if(!GameConfig.IsMultiMode)
        {
		    CSMgCreator creator = CSMain.s_MgCreator;
		    if (creator != null)
		    {
			    CSEntityObject ceo = GetComponent<CSEntityObject>();
                int r;
                if (csbl != null)
                    r = ceo.Init(csbl, creator);
                else
                    r = ceo.Init(itemObjectId, creator);

			    if (r != CSConst.rrtSucceed)
			    {
				    Debug.LogError("Error with Init Entities");
			    }
			    else
			    {

					if ( ceo.m_Type == CSConst.ObjectType.Assembly)
					{
						CSMain.SinglePlayerCheckClod();

                        //--to do: attack
                        //ColonyRunner cr = gameObject.GetComponent<ColonyRunner>();
                        //if (cr != null)
                        //{
                        //    cr.DeathHandlerEvent += OnTowerDeath;
                        //}
					}

//					SendMessage("OnPutGo", mItemObj.instanceId);
			    }
		    }

//		    SendMessage("OnCreatedGo", mItemObj.instanceId);
        }
        else
        {
            int buildingTeam;
            if (csbl != null)
                buildingTeam = csbl.TeamId;
            else
                buildingTeam = mNetlayer.TeamId;
             ;
            CSMgCreator creator;
            if (buildingTeam == BaseNetwork.MainPlayer.TeamId)
            {
                creator = CSMain.s_MgCreator;
            }
            else
            {
                creator = CSMain.Instance.MultiGetOtherCreator(buildingTeam) as CSMgCreator;
            }
            if (creator != null)
            {
                CSEntityObject ceo = GetComponent<CSEntityObject>();
                ColonyNetwork colonyNetwork = mNetlayer as ColonyNetwork;
                ceo._ColonyObj = colonyNetwork._ColonyObj;
                int r;
                if (csbl != null)
                    r = ceo.Init(csbl, creator);
                else
                    r = ceo.Init(itemObjectId, creator);

                if (r != CSConst.rrtSucceed)
                {
                    Debug.LogError("Error with Init Entities");
                    //				Debug.Break();
                }
                else
                {

                    if (ceo.m_Type == CSConst.ObjectType.Assembly)
                    {
                        CSMain.SinglePlayerCheckClod();

                        //--to do: attack
                        //ColonyRunner cr = gameObject.GetComponent<ColonyRunner>();
                        //if (cr != null)
                        //{
                        //    cr.DeathHandlerEvent += OnTowerDeath;
                        //}
                    }

                    //SendMessage("OnPutGo", mItemObj.instanceId);
                }
            }
            //SendMessage("OnCreatedGo", itemObj.instanceId);
        }
	} 

	public override void OnDestruct ()
	{
		base.OnDestruct(); 
        if(!GameConfig.IsMultiMode)
		    SendMessage("OnDestoryGo", mItemObj.instanceId);
	}
}
