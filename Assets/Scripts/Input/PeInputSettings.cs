using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using InControl;

public enum EPeInputSettingsType
{
	SettingsGeneral,
	SettingsChrCtrl,
	SettingsBuildMd,
	SettingsVehicle,
	
	Max,
}

public static partial class PeInput
{
	static readonly string s_inputConfRootName = "InputSettings";
	static readonly string s_inputConfVersion = "20161114";
	public class KeyJoySettingPair	// public for key settings ui
	{
		public KeyCode	_key;
		public InputControlType	_joy;	
		public string	_keyDesc;
		public string	_joyDesc;
		public bool		_keyLock;
		public bool		_joyLock;// if true, modding this is prohibited.
        public KeyJoySettingPair(KeyCode key0, InputControlType key1, bool lock0 = false, bool lock1 = false, string desc0 = "", string desc1 = "")
		{
			_key = key0;
			_joy = key1;
			_keyDesc = desc0;
			_joyDesc = desc1;
			_keyLock = lock0;
			_joyLock = lock1;
		}
		public KeyJoySettingPair(KeyJoySettingPair src)
		{
			_key = src._key;
			_joy = src._joy;
			_keyDesc = src._keyDesc;
			_joyDesc = src._joyDesc;
			_keyLock = src._keyLock;
			_joyLock = src._joyLock;
		}
		public void Clone(KeyJoySettingPair src)
		{
			_key = src._key;
			_joy = src._joy;
			_keyDesc = src._keyDesc;
			_joyDesc = src._joyDesc;
			_keyLock = src._keyLock;
			_joyLock = src._joyLock;
		}
		public static implicit operator KeyJoySettingPair(KeyCode key)
		{
			return new KeyJoySettingPair(key, InputControlType.None);
		}
	}
	public static KeyJoySettingPair[][] SettingsAll{get{ return s_settingsAll; }}
	public static KeyJoySettingPair[] SettingsGeneral{get{ return s_settingsGeneral;	}}
	public static KeyJoySettingPair[] SettingsChrCtrl{get{ return s_settingsChrCtrl;	}}
	public static KeyJoySettingPair[] SettingsBuildMd{get{ return s_settingsBuildMd;	}}
	public static KeyJoySettingPair[] SettingsVehicle{get{ return s_settingsVehicle;	}}
	public static int StrIdOfGeneral(int i){ return s_strIdGeneral[i];	}
	public static int StrIdOfChrCtrl(int i){ return s_strIdChrCtrl[i];	}
	public static int StrIdOfBuildMd(int i){ return s_strIdBuildMd[i];	}
	public static int StrIdOfVehicle(int i){ return s_strIdVehicle[i];	}

	public enum ESettingsGeneral
	{
		Attack,
		Shoot,
		Dig,
		Build,
		Cut,			//5
		PutOn,
		PutAway,
		Interact,
		Gather,
		Take,			//10
		ZoomInCamera,
		ZoomOutCamera,
		AutoRun,
		Interact_Talk_Cut_Gather_Take,
		WorldMap,		//15
		Mission,
		Inventory,
		CharacterStats,
		Rotate,
		ReplicationMenu,//20
		SkillMenu,
		FollowersMenu,
		HandheldPC,
		CreationSystem,
		ColonyMenu,//25
		FriendsMenu,
		Shop,
		OpenChatWindow_Send,
		GameMenu,
		EscMenu_CloseAllUI,//30
		ChangeContrlMode,
		SaveMenu,
		LoadMenu,
		ScreenCapture,
		QuickBar1,//35
		QuickBar2,
		QuickBar3,
		QuickBar4,
		QuickBar5,
		QuickBar6,//40
		QuickBar7,
		QuickBar8,
		QuickBar9,
		QuickBar10,
		PrevQuickBar,//45
		NextQuickBar,
        LiberatiePerspective,       //lz-2018.01.16 移动方向和相机方向分开
        Max,
	};
	static readonly int[] s_strIdGeneral = {
		10100,
		10289,
		10183,
		10184,
		10185,
		10287,
		10286,
		10102,
		10284,
		10285,
		10103,
		10104,
		10101,
		10114,
		10123,
		10124,
		10125,
		10126,
		10116,
		10127,
		10128,
		10129,
		10130,
		10131,
		10132,
		10133,
		10134,
		10148,
		10288,
		10150,
		10152,
		10156,
		10157,
		10158,
		10138,
		10139,
		10140,
		10141,
		10142,
		10143,
		10144,
		10145,
		10146,
		10147,
		10282,
		10283,
        10081
    };
	static KeyJoySettingPair[] s_settingsGeneral = {		// refered by s_logicInputConf, so we need prealloc each KeyJoySettingPair
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3),
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.RightTrigger),
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3),
		KeyCode.Mouse0,
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3),	//5
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3), 
		new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action2),
		new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4),
		new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4),
		new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4),					//10
		new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.None, true, true),//TBD: scroll 
		new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.None, true, true),//TBD: scroll
		new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.LeftStickButton, false, true),
		new KeyJoySettingPair(KeyCode.E, InputControlType.Action4),
		KeyCode.M,				//15
		KeyCode.Q,              
		KeyCode.I,              
		KeyCode.C,             
		KeyCode.T,
		KeyCode.Y,            //20    
		AltKey(KeyCode.T),    
		AltKey(KeyCode.F),
		KeyCode.H,              
		KeyCode.J,                 
		AltKey(KeyCode.C),  //25
		AltKey(KeyCode.Z),	
		AltKey(KeyCode.X),
		KeyCode.Return,  
		KeyCode.BackQuote,
		new KeyJoySettingPair(KeyCode.Escape, InputControlType.Start, false, true), 	//30
		KeyCode.F5,			
		KeyCode.F9,
		KeyCode.F10,
		KeyCode.F12,
		new KeyJoySettingPair(KeyCode.Alpha1, InputControlType.DPadUp, false, true),	//35
		new KeyJoySettingPair(KeyCode.Alpha2, InputControlType.DPadLeft, false, true),
		new KeyJoySettingPair(KeyCode.Alpha3, InputControlType.DPadDown, false, true),
		new KeyJoySettingPair(KeyCode.Alpha4, InputControlType.DPadRight, false, true),
		KeyCode.Alpha5,
		KeyCode.Alpha6,//40
		KeyCode.Alpha7,
		KeyCode.Alpha8,
		KeyCode.Alpha9,
		KeyCode.Alpha0,
		KeyCode.Comma,//45
		KeyCode.Period,
        KeyCode.LeftAlt
	};
	static readonly KeyJoySettingPair[] s_settingsGeneralDef = {
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3),
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.RightTrigger),
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3),
		KeyCode.Mouse0,
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3),	//5
		new KeyJoySettingPair(KeyCode.Mouse0, InputControlType.Action3), 
		new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action2),
		new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4),
		new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4),
		new KeyJoySettingPair(KeyCode.Mouse1, InputControlType.Action4),					//10
		new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.None, true, true),//TBD: scroll 
		new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.None, true, true),//TBD: scroll
		new KeyJoySettingPair(KeyCode.Mouse2, InputControlType.LeftStickButton, false, true),
		new KeyJoySettingPair(KeyCode.E, InputControlType.Action4),
		KeyCode.M,				//15
		KeyCode.Q,              
		KeyCode.I,              
		KeyCode.C,             
		KeyCode.T,
		KeyCode.Y,            //20    
		AltKey(KeyCode.T),    
		AltKey(KeyCode.F),
		KeyCode.H,              
		KeyCode.J,                 
		AltKey(KeyCode.C),  //25
		AltKey(KeyCode.Z),	
		AltKey(KeyCode.X),
		KeyCode.Return,  
		KeyCode.BackQuote,
		new KeyJoySettingPair(KeyCode.Escape, InputControlType.Start, false, true), //30
		KeyCode.F5,
		KeyCode.F9,
		KeyCode.F10,
		KeyCode.F12,
		new KeyJoySettingPair(KeyCode.Alpha1, InputControlType.DPadUp, false, true),	//35
		new KeyJoySettingPair(KeyCode.Alpha2, InputControlType.DPadLeft, false, true),
		new KeyJoySettingPair(KeyCode.Alpha3, InputControlType.DPadDown, false, true),
		new KeyJoySettingPair(KeyCode.Alpha4, InputControlType.DPadRight, false, true),
		KeyCode.Alpha5,
		KeyCode.Alpha6,
		KeyCode.Alpha7,
		KeyCode.Alpha8,
		KeyCode.Alpha9,
		KeyCode.Alpha0,		
		KeyCode.Comma,
		KeyCode.Period,
        KeyCode.LeftAlt
    };
	enum ESettingsChrCtrl
	{
		MoveForward,
		MoveBack,
		MoveLeft,
		MoveRight,
		Jump_SwimmingUp,
		Reload,
		Walk_Run,
		Sprint,
		Block,
		
		Max,
	};
	static readonly int[] s_strIdChrCtrl = {
		10105,
		10107,
		10109,
		10111,
		10113,
		10115,
		10118,
		10119,
		10120,
	};
	static KeyJoySettingPair[] s_settingsChrCtrl = {
		new KeyJoySettingPair(KeyCode.W, InputControlType.LeftStickY, false, true),
		new KeyJoySettingPair(KeyCode.S, InputControlType.LeftStickY, false, true),
		new KeyJoySettingPair(KeyCode.A, InputControlType.LeftStickX, false, true),
		new KeyJoySettingPair(KeyCode.D, InputControlType.LeftStickX, false, true),
		new KeyJoySettingPair(KeyCode.Space, InputControlType.Action1),
		new KeyJoySettingPair(KeyCode.R, InputControlType.Action3),
		KeyCode.Z,
		new KeyJoySettingPair(KeyCode.LeftShift, InputControlType.RightBumper),
		new KeyJoySettingPair(KeyCode.LeftControl, InputControlType.LeftBumper),
	};
	static readonly KeyJoySettingPair[] s_settingsChrCtrlDef = {
		new KeyJoySettingPair(KeyCode.W, InputControlType.LeftStickY, false, true),
		new KeyJoySettingPair(KeyCode.S, InputControlType.LeftStickY, false, true),
		new KeyJoySettingPair(KeyCode.A, InputControlType.LeftStickX, false, true),
		new KeyJoySettingPair(KeyCode.D, InputControlType.LeftStickX, false, true),
		new KeyJoySettingPair(KeyCode.Space, InputControlType.Action1),
		new KeyJoySettingPair(KeyCode.R, InputControlType.Action3),
		KeyCode.Z,
		new KeyJoySettingPair(KeyCode.LeftShift, InputControlType.RightBumper),
		new KeyJoySettingPair(KeyCode.LeftControl, InputControlType.LeftBumper),
	};
	public enum ESettingsBuildMd
	{
		BuildMode,
		FreeBuildingMode,
		TweakSelectionArea1,
		TweakSelectionArea2,
		TweakSelectionArea3,
		TweakSelectionArea4,
		TweakSelectionArea5,
		TweakSelectionArea6,
		RotateOnAxis,
		Undo,
		Redo,
		ChangeBrush,
		DeleteSelection,
		CrossSelection,
		SubtractSelection,
		AddSelection,
		Extrude,
		Squash,
		TopQuickBar1,
		TopQuickBar2,
		TopQuickBar3,
		TopQuickBar4,
		TopQuickBar5,
		TopQuickBar6,
		TopQuickBar7,		
		
		Max,
	};
	static readonly int[] s_strIdBuildMd = {
		10122,
		10186,
		10159,
		10160,
		10161,
		10162,
		10163,
		10164,
		10165,
		10190,
		10191,
		10192,
		10193,
		10194,
		10195,
		10196,
		10197,
		10198,
		10166,
		10167,
		10168,
		10169,
		10170,
		10171,
		10172,
	};
	static KeyJoySettingPair[] s_settingsBuildMd = {
		KeyCode.B,
		KeyCode.F,
		new KeyJoySettingPair(KeyCode.UpArrow, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.DownArrow, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.LeftArrow, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.RightArrow, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.PageUp, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.PageDown, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.G, InputControlType.None, true, true),
		new KeyJoySettingPair(CtrlKey(KeyCode.Z), InputControlType.None, true, true),
		new KeyJoySettingPair(CtrlKey(KeyCode.X), InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.Tab, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.Delete, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.LeftControl, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.LeftAlt, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.LeftShift, InputControlType.None, true, true),
		new KeyJoySettingPair(ShiftKey(KeyCode.PageUp), InputControlType.None, true, true),
		new KeyJoySettingPair(ShiftKey(KeyCode.PageDown), InputControlType.None, true, true),
		ShiftKey(KeyCode.Alpha1),
		ShiftKey(KeyCode.Alpha2),
		ShiftKey(KeyCode.Alpha3),
		ShiftKey(KeyCode.Alpha4),
		ShiftKey(KeyCode.Alpha5),
		ShiftKey(KeyCode.Alpha6),
		ShiftKey(KeyCode.Alpha7),
	};
	static readonly KeyJoySettingPair[] s_settingsBuildMdDef = {
		KeyCode.B,
		KeyCode.F,
		new KeyJoySettingPair(KeyCode.UpArrow, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.DownArrow, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.LeftArrow, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.RightArrow, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.PageUp, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.PageDown, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.G, InputControlType.None, true, true),
		new KeyJoySettingPair(CtrlKey(KeyCode.Z), InputControlType.None, true, true),
		new KeyJoySettingPair(CtrlKey(KeyCode.X), InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.Tab, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.Delete, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.LeftControl, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.LeftAlt, InputControlType.None, true, true),
		new KeyJoySettingPair(KeyCode.LeftShift, InputControlType.None, true, true),
		new KeyJoySettingPair(ShiftKey(KeyCode.PageUp), InputControlType.None, true, true),
		new KeyJoySettingPair(ShiftKey(KeyCode.PageDown), InputControlType.None, true, true),
		ShiftKey(KeyCode.Alpha1),
		ShiftKey(KeyCode.Alpha2),
		ShiftKey(KeyCode.Alpha3),
		ShiftKey(KeyCode.Alpha4),
		ShiftKey(KeyCode.Alpha5),
		ShiftKey(KeyCode.Alpha6),
		ShiftKey(KeyCode.Alpha7),
	};
	enum ESettingsVehicle
	{
		Fire1,
		Fire2,
		Brake,
		ThrottleDown,
		FireMode,
		Sprint,
		VehicleLight,
		MissileLock,
		MissileLaunch,
		VehicleWeaponGroup1Toggle,
		VehicleWeaponGroup2Toggle,
		VehicleWeaponGroup3Toggle,
		VehicleWeaponGroup4Toggle,

		Max,
	};
	static readonly int[] s_strIdVehicle = {
		10212,
		10213,
		10201,
		10202,
		10203,
		10204,
		10205,
		10206,
		10207,
		10208,
		10209,
		10210,
		10211,
	};
	static KeyJoySettingPair[] s_settingsVehicle = {
		KeyCode.Mouse0,
		KeyCode.Mouse1,
		KeyCode.Space,
		KeyCode.LeftAlt,
		KeyCode.F,
		KeyCode.LeftShift,
		KeyCode.L,
		KeyCode.Z,
		KeyCode.X,
		KeyCode.F1,
		KeyCode.F2,
		KeyCode.F3,
		KeyCode.F4,
	};
	static readonly KeyJoySettingPair[] s_settingsVehicleDef = {
		KeyCode.Mouse0,
		KeyCode.Mouse1,
		KeyCode.Space,
		KeyCode.LeftAlt,
		KeyCode.F,
		KeyCode.LeftShift,
		KeyCode.L,
		KeyCode.Z,
		KeyCode.X,
		KeyCode.F1,
		KeyCode.F2,
		KeyCode.F3,
		KeyCode.F4,
	};
	// Sum all
	static readonly KeyJoySettingPair[][] s_settingsAll = new KeyJoySettingPair[(int)EPeInputSettingsType.Max][]{
		s_settingsGeneral,
		s_settingsChrCtrl,
		s_settingsBuildMd,
		s_settingsVehicle,
	};
	static readonly KeyJoySettingPair[][] s_settingsAllDef = new KeyJoySettingPair[(int)EPeInputSettingsType.Max][]{
		s_settingsGeneralDef,
		s_settingsChrCtrlDef,
		s_settingsBuildMdDef,
		s_settingsVehicleDef,
	};
	static string GetSettingName(int type, int index)
	{
		switch (type) {
		case 0:	return ((ESettingsGeneral)index).ToString();
		case 1:	return ((ESettingsChrCtrl)index).ToString();
		case 2:	return ((ESettingsBuildMd)index).ToString();
		case 3:	return ((ESettingsVehicle)index).ToString();
		}
		return string.Empty;
	}
	static int GetSettingIndex(int type, string name)
	{
		switch (type) {
		case 0:	return (int)Enum.Parse(typeof(ESettingsGeneral), name);
		case 1:	return (int)Enum.Parse(typeof(ESettingsChrCtrl), name);
		case 2:	return (int)Enum.Parse(typeof(ESettingsBuildMd), name);
		case 3:	return (int)Enum.Parse(typeof(ESettingsVehicle), name);
		}
		return -1;
	}

	public static void ResetSetting()
	{
		for (int i = 0; i < s_settingsAll.Length; i++) {
			for(int j = 0; j < s_settingsAll[i].Length; j++){
				s_settingsAll[i][j].Clone(s_settingsAllDef[i][j]);
			}
		}
		ResetLogicInput();
	}

	public static void SaveInputConfig(string configFile, bool bApply = true)
	{
		XmlDocument xmlDoc = new XmlDocument();
		try{
			using (FileStream fs = new FileStream (configFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				xmlDoc.Load (fs);
			}
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
			xmlDoc = new XmlDocument();
		}
		
		XmlElement rootNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode(s_inputConfRootName);
		if(null == rootNode)
		{
			rootNode = xmlDoc.CreateElement(s_inputConfRootName);
			xmlDoc.DocumentElement.AppendChild(rootNode);
		}
		else
		{
			rootNode.RemoveAll();
		}
		rootNode.SetAttribute("Ver", s_inputConfVersion);

		for (int i = 0; i < s_settingsAll.Length; i++) {
			XmlElement typeNode = xmlDoc.CreateElement(((EPeInputSettingsType)i).ToString());
			rootNode.AppendChild(typeNode);
			for(int j = 0; j < s_settingsAll[i].Length; j++)
			{
				XmlElement keysetNode = xmlDoc.CreateElement(GetSettingName(i,j));
				typeNode.AppendChild(keysetNode);
				
				KeyJoySettingPair conf = s_settingsAll[i][j];
				keysetNode.SetAttribute("Key",((int)conf._key).ToString());
				keysetNode.SetAttribute("Joy",((int)conf._joy).ToString());
				keysetNode.SetAttribute("KeyLock",Convert.ToString(conf._keyLock));
				keysetNode.SetAttribute("JoyLock",Convert.ToString(conf._joyLock));
				keysetNode.SetAttribute("KeyDes",conf._keyDesc);
				keysetNode.SetAttribute("JoyDes",conf._joyDesc);
			}
		}
		try{
			using (FileStream fs = new FileStream (configFile, FileMode.Create, FileAccess.Write, FileShare.None)) {
				xmlDoc.Save (fs);
			}		
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
		}

		if(bApply)	ResetLogicInput();
	}
	public static void LoadInputConfig(string configFile)
	{
		XmlDocument xmlDoc = new XmlDocument();
		try{
			using (FileStream fs = new FileStream (configFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
				xmlDoc.Load (fs);
			}		
		}catch(Exception e){
			GameLog.HandleIOException(e, GameLog.EIOFileType.Settings);
			xmlDoc = new XmlDocument();
		}
		
		XmlElement rootNode = (XmlElement)xmlDoc.DocumentElement.SelectSingleNode(s_inputConfRootName);
		if(null == rootNode || !rootNode.HasAttribute("Ver") || rootNode.GetAttribute("Ver") != s_inputConfVersion)
		{
			SaveInputConfig(configFile, false);
			return;
		}

		bool bError = false;
		foreach(XmlNode typeNode in rootNode.ChildNodes)
		{
			try{
				int type = (int)Enum.Parse(typeof(EPeInputSettingsType), typeNode.Name);
				KeyJoySettingPair[] curSettings = s_settingsAll[type];
				foreach(XmlNode pairNode in typeNode.ChildNodes)
				{
					try{
						XmlElement pairElem = (XmlElement)pairNode;
						KeyCode key = (KeyCode)Convert.ToInt32(pairElem.GetAttribute("Key")); 
						InputControlType joy = (InputControlType)Convert.ToInt32(pairElem.GetAttribute("Joy"));
						bool keyLock = Convert.ToBoolean(pairElem.GetAttribute("KeyLock"));
						bool joyLock = Convert.ToBoolean(pairElem.GetAttribute("JoyLock"));
						string keyDesc = pairElem.GetAttribute("KeyDes");
						string joyDesc = pairElem.GetAttribute("JoyDes");
						int idx = GetSettingIndex(type, pairNode.Name);
						curSettings[idx].Clone(new KeyJoySettingPair(key, joy, keyLock, joyLock, keyDesc, joyDesc));
					}catch{
						bError = true;
						Debug.LogError("[PeInput]Error occured while reading xmlnode:"+typeNode.Name+"-"+pairNode.Name);
					}
				}
			}catch{
				bError = true;
				Debug.LogError("[PeInput]Error occured while reading settings type:"+typeNode.Name);
			}
		}

        //lz-2017.08.08 把玩家旧的不正确的翻滚配置修改正确
        var settingsChrCtrl = s_settingsAll[(int)EPeInputSettingsType.SettingsChrCtrl];
        if (settingsChrCtrl[0]._joy == InputControlType.None) settingsChrCtrl[0]._joy = InputControlType.LeftStickY;
        if (settingsChrCtrl[1]._joy == InputControlType.None) settingsChrCtrl[1]._joy = InputControlType.LeftStickY;
        if (settingsChrCtrl[2]._joy == InputControlType.None) settingsChrCtrl[2]._joy = InputControlType.LeftStickX;
        if (settingsChrCtrl[3]._joy == InputControlType.None) settingsChrCtrl[3]._joy = InputControlType.LeftStickX;

        if (bError)
        {
            SaveInputConfig(configFile, true);
            return;
        }
		ResetLogicInput();
	}
}
