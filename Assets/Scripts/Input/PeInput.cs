//#define NO_JOY	// code and input setting
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public static partial class PeInput
{
    public enum LogicFunction
    {
        DrawWeapon,
        Attack,
        BegDigging,
        EndDigging,
        BegShooting,        //5	
        EndShooting,
        BegLaserWarming,
        EndLaserWarming,
        Build,
        Item_Drag,          //10	
        Item_Drop,
        UI_SkipDialog,
        TalkToNpc,
        OpenItemMenu,
        GatherHerb,     //15
        DrawWater,
        PickBody,
        SheatheWeapon,
        Cut,
        RotateCam,          //20
        UI_RMouseOp,
        Item_Use,
        Item_Equip,
        Item_CancelDrag,
        PushForwardCam,         //25
        PullBackwardCam,
        AutoRunOnOff,
        TakeForwardVehicleOnOff,
        ClimbForwardLadderOnOff,
        InteractWithItem,           //30	
        Item_RotateItem,
        Item_RotateItemPress,
        WorldMapUI,
        MissionUI,
        PackageUI,              //35
        CharacterUI,
        ReplicationUI,
        SkillUI,
        FollowersUI,
        HandheldPcUI,           //40
        CreationSystem,
        ColonyUI,
        FriendsUI,
        ShopUI,
        EnterChat,              //45
        SendChat,
        UI_SkipDialog2,
        UI_Confirm,
        TalkMenuUI,
        GameMenuUI,             //50
        OptionsUI,
        UI_CloseUI,
        UI_Cancel,
        ChangeContrlMode,
        SaveMenuUI,             //55
        LoadMenuUI,
        ScreenCapture,
        QuickBar1,
        QuickBar2,
        QuickBar3,              //60
        QuickBar4,
        QuickBar5,
        QuickBar6,
        QuickBar7,
        QuickBar8,              //65
        QuickBar9,
        QuickBar10,
        PrevQuickBar,
        NextQuickBar,
        LiberatiePerspective,
        //UIClickButton,
        //UIDrag,

        MoveForward,        //70
        DodgeForward,
        MoveBackward,
        DodgeBackward,
        MoveLeft,
        DodgeLeft,          //75
        MoveRight,
        DodgeRight,
        Jump,
        SwimUp,
        Jet,                //80
        Reload,
        SwitchWalkRun,
        Sprint,
        Block,
        UI_SkipDialog1,     //85

        BuildMode,
        Build_FreeBuildModeOnOff,
        Build_TweakSelectionArea_Up,
        Build_TweakSelectionArea_Dn,
        Build_TweakSelectionArea_Lt,    //90
        Build_TweakSelectionArea_Rt,
        Build_TweakSelectionArea_PgUp,
        Build_TweakSelectionArea_PgDn,
        Build_RotateOnAxis,
        Build_Shortcut1,                //95
        Build_Shortcut2,
        Build_Shortcut3,
        Build_Shortcut4,
        Build_Shortcut5,
        Build_Shortcut6,                //100
        Build_Shortcut7,
        Build_Undo,
        Build_Redo,
        Build_ChangeBrush,
        Build_DeleteSelection,          //105
        Build_CrossSelection,
        Build_SubtractSelection,
        Build_AddSelection,
        Build_Extrude,
        Build_Squash,                   //110

        Vehicle_BegUnfixedShooting,
        Vehicle_EndUnfixedShooting,
        Vehicle_BegFixedShooting,
        Vehicle_EndFixedShooting,
        Vehicle_Brake,                  //115
        Vehicle_LiftUp,
        Vehicle_LiftDown,
        Vehicle_AttackModeOnOff,
        Vehicle_Sprint,
        SwitchLight,                    //120
        MissleTarget,
        MissleLaunch,
        VehicleWeaponGrp1,
        VehicleWeaponGrp2,
        VehicleWeaponGrp3,              //125
        VehicleWeaponGrp4,
    };
    private class KeyExcluders
    {
        public KeyCode _key = KeyCode.None;
        public List<LogicInput> _excluders = new List<LogicInput>();
        public KeyExcluders(KeyCode key) {
            _key = key;
        }
    };
    private static bool MouseExcluder()
    {
        return UIMouseEvent.opAnyGUI;
    }
    private static bool UIDlgExcluder()
    {
        return GameUI.Instance != null && GameUI.Instance.mNPCTalk != null && GameUI.Instance.mNPCTalk.isShow && GameUI.Instance.mNPCTalk.IsCanSkip();
    }
    private static bool KeyMaskExcluderShift() { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); }
    private static bool KeyMaskExcluderAlt() { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); }
    private static bool KeyMaskExcluderCtrl() { return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl); }
    private static List<KeyExcluders> s_inputExcluders = new List<KeyExcluders>();
    private static LogicInput s_curLogicInput = null;

    private class LogicInput
    {
        public KeyJoySettingPair KeyJoy;
        private KeyPressType _keyPressType;
        private KeyPressType _joyPressType;
        private float[] _keyPara = null;
        private float[] _joyPara;
        public Func<bool> Excluder; // If this is true, exclude other input with the same keycode
        public Func<bool> PressTst; // the first bool is the input parameter
        public Func<bool> JoyTst;
        public LogicInput Alternate = null;
        public LogicInput(KeyJoySettingPair keyJoy,
                          Func<bool> excluder = null,
                          KeyPressType keyPressType = KeyPressType.Down, float[] keyPara = null,
                          KeyPressType joyPressType = KeyPressType.JoyDown, float[] joyPara = null, LogicInput alternate = null)
        {
            _keyPressType = keyPressType;
            _joyPressType = joyPressType;
            KeyJoy = keyJoy;
            Excluder = excluder;
            Alternate = alternate;
        }
        public void ResetPressTstFunc()
        {
            //Clear excluders adding procedually
            Excluder -= KeyMaskExcluderShift;
            Excluder -= KeyMaskExcluderCtrl;
            Excluder -= KeyMaskExcluderAlt;

            PressTst = CreatePressTestFunc(KeyJoy._key, _keyPressType, _keyPara);
#if !NO_JOY
            if (KeyJoy._joy != InControl.InputControlType.None || _joyPressType >= KeyPressType.JoyStickBegin) {
				JoyTst = CreatePressTestFuncJoy(KeyJoy._joy, _joyPressType, _keyPara);
            }
#endif

            // Record filter
            KeyCode key = KeyJoy._key;
            int mask = (int)KeyJoy._key & KeyMask;
            if (mask != 0) {
                if ((mask & ShiftMask) != 0) Excluder += KeyMaskExcluderShift;
                if ((mask & CtrlMask) != 0) Excluder += KeyMaskExcluderCtrl;
                if ((mask & AltMask) != 0) Excluder += KeyMaskExcluderAlt;
                key = (KeyCode)((int)KeyJoy._key - mask);
            }
            if (Excluder != null)
            {
                int idx = s_inputExcluders.FindIndex(x => x._key == key);
                if (idx < 0) {
                    s_inputExcluders.Add(new KeyExcluders(key));
                    idx = s_inputExcluders.Count - 1;
                }
                s_inputExcluders[idx]._excluders.Add(this);
            }

            if (Alternate != null) {
                Alternate.ResetPressTstFunc();
            }
            return;
        }
    }
    static List<LogicInput> s_logicInputConf = new List<LogicInput>()	// ordered by enum LogicFunction
	{
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.PutOn],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Attack],null,KeyPressType.Down),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Attack],null,KeyPressType.Down),
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Attack],null,KeyPressType.UpHPrior, null, KeyPressType.JoyUp),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Shoot],null,KeyPressType.Down, null, KeyPressType.JoyDown),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Shoot],null,KeyPressType.UpHPrior, null, KeyPressType.JoyUp),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Attack],null,KeyPressType.Down),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Attack],null,KeyPressType.UpHPrior),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Build],null,KeyPressType.Click),	//Build
		new LogicInput(KeyCode.Mouse0,null,KeyPressType.PressHPrior),							//Item_Drag  //10
		new LogicInput(KeyCode.Mouse0,null,KeyPressType.Up),									//Item_Drop
		new LogicInput(KeyCode.Mouse0,MouseExcluder,KeyPressType.Click),						//UI_SkipDialog
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact],null,KeyPressType.Click,null,KeyPressType.JoyDown, null, new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact_Talk_Cut_Gather_Take],null,KeyPressType.Click)),//Talk
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact],null,KeyPressType.ClickNoMove, null, KeyPressType.JoyDown),	//OpenItemMenu
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Gather],null,KeyPressType.Click,null,KeyPressType.JoyDown,null, new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact_Talk_Cut_Gather_Take],null,KeyPressType.Click)),//Gather
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Dig],null,KeyPressType.Press),				// Draw water, change from rBtn to lBtn base on yinben's request 20160523
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Take],null,KeyPressType.Click,null,KeyPressType.JoyDown,null, new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact_Talk_Cut_Gather_Take],null,KeyPressType.Click)),//PickBody
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.PutAway],null,KeyPressType.ClickNoMove, null, KeyPressType.JoyDown),	//SheatheWeapon
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Cut],null,KeyPressType.Press,null,KeyPressType.Press,null, new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact_Talk_Cut_Gather_Take],null,KeyPressType.Press)),//Cut
		new LogicInput(KeyCode.Mouse1,null,KeyPressType.LongPress),								//RotateCam use LongPress to avoid conflict with sheathWeapon //20
		new LogicInput(KeyCode.Mouse1,null,KeyPressType.Click),									//UI_RMouseOp //not used
		new LogicInput(KeyCode.Mouse1,null,KeyPressType.Click),									//Item_Use
		new LogicInput(KeyCode.Mouse1,null,KeyPressType.ClickNoMove),							//Item_Equip
		new LogicInput(KeyCode.Mouse1,MouseExcluder,KeyPressType.Click),						//Item_CancelDrag	//this should check opAnyUI
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.ZoomInCamera],null,KeyPressType.Click),	//25
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.ZoomOutCamera],null,KeyPressType.Click),
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.AutoRun],null,KeyPressType.Click,null,KeyPressType.JoyDown),
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact_Talk_Cut_Gather_Take],null,KeyPressType.Click,null,KeyPressType.JoyDown),//TakeForwardVehicleOnOff,
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact_Talk_Cut_Gather_Take],null,KeyPressType.Click,null,KeyPressType.JoyDown),//ClimbForwardLadderOnOff,
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Interact_Talk_Cut_Gather_Take],null,KeyPressType.Click,null,KeyPressType.JoyDown),//InteractWithItem,			//30
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Rotate],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Rotate],null,KeyPressType.Press),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.WorldMap],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Mission],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Inventory],null,KeyPressType.Click),			//35
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.CharacterStats],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.ReplicationMenu],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.SkillMenu],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.FollowersMenu],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.HandheldPC],null,KeyPressType.Click),		//40
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.CreationSystem],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.ColonyMenu],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.FriendsMenu],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.Shop],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.OpenChatWindow_Send],null,KeyPressType.Click),	//45
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.OpenChatWindow_Send],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.OpenChatWindow_Send],UIDlgExcluder,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.OpenChatWindow_Send],UIDlgExcluder,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.OpenChatWindow_Send],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.GameMenu],null,KeyPressType.Click),				//50
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.EscMenu_CloseAllUI],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.EscMenu_CloseAllUI],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.EscMenu_CloseAllUI],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.ChangeContrlMode],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.SaveMenu],null,KeyPressType.Click),				//55
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.LoadMenu],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.ScreenCapture],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar1],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar2],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar3],null,KeyPressType.Click),				//60
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar4],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar5],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar6],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar7],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar8],null,KeyPressType.Click),				//65
		new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar9],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.QuickBar10],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.PrevQuickBar],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.NextQuickBar],null,KeyPressType.Click),
        new LogicInput(s_settingsGeneral[(int)ESettingsGeneral.LiberatiePerspective],null,KeyPressType.Press),


        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.MoveForward],null,KeyPressType.DirU),			//70
		new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.MoveForward],null,KeyPressType.DoublePress, null, KeyPressType.JoyStickUpDoublePress),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.MoveBack],null,KeyPressType.DirD),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.MoveBack],null,KeyPressType.DoublePress, null, KeyPressType.JoyStickDownDoublePress),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.MoveLeft],null,KeyPressType.DirL),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.MoveLeft],null,KeyPressType.DoublePress, null, KeyPressType.JoyStickLeftDoublePress),		//75
		new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.MoveRight],null,KeyPressType.DirR),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.MoveRight],null,KeyPressType.DoublePress, null, KeyPressType.JoyStickRightDoublePress),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.Jump_SwimmingUp],null,KeyPressType.Down),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.Jump_SwimmingUp],null,KeyPressType.Press,null,KeyPressType.JoyPress),
		new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.Jump_SwimmingUp],null,KeyPressType.Press,null,KeyPressType.JoyPress),		//80
		new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.Reload],null,KeyPressType.Click),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.Walk_Run],null,KeyPressType.Click),
		new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.Sprint],null,KeyPressType.Press,null,KeyPressType.JoyPress),
		new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.Block],null,KeyPressType.Press,null,KeyPressType.JoyPress),
        new LogicInput(s_settingsChrCtrl[(int)ESettingsChrCtrl.Jump_SwimmingUp],UIDlgExcluder,KeyPressType.Click),		//85

		new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.BuildMode],          null,KeyPressType.Click),///CD, new float[]{1}),
		new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.FreeBuildingMode],   null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TweakSelectionArea1],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TweakSelectionArea2],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TweakSelectionArea3],null,KeyPressType.Click),			//90
		new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TweakSelectionArea4],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TweakSelectionArea5],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TweakSelectionArea6],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.RotateOnAxis],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TopQuickBar1],null,KeyPressType.Click),					//95
		new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TopQuickBar2],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TopQuickBar3],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TopQuickBar4],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TopQuickBar5],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TopQuickBar6],null,KeyPressType.Click),					//100
		new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.TopQuickBar7],null,KeyPressType.Click),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.Undo],null,KeyPressType.Press),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.Redo],null,KeyPressType.Press),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.ChangeBrush],null,KeyPressType.Press),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.DeleteSelection],null,KeyPressType.Press),				//105
		new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.CrossSelection],null,KeyPressType.Press),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.SubtractSelection],null,KeyPressType.Press),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.AddSelection],null,KeyPressType.Press),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.Extrude],null,KeyPressType.Press),
        new LogicInput(s_settingsBuildMd[(int)ESettingsBuildMd.Squash],null,KeyPressType.Press),						//110

		new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.Fire1],null,KeyPressType.Down),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.Fire1],null,KeyPressType.Up),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.Fire2],null,KeyPressType.Down),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.Fire2],null,KeyPressType.Up),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.Brake],null,KeyPressType.Press),							//115
		new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.Brake],null,KeyPressType.Press),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.ThrottleDown],null,KeyPressType.Press),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.FireMode],null,KeyPressType.Click),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.Sprint],null,KeyPressType.Press),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.VehicleLight],null,KeyPressType.Click),					//120
		new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.MissileLock],null,KeyPressType.Click),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.MissileLaunch],null,KeyPressType.Click),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.VehicleWeaponGroup1Toggle],null,KeyPressType.Click),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.VehicleWeaponGroup2Toggle],null,KeyPressType.Click),
        new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.VehicleWeaponGroup3Toggle],null,KeyPressType.Click),		//125
		new LogicInput(s_settingsVehicle[(int)ESettingsVehicle.VehicleWeaponGroup4Toggle],null,KeyPressType.Click),
    };
    static void ResetLogicInput()
    {
        s_inputExcluders.Clear();
        ResetPressInfo();
        // Update logical functions
        int n = s_logicInputConf.Count;
        for (int i = 0; i < n; i++) {
            s_logicInputConf[i].ResetPressTstFunc();
        }
    }
    static PeInput()
    {
        Input.ResetInputAxes();
        ResetSetting();
        CameraForge.InputModule.SetAxis("Mouse ScrollWheel", GetMouseWheel);
    }
    public static bool enable = true;
    private static bool InputEnable()
    {
        if (!enable) return false;
        if (PlotLensAnimation.IsPlaying) return false;
        if (PeCustom.PlayAnimAction.playerAniming) return false;
        if (StroyManager.isPausing) return false;
        if (GameConfig.IsInVCE) return false;
        if (UICamera.inputHasFocus) return false;
        return true;
    }
    private static bool NotExcluderByOther()
    {
        // exclude other logic key with the same key setting
        int n = s_inputExcluders.Count;
        for (int idx = 0; idx < n; idx++) {
            if (s_inputExcluders[idx]._key == s_curLogicInput.KeyJoy._key) {
                List<LogicInput> inputExcluders = s_inputExcluders[idx]._excluders;
                n = inputExcluders.Count;
                for (int i = 0; i < n; i++) {
                    if (inputExcluders[i] != s_curLogicInput && inputExcluders[i].Excluder()) {
                        return false;
                    }
                }
                break;
            }
        }
        return true;
    }
    private static bool CheckLogicInput(LogicInput conf)
    {
        if (conf.Excluder != null && !conf.Excluder()) {
            return false;
        }
        s_curLogicInput = conf;

        bool keyBoardInput = conf.PressTst();
        bool joyStickInput = SystemSettingData.Instance.UseController && (null != conf.JoyTst ? conf.JoyTst() : false);
        if (keyBoardInput) PeInput.UsingJoyStick = false;
        if (joyStickInput) PeInput.UsingJoyStick = true;
        return keyBoardInput || joyStickInput;
    }
    public static bool Get(LogicFunction key)
    {
        if (!InputEnable()) {
            return false;
        }
        /* Tst Code
                if (key == LogicFunction.Item_CancelDrag ) {
                    if(Input.GetKeyDown(KeyCode.Mouse1)){
                        int aaa = 0;
                    } else if(Input.GetKeyUp(KeyCode.Mouse1)){
                        int aaa = 0;
                    }
                }
        */
        LogicInput conf = s_logicInputConf[(int)key];
        while (conf != null) {
            if (CheckLogicInput(conf)) {
                return true;
            }
            conf = conf.Alternate;
        }
        return false;
    }
    public static float GetAxisH()
    {
        if (!InputEnable()) return 0;
        return s_curAxisH;
    }
    public static float GetAxisV()
    {
        if (!InputEnable()) return 0;
        return s_curAxisV;
    }
    public static object GetMouseWheel()
    {
        if (!InputEnable()) return 0;
        if (MouseExcluder()) return 0;
        return Input.GetAxis("Mouse ScrollWheel");
    }
    public static void Update()
    {
        if (s_arrowAxisEnable) UpdateAxisWithArrowKey();
        else UpdateAxisWithoutArrowKey();
    }

    public static bool UsingJoyStick;

    /// <summary>
    /// lz-2016.09.19 通过逻辑功能键获得当前设置的按键
    /// </summary>
    /// <param name="funKey">逻辑功能键</param>
    /// <returns></returns>
    public static KeyCode GetKeyCodeByLogicFunKey(LogicFunction funKey)
    {
        int index = (int)funKey;
        if (index < 0 || index >= s_logicInputConf.Count) return KeyCode.None;

        LogicInput input=s_logicInputConf[index];
        if (null == input || null == input.KeyJoy) return KeyCode.None;
        return input.KeyJoy._key;
    }
}
