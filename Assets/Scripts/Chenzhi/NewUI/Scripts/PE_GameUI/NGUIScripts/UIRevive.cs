using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using TownData;
using Pathea;
using SkillSystem;

public class UIRevive : UIBaseWnd
{
    public UILabel mName;
    public UILabel mLeftTimeLabal;
    public Grid_N mGrid;
    public Grid_N mGrid_2;
    public UITexture mHeadTex;
    [SerializeField]
    N_ImageButton mPlayerOkBtn;
    [SerializeField]
    N_ImageButton mPlayerCancelBtn;
    [SerializeField]
    N_ImageButton mServantOkBtn;
    [SerializeField]
    N_ImageButton mServantCancelBtn;

    public Transform mTsPlayer;
    public Transform mTsServant;

    const int mRevivePrtroId = 937;

    enum ReivieState
    {
        None = 0,
        Reivie_Player,
        Reivie_Servant,
    }
    ReivieState currentState = ReivieState.None;

    PlayerPackageCmpt playerPackage = null;
    PeEntity currentEntity = null;
    ItemSample item;
    public override void OnCreate()
    {
        base.OnCreate();
        Pathea.MainPlayer.Instance.mainPlayerCreatedEventor.Subscribe((sender, arg) =>
        {
            AttachEvent();
        });
        if (null != Pathea.MainPlayer.Instance.entity)
        {
            AttachEvent();
        }

        //lz-2016.08.03 把复活界面的按钮发消息触发改为事件，避免频繁发消息对象问题报错
        UIEventListener.Get(mPlayerOkBtn.gameObject).onClick += (go) => { BtnClick_OnRevive(); };
        UIEventListener.Get(mPlayerCancelBtn.gameObject).onClick += (go) => { BtnClick_OnCancel(); };
        UIEventListener.Get(mServantOkBtn.gameObject).onClick += (go) => { BtnClick_OnServentRevive(); };
        UIEventListener.Get(mServantCancelBtn.gameObject).onClick += (go) => { BtnClick_OnCancel(); };
    }

    void Start()
    {
        item = new ItemSample(mRevivePrtroId);
        mGrid.SetItem(item);
        mGrid_2.SetItem(item);
    }

    //float m_DelayTime = 100000.0f;
    //bool mStartReciprocal = false;

    public override void Show()
    {
        if (currentState == ReivieState.None)
        {
            Hide();
            return;
        }
        base.Show();

        playerPackage = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();

        mTsPlayer.gameObject.SetActive(currentState == ReivieState.Reivie_Player);
        mTsServant.gameObject.SetActive(currentState == ReivieState.Reivie_Servant);

        //mStartReciprocal = true;

        // m_DelayTime = 100000.0f;
    }


    public void mUpdate()
    {
		bool bCanRevive = CanRevive ();
		mPlayerOkBtn.isEnabled = bCanRevive;
		mServantOkBtn.isEnabled = bCanRevive;
		int nItem = UINPCfootManMgr.Instance.mItemList.Count;
		for (int i = 0; i < nItem; i++) {
			UIfootManItem item = UINPCfootManMgr.Instance.mItemList [i];
			if (null != item.npcCmpt) {
				if (item.npcCmpt == mServantRevided)
					SetReviveWaitTime (item.ReviveTimer);
				if (item.ReviveTimer <= 0f) {
					if (ServantLeaderCmpt.Instance != null) {
						ServantLeaderCmpt.Instance.RemoveServant (item.npcCmpt.Entity.GetCmpt<NpcCmpt> ());
						HideServantRevive ();
					}
				}
			}
		}

        //if (mStartReciprocal)
        //{
        //    if (currentState == ReivieState.Reivie_Servant)//仆从
        //    {
        //        foreach (UIfootManItem item in UINPCfootManMgr.Instance.mItemList)
        //        {
        //            if (mServantRevided == item.npcCmpt)
        //            {
        //                SetReviveWaitTime(item.mReviveTimer);
        //                if (item.mReviveTimer <= 0f)
        //                {
        //                    if ((ServantLeaderCmpt.Instance != null) && (mServantRevided != null))
        //                    {
        //                        ServantLeaderCmpt.Instance.RemoveServant(mServantRevided.Entity.GetCmpt<NpcCmpt>());
        //                        HideServantRevive();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else if (currentState == ReivieState.Reivie_Player)//玩家
        //    {
        //        //do nothing
        //    }
        //}
    }

    #region Player
    // 注册玩家死亡触发事件
    void AttachEvent()
    {
        SkAliveEntity skAlive_player = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<SkAliveEntity>();
        if (skAlive_player != null)
        {
            skAlive_player.deathEvent += OnPlayerDead;
            if (skAlive_player.isDead)
                OnPlayerDead(skAlive_player, null);
        }
    }

    void OnPlayerDead(SkEntity skSelf, SkEntity skCaster)
    {
        currentState = ReivieState.Reivie_Player;
        currentEntity = (skSelf as SkAliveEntity).Entity;
        Show();
    }
    #endregion

    #region Servant
    public void ShowServantRevive(PeEntity servant)
    {
        return;
//        currentState = ReivieState.Reivie_Servant;
//        currentEntity = servant;
//        Show();
    }


    NpcCmpt mServantRevided;
    //luwei
    public void ShowServantRevive(NpcCmpt servant)
    {
        //return ;
        SkAliveEntity skAlive_player = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<SkAliveEntity>();
        if (skAlive_player.isDead)
            return;

        currentState = ReivieState.Reivie_Servant;
        currentEntity = servant.Entity;
        SetServantReviveInfo(Pathea.PeEntityExt.PeEntityExt.ExtGetFaceTex(servant.Follwerentity),
                             Pathea.PeEntityExt.PeEntityExt.ExtGetName(currentEntity)
        );
        mServantRevided = servant;
        Show();
    }

    public void HideServantRevive()
    {
        //mStartReciprocal = false;
        if (currentState == ReivieState.Reivie_Servant || currentEntity == null)
        {
            Hide();
        }

        return;
        //lw:2017.3.14:仆从死亡后，解除关系时，会将玩家复活
        //if (!PeGameMgr.IsMulti)
        //{
        //    DoRevive(true);
        //    //Pathea.FastTravelMgr.Instance.TravelTo(GetNearFastTrvalPos(currentEntity.position));
        //    Hide();
        //}
        //else
        //{
        //    Vector3 warpPos = Vector3.zero;
        //    if (PeGameMgr.IsMultiCoop)
        //    {
        //        warpPos = GetNearFastTrvalPos(currentEntity.position);
        //    }
        //    else
        //    {
        //        IntVector2 posXZ = VArtifactUtil.GetSpawnPos();
        //        warpPos = new Vector3(posXZ.x, VFDataRTGen.GetPosTop(posXZ), posXZ.y);
        //    }

        //    if (null != PlayerNetwork.mainPlayer)
        //    {
        //        PlayerNetwork.mainPlayer.RequestChangeScene(0);
        //        PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerReset, warpPos);
        //    }                
        //}
    }

    public void ShowServantRevive()
    {
        if (UINPCfootManMgr.Instance == null)
        {
            return;
        }

        //if (UINPCfootManMgr.Instance.IsRevive())
        //{
        //    NpcCmpt servant = UINPCfootManMgr.Instance.GetDeadServant();
        //    if (servant != null)
        //    {
        //        ShowServantRevive(servant);
        //    }
        //}
        NpcCmpt _servant = GameUI.Instance.mServantWndCtrl.GetNeedReviveServant();
        if (_servant != null)
        {
            ShowServantRevive(_servant);
        }

    }


    public void SetReviveWaitTime(float delayTime)
    {
        int Times = (int)delayTime;

        int hours = ((Times / 60) / 60) % 24;
        int minute = (Times / 60) % 60;
        int second = Times % 60;

        mLeftTimeLabal.text = hours.ToString() + ":" + minute.ToString() + ":" + second.ToString();
    }

    void SetServantReviveInfo(Texture head, string name)
    {
        mName.text = name;
        mHeadTex.mainTexture = head;
    }
    #endregion

    public bool CanRevive()
    {
		if (playerPackage == null)
        {
			if(GameUI.Instance != null && GameUI.Instance.mMainPlayer != null){
				playerPackage = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
			}
			if(playerPackage == null)
				return false;
        }
		return playerPackage.ContainsItem(mRevivePrtroId);
    }

    bool DoRevive(bool immediately = false)
    {
        MotionMgrCmpt motionMgrCmpt = currentEntity.GetCmpt<MotionMgrCmpt>();
		PEActionParamB paramB = PEActionParamB.param;
        if (immediately)
		{
			paramB.b = true;
			motionMgrCmpt.DoActionImmediately(PEActionType.Revive, paramB);
		}
		else
		{
			paramB.b = false;
			motionMgrCmpt.DoAction(PEActionType.Revive, paramB);
		}
        return motionMgrCmpt.IsActionRunning(PEActionType.Revive);
    }
    //获取玩家最近的传送点
    Vector3 GetNearFastTrvalPos(Vector3 playerPos)
    {
		Vector3 nearPos = playerPos;
        float distance = float.MaxValue;

        if (PeGameMgr.IsCustom)
        {
            nearPos = CustomGameData.Mgr.Instance.curGameData.curPlayer.StartLocation;
        }

        PeMap.LabelMgr.Instance.ForEach(delegate(PeMap.ILabel label)
        {
            if (label.FastTravel() == true)
            {
                float dis = Vector3.Distance(playerPos, label.GetPos());

                if (dis < distance)
                {
                    distance = dis;
                    nearPos = label.GetPos();
                }
            }
        });

        return nearPos;
    }

    #region OnClickFunc
    void BtnClick_OnRevive()
    {
        if (currentEntity == null)
            return;

        if (!PeGameMgr.IsMulti)
        {
            if (DoRevive(false))
            {
                Pathea.UseItemCmpt useItem = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.UseItemCmpt>();

                useItem.Revive();
                Hide();

            }
        }
        else
        {
            if (null != PlayerNetwork.mainPlayer)
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerRevive, currentEntity.position);
            Hide();
        }
    }

    void BtnClick_OnServentRevive()
    {
        if (mServantRevided != null && !mServantRevided.CanRecive)
            return;
        if (currentEntity == null)
            return;

        foreach (UIfootManItem item in UINPCfootManMgr.Instance.mItemList)
        {
            if (null != item.npcCmpt)
            {
                if (item.npcCmpt == mServantRevided)
                    item.InitReviveTime();
            }
        }

        if (!PeGameMgr.IsMulti)
        {
            if (DoRevive(false))
            {
                Pathea.UseItemCmpt ServentItem = currentEntity.GetCmpt<Pathea.UseItemCmpt>();

                if (null == ServentItem)
                    ServentItem = currentEntity.Add<Pathea.UseItemCmpt>();

                ServentItem.ReviveServent();
                Hide();
            }
        }
        else
        {
            PlayerNetwork.RequestServantRevive(currentEntity.Id, currentEntity.position);
            Hide();
        }
    }


    void BtnClick_OnCancel()
    {
        if (currentState == ReivieState.Reivie_Servant || currentEntity == null)
        {
            Hide();
            return;
        }
        if (!PeGameMgr.IsMulti)
        {
            ReviveLabel label = new ReviveLabel();
            label.pos = currentEntity.position;
            ReviveLabel.Mgr.Instance.Add(label);

			if(RandomDungenMgrData.InDungeon){
				bool reviveSuc = DoRevive(true);
				if(reviveSuc){
					currentEntity.position = RandomDungenMgrData.revivePos;
					Hide();
				}
				return;
			}

            //lz-2016.08.03 空对象
            if (null != MissionManager.Instance)
            {
                MissionManager.Instance.RemoveFollowTowerMission();
            }
            DoRevive(true);
            Vector3 revivePos;
            if (SingleGameStory.curType == SingleGameStory.StoryScene.DienShip0)
                revivePos = new Vector3(14798.09f, 20.98818f, 8246.396f);
            else if (SingleGameStory.curType == SingleGameStory.StoryScene.L1Ship)
                revivePos = new Vector3(9649.354f, 90.488f, 12744.77f);
            else if (SingleGameStory.curType == SingleGameStory.StoryScene.PajaShip)
                revivePos = new Vector3(1593.53f, 148.635f, 8022.03f);
	        else
                revivePos = GetNearFastTrvalPos(currentEntity.position);
            Pathea.FastTravelMgr.Instance.TravelTo(revivePos);
        }
        else
        {
            Vector3 warpPos = Vector3.zero;
			if(RandomDungenMgrData.InDungeon){
				warpPos = RandomDungenMgrData.revivePos;
			}
            else if (PeGameMgr.IsMultiCoop)
            {
                warpPos = GetNearFastTrvalPos(currentEntity.position);
            }
            else
            {
                if (PeGameMgr.IsCustom)
                {
                    warpPos = PlayerNetwork.mainPlayer.GetCustomModePos();
                }
                else
                {
                    IntVector2 posXZ = VArtifactUtil.GetSpawnPos();
                    warpPos = new Vector3(posXZ.x, VFDataRTGen.GetPosTop(posXZ), posXZ.y);
                }
            }

            PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_PlayerReset, warpPos);
            PlayerNetwork.mainPlayer.RequestChangeScene(0);
        }

        Hide();
    }

    #endregion

}


