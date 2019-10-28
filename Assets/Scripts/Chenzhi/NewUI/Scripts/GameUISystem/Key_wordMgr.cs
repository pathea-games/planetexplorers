using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Pathea;

public class Key_wordMgr : MonoBehaviour
{

    private static Key_wordMgr _self = null;
    public static Key_wordMgr Self
    {
        get { return _self; }
    }

    public bool enableQuickKey = true;

    void Awake()
    {
        _self = this;
        m_CursorHandler.Type = CursorState.EType.Hand;
    }

    // Use this for initialization
    void Start()
    {

    }

    bool CloseFrontWnd()
    {
        if (UIStateMgr.Instance == null)
            return false;

        if (GameUI.Instance == null)
            return false;

        if (GameUI.Instance.mUIWorldMap.isShow)
        {
            GameUI.Instance.mUIWorldMap.Hide();
            return true;
        }
        else if (GameUI.Instance.mOption.isShow)
        {
            GameUI.Instance.mOption.Hide();
            return true;
        }
        else if (GameUI.Instance.mSaveLoad.isShow)
        {
            GameUI.Instance.mSaveLoad.Hide();
            return true;
        }
        else if (MessageBox_N.IsShowing)
        {
            if (MessageBox_N.Instance)
            {
                MsgInfoType _inftype = MessageBox_N.Instance.GetCurrentInfoTypeP();
                if (!(_inftype == MsgInfoType.LobbyLoginMask || _inftype == MsgInfoType.ServerDeleteMask || _inftype == MsgInfoType.ServerLoginMask))
                    MessageBox_N.CancelMask(_inftype);
                return true;
            }
        }


        List<UIBaseWnd> wnds = UIStateMgr.Instance.mBaseWndList;

        UIBaseWnd frontmost = null;

        foreach (UIBaseWnd wnd in wnds)
        {
            if (wnd == GameUI.Instance.mMissionTrackWnd
                || wnd == GameUI.Instance.mItemsTrackWnd
                || wnd == GameUI.Instance.mCustomMissionTrack.missionInterpreter.missionTrackWnd
                || wnd == GameUI.Instance.mRevive) //lz-2017.01.04 按esc退出的时候复活界面不关闭错误 #7975
                continue;
            if (wnd.isShow && wnd.Active)
            {
                frontmost = wnd;
                frontmost.Hide();
                return true;
            }

        }

        //		if (GameUIMode.Instance.curUIMode == GameUIMode.UIMode.um_building)
        //		{
        //			GameUI.Instance.mBuildBlock.QuitBuildMode();
        //			return true;
        //		}

        return false;
    }

    CursorHandler m_CursorHandler = new CursorHandler();

    // Update is called once per frame
    void Update()
    {
        //		MousePicker.IPickable player = null;
        //		if (Pathea.MainPlayer.Instance != null && Pathea.MainPlayer.Instance.entity != null)
        //			player =  Pathea.MainPlayer.Instance.entity.GetComponent<MousePickablePeEntity>()as MousePicker.IPickable;
        //
        //		if (CursorState.self.Handler == null  && MousePicker.Instance.curPickObj != null && player != MousePicker.Instance.curPickObj)
        //		{
        //			CursorState.self.SetHandler(m_CursorHandler);
        //		}
        //		else 
        //		{
        //			if (MousePicker.Instance.curPickObj == null)
        //				CursorState.self.ClearHandler(m_CursorHandler);
        //		}

        if (GameUI.Instance == null)
            return;

        if (null != GameUI.Instance.mMainPlayer && !GameConfig.IsInVCE && !UICamera.inputHasFocus)
        {
            if (PeInput.Get(PeInput.LogicFunction.OptionsUI))
            {
                KeyFunc_OptionUI();
            }
            else if (PeInput.Get(PeInput.LogicFunction.SaveMenuUI))
            {
                if (!GameConfig.IsMultiMode)
                    KeyFunc_SaveUI();
            }
            else if (PeInput.Get(PeInput.LogicFunction.LoadMenuUI))
            {
                if (!GameConfig.IsMultiMode)
                    KeyFunc_LoadUI();
            }
        }

        if (GameConfig.IsInVCE)
        {
            //lz-2016.10.12 改为逻辑按键
            if (PeInput.Get(PeInput.LogicFunction.CreationSystem) && !UICamera.inputHasFocus)
            {
                // VCEditor.Quit();
                if (VCEditor.Instance != null && VCEditor.Instance.m_UI != null)
                {
                    VCEditor.Instance.m_UI.OnQuitClick();
                }
            }
        }
        if (null != GameUI.Instance.mMainPlayer && !GameConfig.IsInVCE && !UISystemMenu.IsSystemOping() && !UICamera.inputHasFocus)
        {
            if (PeInput.Get(PeInput.LogicFunction.PackageUI))
                KeyFunc_ItemPackge();

            if (PeInput.Get(PeInput.LogicFunction.WorldMapUI))
            {
                GlobalEvent.NoticeMouseUnlock();
                KeyFunc_WorldMap();
            }

            if (PeInput.Get(PeInput.LogicFunction.CharacterUI))
            {
                KeyFunc_Character();
            }

            if (PeInput.Get(PeInput.LogicFunction.MissionUI))
            {
                KeyFunc_MissionUI();
            }

            if (PeInput.Get(PeInput.LogicFunction.ColonyUI))
            {
                KeyFunc_ColonyUI();
            }

            if (PeInput.Get(PeInput.LogicFunction.ReplicationUI))
            {
                KeyFunc_ReplicationUI();
            }

            if (PeInput.Get(PeInput.LogicFunction.FollowersUI))
            {
                KeyFunc_FollowerUI();
            }

            if (PeInput.Get(PeInput.LogicFunction.SkillUI))
            {
                KeyFunc_SkillUI();
            }

            if (PeInput.Get(PeInput.LogicFunction.HandheldPcUI))
            {
                KeyFunc_PhoneUI();
            }

            if (PeInput.Get(PeInput.LogicFunction.CreationSystem))
            {
                //lz-2016.10.12 射击模式不允许打开创建系统
                if (!VCEditor.s_Active && UISightingTelescope.Instance.CurType == UISightingTelescope.SightingType.Null)
                    VCEditor.Open();
            }

            if (PeInput.Get(PeInput.LogicFunction.TalkMenuUI))
            {
                KeyFunc_TalkMenuUI();
            }

            //lz-2016.06.28 快捷键打开游戏菜单
            if (PeInput.Get(PeInput.LogicFunction.GameMenuUI))
            {
                KeyFunc_GameMenuUI();
            }

            #region QuickBar
            if (enableQuickKey)
            {
                if (PeInput.Get(PeInput.LogicFunction.QuickBar1) || Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(0);
                }

                if (PeInput.Get(PeInput.LogicFunction.QuickBar2) || Input.GetKeyDown(KeyCode.Alpha2) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(1);
                }


                if (PeInput.Get(PeInput.LogicFunction.QuickBar3) || Input.GetKeyDown(KeyCode.Alpha3) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(2);
                }

                if (PeInput.Get(PeInput.LogicFunction.QuickBar4) || Input.GetKeyDown(KeyCode.Alpha4) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(3);
                }

                if (PeInput.Get(PeInput.LogicFunction.QuickBar5) || Input.GetKeyDown(KeyCode.Alpha5) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(4);
                }

                if (PeInput.Get(PeInput.LogicFunction.QuickBar6) || Input.GetKeyDown(KeyCode.Alpha6) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(5);
                }

                if (PeInput.Get(PeInput.LogicFunction.QuickBar7) || Input.GetKeyDown(KeyCode.Alpha7) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(6);
                }

                if (PeInput.Get(PeInput.LogicFunction.QuickBar8) || Input.GetKeyDown(KeyCode.Alpha8) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(7);
                }

                if (PeInput.Get(PeInput.LogicFunction.QuickBar9) || Input.GetKeyDown(KeyCode.Alpha9) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(8);
                }

                if (PeInput.Get(PeInput.LogicFunction.QuickBar10) || Input.GetKeyDown(KeyCode.Alpha0) && Input.GetKey(KeyCode.LeftShift))
                {
                    KeyFunc_QuickBar(9);
                }

                //lz-2016.08.08 增加翻页按键调用
                if (PeInput.Get(PeInput.LogicFunction.PrevQuickBar))
                {
                    KeyFunc_PrevQuickBar();
                }

                if (PeInput.Get(PeInput.LogicFunction.NextQuickBar))
                {
                    KeyFunc_NextQuickBar();
                }
            }

            #endregion
        }

    }

    void KeyFunc_WorldMap()
    {
        if (Pathea.PeGameMgr.sceneMode == Pathea.PeGameMgr.ESceneMode.Custom || PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
        {
            return;
        }

        GameUI.Instance.mUIWorldMap.ChangeWindowShowState();
        //GameUI.Instance.ShowGameWnd();
        //GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
        GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(false);
    }

    void KeyFunc_ItemPackge()
    {
        GameUI.Instance.mItemPackageCtrl.ChangeWindowShowState();
        //GameUI.Instance.ShowGameWnd();
        //GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
        GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(false);
    }

    void KeyFunc_Character()
    {
        GameUI.Instance.mUIPlayerInfoCtrl.ChangeWindowShowState();
        //GameUI.Instance.ShowGameWnd();
        //GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
        GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(false);
    }

    void KeyFunc_QuickBar(int index)
    {
        GameUI.Instance.mUIMainMidCtrl.OnKeyDown_QuickBar(index);
    }

    void KeyFunc_OptionUI()
    {
        if (GameUI.Instance.mSystemMenu.isShow)
            GameUI.Instance.mSystemMenu.ChangeWindowShowState();
        else
        {
            if (!CloseFrontWnd())
                GameUI.Instance.mSystemMenu.ChangeWindowShowState();
        }
    }
    void KeyFunc_SaveUI()
    {
        if (UISaveLoad.Instance.isShow)
        {
            UISaveLoad.Instance.Hide();
        }
        else if (!UISystemMenu.IsSystemOping())
        {
            GameUI.Instance.mSystemMenu.OnSaveBtn();
        }
    }
    void KeyFunc_LoadUI()
    {
        if (UISaveLoad.Instance.isShow)
        {
            UISaveLoad.Instance.Hide();
        }
        else if (!UISystemMenu.IsSystemOping())
        {
            GameUI.Instance.mSystemMenu.OnLoadBtn();
        }
    }

    void KeyFunc_MissionUI()
    {
        if (Pathea.PeGameMgr.IsCustom)
        {
            GameUI.Instance.mMissionGoal.ChangeWindowShowState();
            if (GameUI.Instance.mMissionGoal.isShow)
                GameUI.Instance.mCustomMissionTrack.Show();
        }
        else
            GameUI.Instance.mUIMissionWndCtrl.ChangeWindowShowState();

        //GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
        GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(false);
    }

    void KeyFunc_ColonyUI()
    {
        //lz-2016.08.03 教程, Custom 模式禁止打开基地UI
        if (!Pathea.PeGameMgr.IsTutorial && !Pathea.PeGameMgr.IsCustom && !Pathea.PeGameMgr.IsMultiCustom)
        {
            GameUI.Instance.mCSUI_MainWndCtrl.ChangeWindowShowState();
            //GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
            GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(false);
        }
    }

    void KeyFunc_ReplicationUI()
    {
        GameUI.Instance.mCompoundWndCtrl.ChangeWindowShowState();
        GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
    }

    void KeyFunc_FollowerUI()
    {
        GameUI.Instance.mServantWndCtrl.ChangeWindowShowState();
        //GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
        GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(false);
    }

    void KeyFunc_SkillUI()
    {
        if (Pathea.PeGameMgr.IsAdventure&&RandomMapConfig.useSkillTree)
        {
            GameUI.Instance.mSkillWndCtrl.ChangeWindowShowState();
            //GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
            GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(false);
        }
    }

    void KeyFunc_PhoneUI()
    {
        GameUI.Instance.mPhoneWnd.ChangeWindowShowState();
        //GameUI.Instance.mMainPlayer.SendMsg(Pathea.EMsg.UI_ShowChange, true);
        GameUI.Instance.mMainPlayer.motionEquipment.ActiveWeapon(false);
    }

    void KeyFunc_TalkMenuUI()
    {
        UITalkwithctr.Instance.ShowMenu();
    }

    //lz-2016.06.28 快捷键打开游戏菜单
    void KeyFunc_GameMenuUI()
    {
        UIGameMenuCtrl.Instance.Show();
    }

    //lz-2016.06.28 上一页
    void KeyFunc_PrevQuickBar()
    {
        if (null != UIMainMidCtrl.Instance)
            UIMainMidCtrl.Instance.OnKeyFunc_PrevQuickBar();
    }

    //lz-2016.06.28 下一页
    void KeyFunc_NextQuickBar()
    {
        if (null != UIMainMidCtrl.Instance)
            UIMainMidCtrl.Instance.OnKeyFunc_NextQuickBar();
    }
}
