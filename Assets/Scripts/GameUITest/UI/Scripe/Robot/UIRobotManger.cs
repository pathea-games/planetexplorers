using UnityEngine;
using System;
using System.Collections;
using ItemAsset;
using WhiteCat;

public class UIRobotManger : MonoBehaviour
{
    [SerializeField]
    UIRobotItem mRobot;
    [SerializeField]
    UIGrid mServantGrid;
    [SerializeField]
    UIGrid mRobotGrid;

    [SerializeField]
    Transform Centent = null;

    bool mNeedReposition = false;

    LifeLimit mLifeCmpt = null;
    Energy mEnergyCmpt = null;
    bool mShowLifeTip = true;
    bool mShowEnergyTip = true;

    void Awake()
    {
        RobotController.onPlayerGetRobot += OnCreatRobot;
        RobotController.onPlayerLoseRobot += OnDeleteRobot;
    }

    void OnDestroy()
    {
        RobotController.onPlayerGetRobot -= OnCreatRobot;
        RobotController.onPlayerLoseRobot -= OnDeleteRobot;
    }

    void ResetPostion()
    {
        Vector3 oldPos = mServantGrid.transform.localPosition;
        if (mNeedReposition)
        {
            mServantGrid.transform.localPosition = new Vector3(oldPos.x, -54f, oldPos.z);
        }
        else
        {
            mServantGrid.transform.localPosition = new Vector3(oldPos.x, 0f, oldPos.z);
        }
    }

    void Update()
    {
        //死亡提示
        if (mLifeCmpt != null && mShowLifeTip)
        {
            if (mLifeCmpt.floatValue.percent == 0f)
            {
                mShowLifeTip = false;
                new PeTipMsg(PELocalization.GetString(8000177), PeTipMsg.EMsgLevel.HighLightRed);
            }
        }

        //能量消耗完提示
        if (mEnergyCmpt != null && mShowEnergyTip)
        {
            if (mEnergyCmpt.floatValue.percent == 0f)
            {
                mShowEnergyTip = false;
                new PeTipMsg(PELocalization.GetString(8000176), PeTipMsg.EMsgLevel.HighLightRed);
                new PeTipMsg(PELocalization.GetString(8000176), PeTipMsg.EMsgLevel.HighLightRed);
                new PeTipMsg(PELocalization.GetString(8000176), PeTipMsg.EMsgLevel.HighLightRed);
            }
        }

        // 为了处理玩家回收机器人后，不能触发事件的问题。
        if (!RobotController.playerFollower && mRobot.IsShow)
        {
            OnDeleteRobot();
        }

        Centent.localPosition = new Vector3(-UIMinMapCtrl.Instance.GetMinMapWidth() - 20, -5, -22);
    }

    #region CallBack

    void OnCreatRobot(ItemObject obj, GameObject gameobj)
    {
        if (obj == null)
            return;

        mLifeCmpt = obj.GetCmpt<LifeLimit>();
        mEnergyCmpt = obj.GetCmpt<Energy>();
        mShowLifeTip = true;
        mShowEnergyTip = true;

        mRobot.mItemObj = obj;
        mRobot.mGameobj = gameobj;
        mRobot.Show();
        mNeedReposition = true;
        ResetPostion();
    }

    void OnDeleteRobot()
    {
        mRobot.mItemObj = null;
        mRobot.mGameobj = null;
        mRobot.Close();
        mNeedReposition = false;
        ResetPostion();
    }

    #endregion
}
