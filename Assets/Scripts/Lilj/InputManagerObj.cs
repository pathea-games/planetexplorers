//using System.Collections.Generic;
//using UnityEngine;
//
//public enum KeyFunction
//{
//	Attack_Build_Dig_Fire1,
//	Talk_Cancel_Fire2,
//	AutoRun,
//	MoveForward,
//	MoveLeft,
//	MoveBack,
//	MoveRight,
//	Interaction_Use_Collect_Talk,
//	EnterExitAiming_FreeBuildingMode,
//	TakeAllItems_Rotate,
//	Takeout_PutAwayWeapon,
//	Walk_Run,
//	Block,
//	HandheldPC_Shop,
//	Jump_SwimmingUp_ThrottleUp,
//	MissionList,
//	CharacterStats,
//	Climb,
//	BuildMode,
//	ColonyBase,
//	ReplicationMenu,
//	ItemsList,
//	WorldMap,
//	CreationSystem,
//	Brake,
//	Vehicle_Light,
//	Vehicle_N2O,
//	Options_CloseallUI,
//	QuickBar1,
//	QuickBar2,
//	QuickBar3,
//	QuickBar4,
//	QuickBar5,
//	QuickBar6,
//	QuickBar7,
//	QuickBar8,
//	QuickBar9,
//	QuickBar0,
//	ContrlMode,
//	FirstPersonMode,
//	SaveMenu,
//	LoadMenu,
//	ScreenCapture,
//	DodgeLeft,
//	DodgeRight,
//	UndoAction,
//	RedoAction,
//	Null
//}
//
//public class KeySetting
//{
//	public KeyFunction 	mFunction;
//	public KeyCode		mKey;
//	public KeyCode		mCtrl;
//	public string		mKeyDes;
//	public string		mCtrlDes;
//	public bool			mLockKey;
//	public bool			mLockCtrl;
//	public KeySetting()
//	{
//		
//	}
//	
//	public KeySetting(KeySetting other)
//	{
//		mFunction = other.mFunction;
//		mKey = other.mKey;
//		mCtrl = other.mCtrl;
//		mKeyDes = other.mKeyDes;
//		mCtrlDes = other.mCtrlDes;
//		mLockKey = other.mLockKey;
//		mLockCtrl = other.mLockCtrl;
//	}
//	
//	public KeySetting(KeyFunction func, KeyCode key, KeyCode ctrl, string keyDes = "", string ctrlDes = "", bool lockKey = false, bool lockCtrl = false)
//	{
//		mFunction = func;
//		mKey = key;
//		mCtrl = ctrl;
//		mKeyDes = keyDes;
//		mCtrlDes = ctrlDes;
//		mLockKey = lockKey;
//		mLockCtrl = lockCtrl;
//	}
//}
//
//public class CombineKeySetting
//{
//	public KeyFunction 	mFunction;
//	public KeyCode		mKey1;
//	public KeyCode		mKey2;
//	public CombineKeySetting(KeyFunction func, KeyCode key1, KeyCode key2)
//	{
//		mFunction = func;
//		mKey1 = key1;
//		mKey2 = key2;
//	}
//}
//
//public class InputManager:Pathea.MonoLikeSingleton<InputManager>
//{
//    protected override void OnInit()
//    {
//        base.OnInit();
//        Init();
//    }
//
//	const float MousemoveThreshold = 10f;
//	public static bool mMouseMove;
//	public static bool mLMouseClick;
//	public static bool mRMouseClick;
//	public static bool mLMousePress;
//	public static bool mRMousePress;
//	public static bool mMouseOnUI;
//
//	public static bool RotItemP;
//	public static bool RotItemC;
//	public static bool mOnFuncBtn;
//	public static bool UseEquipment;
//
//	public static bool JetPackBoost;
//
//	public static float MoveVertical;
//	public static float MoveHorizontal;
//
//	public static bool Attack { get { return Input.GetMouseButton(0); } }
//	
//	Vector3 mOldMousePos = Vector3.zero;
//	
//	public static KeyFunction GetKeyFunc(string name)
//	{
//		for(KeyFunction keyf = KeyFunction.Attack_Build_Dig_Fire1;keyf <= KeyFunction.RedoAction; keyf++)
//			if(keyf.ToString() == name)
//				return keyf;
//		
//		return KeyFunction.Null;
//	}
//
//    //public static bool GetKeyState(string funcName)
//    //{
//    //    switch(funcName)
//    //    {
//    //    case "Attack":
//    //        return Attack;
//    //        break;
//    //    }
//    //    return false;
//    //}
//	
//	public static readonly KeySetting[] mDefaulyKeySetting = 
//	{
//		new KeySetting(KeyFunction.Attack_Build_Dig_Fire1	,KeyCode.Mouse0	,KeyCode.JoystickButton0,"","",true),
//		new KeySetting(KeyFunction.Talk_Cancel_Fire2 ,KeyCode.Mouse1	,KeyCode.JoystickButton1,"","",true),
//		new KeySetting(KeyFunction.AutoRun			,KeyCode.Mouse2	,KeyCode.JoystickButton8,"","",true),
//		new KeySetting(KeyFunction.MoveForward		,KeyCode.W		,KeyCode.None	,"","LStickUp",false,true),
//		new KeySetting(KeyFunction.MoveLeft			,KeyCode.A		,KeyCode.JoystickButton4	,"","Button5",false,true),
//		new KeySetting(KeyFunction.MoveBack			,KeyCode.S		,KeyCode.None	,"","LStickDown",false,true),
//		new KeySetting(KeyFunction.MoveRight		,KeyCode.D		,KeyCode.JoystickButton5	,"","Button6",false,true),
//		new KeySetting(KeyFunction.Interaction_Use_Collect_Talk		,KeyCode.E		,KeyCode.None,"","LStickRight",false,true),
//		new KeySetting(KeyFunction.EnterExitAiming_FreeBuildingMode		,KeyCode.F		,KeyCode.None),
//		new KeySetting(KeyFunction.TakeAllItems_Rotate		,KeyCode.T		,KeyCode.None),
//		new KeySetting(KeyFunction.Takeout_PutAwayWeapon,KeyCode.Z	,KeyCode.None),
//		new KeySetting(KeyFunction.Walk_Run			,KeyCode.R,KeyCode.JoystickButton6),
//		new KeySetting(KeyFunction.Block			,KeyCode.LeftControl,KeyCode.None),
//		new KeySetting(KeyFunction.Jump_SwimmingUp_ThrottleUp	,KeyCode.Space	,KeyCode.JoystickButton2),
//		new KeySetting(KeyFunction.MissionList		,KeyCode.Q		,KeyCode.None),
//		new KeySetting(KeyFunction.CharacterStats	,KeyCode.V		,KeyCode.None),
//		new KeySetting(KeyFunction.BuildMode		,KeyCode.B		,KeyCode.None),
//		new KeySetting(KeyFunction.Climb			,KeyCode.G		,KeyCode.None),
//		new KeySetting(KeyFunction.ReplicationMenu	,KeyCode.Y		,KeyCode.None),
//		new KeySetting(KeyFunction.ItemsList		,KeyCode.I		,KeyCode.None),
//		new KeySetting(KeyFunction.WorldMap			,KeyCode.M		,KeyCode.None),
//		new KeySetting(KeyFunction.CreationSystem	,KeyCode.J		,KeyCode.None),
//		new KeySetting(KeyFunction.Brake			,KeyCode.X		,KeyCode.None),
//		new KeySetting(KeyFunction.HandheldPC_Shop	,KeyCode.H		,KeyCode.None),
////		new KeySetting(KeyFunction.Entering_ExitingAiming,KeyCode.F,KeyCode.None),
//		new KeySetting(KeyFunction.Vehicle_Light,    KeyCode.L,KeyCode.None),
//		new KeySetting(KeyFunction.Vehicle_N2O,      KeyCode.Space, KeyCode.None),
//		new KeySetting(KeyFunction.Options_CloseallUI,KeyCode.Escape,KeyCode.None,"ESC","",true),
//		new KeySetting(KeyFunction.QuickBar1		,KeyCode.Alpha1	,KeyCode.JoystickButton6),
//		new KeySetting(KeyFunction.QuickBar2		,KeyCode.Alpha2	,KeyCode.JoystickButton10),
//		new KeySetting(KeyFunction.QuickBar3		,KeyCode.Alpha3	,KeyCode.None),
//		new KeySetting(KeyFunction.QuickBar4		,KeyCode.Alpha4	,KeyCode.None),
//		new KeySetting(KeyFunction.QuickBar5		,KeyCode.Alpha5	,KeyCode.None),
//		new KeySetting(KeyFunction.QuickBar6		,KeyCode.Alpha6	,KeyCode.None),
//		new KeySetting(KeyFunction.QuickBar7		,KeyCode.Alpha7	,KeyCode.None),
//		new KeySetting(KeyFunction.QuickBar8		,KeyCode.Alpha8	,KeyCode.None),
//		new KeySetting(KeyFunction.QuickBar9		,KeyCode.Alpha9	,KeyCode.None),
//		new KeySetting(KeyFunction.QuickBar0		,KeyCode.Alpha0	,KeyCode.None),
//		new KeySetting(KeyFunction.ContrlMode		,KeyCode.F2		,KeyCode.None),
//		new KeySetting(KeyFunction.FirstPersonMode	,KeyCode.F3		,KeyCode.None),
////		new KeySetting(KeyFunction.QuickSave		,KeyCode.F5		,KeyCode.None),
////		new KeySetting(KeyFunction.QuickLoad		,KeyCode.F6		,KeyCode.None),
//		new KeySetting(KeyFunction.SaveMenu			,KeyCode.F7		,KeyCode.None),
//		new KeySetting(KeyFunction.LoadMenu			,KeyCode.F8		,KeyCode.None),
//		new KeySetting(KeyFunction.ScreenCapture	,KeyCode.F12	,KeyCode.None)
//	};
//	
//	readonly CombineKeySetting[] mCombineKeySetting = 
//	{
//		new CombineKeySetting(KeyFunction.UndoAction,KeyCode.LeftShift,KeyCode.Z),
//		new CombineKeySetting(KeyFunction.RedoAction,KeyCode.LeftShift,KeyCode.X),
//		new CombineKeySetting(KeyFunction.ColonyBase,KeyCode.LeftAlt,KeyCode.B)
//	};
//	
//	Dictionary<KeyFunction, KeySetting> mKeyList;
//	Dictionary<KeyFunction, CombineKeySetting> mCombineKeyList;
//	
//	public Dictionary<KeyFunction, KeySetting> _KeyList{get{return mKeyList;}}
//	
//	void Init()
//	{
//		mKeyList 		= new Dictionary<KeyFunction, KeySetting>();
//		mCombineKeyList = new Dictionary<KeyFunction, CombineKeySetting>();
//		
//		SetDefaultKeySetting();
//		
//		foreach(CombineKeySetting keyset in mCombineKeySetting)
//			mCombineKeyList[keyset.mFunction] = keyset;
//	}
//	
//	public void SetDefaultKeySetting()
//	{
//		mKeyList.Clear();
//		foreach(KeySetting keyset in mDefaulyKeySetting)
//			mKeyList[keyset.mFunction] = keyset;
//	}
//	
////	public bool IsKeyConflict(KeyCode key)
////	{
////		foreach(KeySetting keyset in mKeyList.Values)
////			if(keyset.mKey == key || keyset.mCtrl == key)
////				return true;
////		return false;
////	}
//	
//	public bool IsKeyDown(KeyFunction func, bool ignoreUI = true)
//	{
//		if(!ignoreUI && null != UICamera.hoveredObject)
//			return false;
//			
//		if(UICamera.inputHasFocus || GameConfig.IsInVCE)
//			return false;
//		
//		if(MessageBox_N.IsShowing)
//			return false;
//		
//		if(mCombineKeyList.ContainsKey(func))
//		{
//				return Input.GetKey(mCombineKeyList[func].mKey1) && Input.GetKeyDown(mCombineKeyList[func].mKey2);
//		}
//		
//		foreach(CombineKeySetting cks in mCombineKeyList.Values)
//		{
//			if(mKeyList.ContainsKey(func) && cks.mKey2 == mKeyList[func].mKey && Input.GetKey(cks.mKey1))
//				return false;
//		}
//		
//		if(mKeyList.ContainsKey(func))
//		{
//				return Input.GetKeyDown(mKeyList[func].mKey) || (mKeyList[func].mCtrl != KeyCode.None && Input.GetKeyDown(mKeyList[func].mCtrl));
//		}
//
//		return false;
//	}
//	
//	public bool IsKeyPressed(KeyFunction func, bool ignoreUI = true)
//	{
//		if(!ignoreUI && null != UICamera.hoveredObject)
//			return false;
//		if(UICamera.inputHasFocus || GameConfig.IsInVCE)
//			return false;
//		if(MessageBox_N.IsShowing)
//			return false;
//		
//		if(mCombineKeyList.ContainsKey(func))
//		{
//			if(Input.GetKey(mCombineKeyList[func].mKey1) && Input.GetKey(mCombineKeyList[func].mKey2))
//				return true;
//			else
//				return false;
//		}
//		
//		if(mKeyList.ContainsKey(func))
//		{
//			return Input.GetKey(mKeyList[func].mKey) || (mKeyList[func].mCtrl != KeyCode.None && Input.GetKey(mKeyList[func].mCtrl));
//		}
//
//		return false;
//	}
//	
//	public bool IsKeyUp(KeyFunction func, bool ignoreUI = true)
//	{
//		if(!ignoreUI && null != UICamera.hoveredObject)
//			return false;
//		if(UICamera.inputHasFocus || GameConfig.IsInVCE)
//			return false;
//		if(MessageBox_N.IsShowing)
//			return false;
//		
//		if(mCombineKeyList.ContainsKey(func))
//		{
//			return Input.GetKey(mCombineKeyList[func].mKey1) && Input.GetKeyUp(mCombineKeyList[func].mKey2);
//		}
//		
//		foreach(CombineKeySetting cks in mCombineKeyList.Values)
//		{
//			if(mKeyList.ContainsKey(func) && cks.mKey2 == mKeyList[func].mKey && Input.GetKey(cks.mKey1))
//				return false;
//		}
//		
//		if(mKeyList.ContainsKey(func))
//		{
//			return Input.GetKeyUp(mKeyList[func].mKey) || (mKeyList[func].mCtrl != KeyCode.None && Input.GetKeyUp(mKeyList[func].mCtrl));
//		}
//		
//		return false;
//	}
//	
//	public KeyCode GetKey(KeyFunction func)
//	{
//		if(mKeyList.ContainsKey(func))
//			return mKeyList[func].mKey;
//		return KeyCode.None;
//	}
//
//	public override void Update()
//	{
//		mMouseOnUI = (null != UICamera.hoveredObject);
//		mMouseMove = Vector3.Distance(mOldMousePos, Input.mousePosition) > MousemoveThreshold;
//		
//		if(Input.GetMouseButtonDown(0))
//		{
//			mLMousePress = true;
//			mOldMousePos = Input.mousePosition;
//		}
//		if(Input.GetMouseButtonDown(1))
//		{
//			mRMousePress = true;
//			mOldMousePos = Input.mousePosition;
//		}
//		
//		if(Input.GetMouseButtonUp(0))
//		{
//			if(mLMousePress && !mMouseMove)
//				mLMouseClick = true;
//			mLMousePress = false;
//		}
//		else
//			mLMouseClick = false;
//		
//		
//		if(Input.GetMouseButtonUp(1))
//		{
//			if(mRMousePress && !mMouseMove)
//				mRMouseClick = true;
//			mRMousePress = false;
//		}
//		else
//			mRMouseClick = false;
//		
//		mOnFuncBtn = Input.GetKeyDown(KeyCode.E) && !UICamera.inputHasFocus;
//		UseEquipment = IsKeyDown(KeyFunction.Attack_Build_Dig_Fire1);
//
//		RotItemP = IsKeyPressed(KeyFunction.TakeAllItems_Rotate) && !UICamera.inputHasFocus;
//		RotItemC = IsKeyUp(KeyFunction.TakeAllItems_Rotate) && !UICamera.inputHasFocus;
//
//        //JetPackBoost = (null != PlayerFactory.mMainPlayer
//        //                && IsKeyPressed(KeyFunction.Jump_SwimmingUp_ThrottleUp)
//        //                && !PlayerFactory.mMainPlayer.mDeath && PlayerFactory.mMainPlayer.CanMove());
//		
//		MoveHorizontal = (IsKeyPressed(KeyFunction.MoveLeft) ? -1f : 0f)
//							+ (IsKeyPressed(KeyFunction.MoveRight) ? 1f : 0f)
//								+ Input.GetAxis("Horizontal")
//								+ Input.GetAxis("LeftStickHorizontal");
//		MoveHorizontal = Mathf.Clamp(MoveHorizontal, -1f, 1f);
//		
//		MoveVertical = (IsKeyPressed(KeyFunction.MoveBack) ? -1f : 0f)
//							+ (IsKeyPressed(KeyFunction.MoveForward) ? 1f : 0f)
//								+ Input.GetAxis("Vertical")
//								+ Input.GetAxis("LeftStickVertical");
//		MoveVertical = Mathf.Clamp(MoveVertical, -1f, 1f);
//	}
//}
