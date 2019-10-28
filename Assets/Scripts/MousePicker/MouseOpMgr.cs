using UnityEngine;
using Pathea;
using System.Collections.Generic;
using NaturalResAsset;

public class MouseOpMgr : Singleton<MouseOpMgr>
{
    public enum MouseOpCursor
    {
        Null,
        Gather,
        Fell,
        NPCTalk,
        LootCorpse,
        Door,
        Bed,
        Carrier,
        Ladder,
        PowerPlantSolar,
        WareHouse,
        RepairMachine,
        Colony,
        Plant,
        Tower,
        PickUpItem,
        KickStarter,
        Hand,
        Ride        //2017.02.24 坐骑的Ride
    }

    public MouseOpCursor currentState = MouseOpCursor.Null;

    void Update()
    {
        UpdateCurrentState();
    }

    void UpdateCurrentState()
    {
        currentState = MouseOpCursor.Null;

        if (null == MainPlayerCmpt.gMainPlayer)
            return;

        //if(null != MainPlayerCmpt.gMainPlayer.actionOpCursor) // always true
        {
            if (MouseOpCursor.Null != MainPlayerCmpt.gMainPlayer.actionOpCursor)
            {
                currentState = MainPlayerCmpt.gMainPlayer.actionOpCursor;
                return;
            }
        }

        if (null != MousePicker.Instance.curPickObj && !MousePicker.Instance.curPickObj.Equals(null))
        {
            MonoBehaviour mono = MousePicker.Instance.curPickObj as MonoBehaviour;
            if (MousePicker.Instance.curPickObj is ClickPEentityLootItem)
            {
                currentState = MouseOpCursor.LootCorpse;
            }
            else if (MousePicker.Instance.curPickObj is MousePickableRandomItem)
            {
                currentState = MouseOpCursor.LootCorpse;
            }
            else if (MousePicker.Instance.curPickObj is MousePickableNPC)
            {
                MousePickableNPC pickableNPC = MousePicker.Instance.curPickObj as MousePickableNPC;
                if (null != pickableNPC && null != pickableNPC.npc)
                {
                    if (pickableNPC.npc.CanTalk)
                        currentState = MouseOpCursor.NPCTalk;
                    else if (pickableNPC.npc.CanHanded)
                        currentState = MouseOpCursor.Hand;
                }
            }
            else if (MousePicker.Instance.curPickObj is DragItemMousePickBed)
                currentState = MouseOpCursor.Bed;
            else if (MousePicker.Instance.curPickObj is DragItemMousePickDoor)
                currentState = MouseOpCursor.Door;
            else if (MousePicker.Instance.curPickObj is DragItemMousePickCarrier)
                currentState = MouseOpCursor.Carrier;
            else if (MousePicker.Instance.curPickObj is DragItemMousePickLadder)
                currentState = MouseOpCursor.Ladder;
            else if (MousePicker.Instance.curPickObj is OperatableItemPowerPlantSolar)
                currentState = MouseOpCursor.PowerPlantSolar;
            else if (MousePicker.Instance.curPickObj is WareHouseObject)
                currentState = MouseOpCursor.WareHouse;
            else if (MousePicker.Instance.curPickObj is OperatableItemRepairMachine)
                currentState = MouseOpCursor.RepairMachine;
            else if (MousePicker.Instance.curPickObj is DragItemMousePickColony)
                currentState = MouseOpCursor.Colony;
            else if (MousePicker.Instance.curPickObj is DragItemMousePickPlant)
                currentState = MouseOpCursor.Plant;
            else if (MousePicker.Instance.curPickObj is DragItemMousePickTower)
                currentState = MouseOpCursor.Tower;
            else if (MousePicker.Instance.curPickObj is DragItemMousePickKickStarter)
                currentState = MouseOpCursor.KickStarter;
            else if (MousePicker.Instance.curPickObj is ClickGatherEvent)
                currentState = MouseOpCursor.Gather;
            else if (MousePicker.Instance.curPickObj is ClickFellEvent)
            {
                MotionMgrCmpt mmc = PeCreature.Instance.mainPlayer.motionMgr;
                Action_Fell af = mmc.GetAction<Action_Fell>();
                if (af.m_Axe && mmc.GetMaskState(PEActionMask.EquipmentHold))
                    currentState = MouseOpCursor.Fell;
            }
            else if (MousePicker.Instance.curPickObj is MousePickRides) //lz-2047.02.24 指向坐骑改为坐骑指针状态
                currentState = MouseOpCursor.Ride;
            else if (null != mono && null != mono.GetComponent<ItemDropMousePickRandomItem>())
                currentState = MouseOpCursor.PickUpItem;
            else if (null != mono && null != mono.GetComponent<ItemDropMousePick>())
                currentState = MouseOpCursor.PickUpItem;
        }
    }
}