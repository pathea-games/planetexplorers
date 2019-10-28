#define JOYSTICK_ENABLE //we just disable joystick and set input's axis 's joy num to 11
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;

public static partial class PeInput
{
#region KeyCode extension
	const int ShiftMask = 0x1000;
	const int CtrlMask = 0x2000;
	const int AltMask = 0x4000;
	const int KeyMask = 0x7000;
	const string RTAxis = "JoyRT";
	const string LeftStickVAxis = "LeftStickVertical";
	const string LeftStickHAxis = "LeftStickHorizontal";
	public static KeyCode ShiftKey(KeyCode key){		return (KeyCode)(ShiftMask+(int)key);			}
	public static KeyCode CtrlKey(KeyCode key){			return (KeyCode)(CtrlMask+(int)key);			}
	public static KeyCode CtrlShiftKey(KeyCode key){	return (KeyCode)(ShiftMask+CtrlMask+(int)key);	}
	public static KeyCode AltKey(KeyCode key){			return (KeyCode)(AltMask+(int)key);				}
	public static KeyCode AltShiftKey(KeyCode key){		return (KeyCode)(ShiftMask+AltMask+(int)key);	}
	public static string ToStr(this KeyCode key)
	{
		string ret = string.Empty;
		int mask = (int)key&KeyMask;
		KeyCode lastKey = key;
		if (mask != 0) {
			lastKey = key - mask;
			ret=((mask & ShiftMask) != 0 ? "Shift+" : "") +
				((mask & CtrlMask) != 0 ? "Ctrl+" : "") +
				((mask & AltMask) != 0 ? "Alt+" : "");
		}
		if (lastKey >= KeyCode.Alpha0 && lastKey <= KeyCode.Alpha9) {
			ret = ret + (lastKey - KeyCode.Alpha0);
		} else if (lastKey == KeyCode.Mouse0) {
			ret = ret + "LButton";
		} else if (lastKey == KeyCode.Mouse1) {
			ret = ret + "RButton";
		} else if (lastKey == KeyCode.Mouse2) {
			ret = ret + "MButton";
		} else {
			ret = ret + lastKey.ToString();
		}
		return ret;
	}
	public static string ToStrShort(this KeyCode key)
	{
		string ret;
		int mask = (int)key&KeyMask;
		KeyCode lastKey = key - mask;
		if (mask != 0) {
			ret=((mask & ShiftMask) != 0 ? "Sh+" : "") +
				((mask & CtrlMask) != 0 ? "Ct+" : "") +
					((mask & AltMask) != 0 ? "Al+" : "") +
					lastKey.ToString ();
		} else {
			ret=key.ToString ();
		}
		return ret.Replace("Button", "").Replace("Alpha", "");
	}
#endregion

#region KeyCode method extension
	// Left Mouse click with mouse move check
	static float s_rMouseLastDown = Time.time;
	static Vector2 s_rMousePosWhileDown = Vector2.zero;
	public static bool IsClickNoMove(this KeyCode key)
	{
		if (key != KeyCode.Mouse0 && key != KeyCode.Mouse1 && key != KeyCode.Mouse2) {
			return Input.GetKeyUp(key) && NotExcluderByOther();
		} else {
			if (Input.GetKeyDown (KeyCode.Mouse1) && NotExcluderByOther()) {
				s_rMouseLastDown = Time.time;
				s_rMousePosWhileDown = Input.mousePosition;
			} else if (Input.GetKeyUp (KeyCode.Mouse1) && NotExcluderByOther()) {
				float dist = Vector2.Distance (s_rMousePosWhileDown, Input.mousePosition);
				//Debug.LogError("mouse move "+ dist);
				if (dist < 20 && Time.time - s_rMouseLastDown < 0.25f) {
					return true;
				}
			}
			return false;
		}
	}

	// Down&Up / Click
	private class ClickPressInfo
	{
		public KeyCode _key;
		public float _interval;
		public float _lastDown;
		public ClickPressInfo(KeyCode key, float[] para)
		{
			_key = key;
			_interval = para == null ? 0.5f : para[0];	// extend interval from 0.2 to 0.3 in order to fix bug 502547, extend to 0.5f accroding to gamers' demands
			_lastDown = -1;
		}
		// static funcs
		static List<ClickPressInfo> s_clickPressKeys = new List<ClickPressInfo>();
		public static bool TryGetValue(KeyCode key, out ClickPressInfo info)
		{
			info = null;

			int idx = s_clickPressKeys.FindIndex (x => x._key == key);
			if (idx < 0)
				return false;

			info = s_clickPressKeys [idx];
			return true;
		}
		public static void Register(KeyCode key, float[] para)
		{
			int idx = s_clickPressKeys.FindIndex (x => x._key == key);
			if (idx < 0) {
				s_clickPressKeys.Add(new ClickPressInfo(key, para));
			} else {
				s_clickPressKeys[idx] = new ClickPressInfo(key, para);
			}
		}
		public static void Clear()
		{
			s_clickPressKeys.Clear();
		}	
	}
	public static bool IsClickPress(this KeyCode key)
	{
		ClickPressInfo pressInfo;
		if(Input.GetKeyDown(key) && NotExcluderByOther() && ClickPressInfo.TryGetValue(key, out pressInfo))
		{
			pressInfo._lastDown = Time.time;
		}
		else if(Input.GetKeyUp(key) && NotExcluderByOther() && ClickPressInfo.TryGetValue(key, out pressInfo))
		{
			float deltaTime = Time.time - pressInfo._lastDown;
			if(deltaTime < pressInfo._interval)
			{
				return true;
			}
		}
		return false;
	}

	// Click with CD
	private class ClickCDPressInfo
	{
		public KeyCode _key;
		public float _cd;
		public float _interval;
		public float _lastDown;
		public float _lastClick;
		public ClickCDPressInfo(KeyCode key, float[] para)
		{
			_key = key;
			_cd = para == null ? 2f : para[0];
			_interval = (para == null||para.Length>=2) ? 0.2f : para[1];
			_lastDown = -1;
			_lastClick = -1;
		}
		//static funcs
		static List<ClickCDPressInfo> s_clickCDPressKeys = new List<ClickCDPressInfo>();
		public static bool TryGetValue(KeyCode key, out ClickCDPressInfo info)
		{
			info = null;
			
			int idx = s_clickCDPressKeys.FindIndex (x => x._key == key);
			if (idx < 0)
				return false;
			
			info = s_clickCDPressKeys [idx];
			return true;
		}
		public static void Register(KeyCode key, float[] para)
		{
			int idx = s_clickCDPressKeys.FindIndex (x => x._key == key);
			if (idx < 0) {
				s_clickCDPressKeys.Add(new ClickCDPressInfo(key, para));
			} else {
				s_clickCDPressKeys[idx] = new ClickCDPressInfo(key, para);
			}
		}
		public static void Clear()
		{
			s_clickCDPressKeys.Clear();
		}	
	}
	public static bool IsClickCDPress(this KeyCode key)
	{
		bool bDn = Input.GetKeyDown(key);
		bool bUp = Input.GetKeyUp(key);
		ClickCDPressInfo pressInfo;
		if((bDn || bUp) && NotExcluderByOther() && ClickCDPressInfo.TryGetValue(key, out pressInfo))
		{
			if(Time.time - pressInfo._lastClick < pressInfo._cd)
				return false;
			
			if(bDn)
			{
				pressInfo._lastDown = Time.time;
			}
			else if(bUp)
			{
				float deltaTime = Time.time - pressInfo._lastDown;
				if(deltaTime < pressInfo._interval)
				{
					pressInfo._lastClick = Time.time;
					return true;
				}
			}
		}
		return false;
	}

	// Double Press
	private class DoublePressInfo
	{
		public KeyCode _key;
		public float _interval;
		public float _lastDown0;
		public float _lastDown1;
		public DoublePressInfo(KeyCode key, float[] para)
		{
			_key = key;
			_interval = para == null ? 0.2f : para[0];
			_lastDown0 = -1;
			_lastDown1 = -1;
		}

		static List<DoublePressInfo> s_doublePressKeys = new List<DoublePressInfo>();
		public static bool TryGetValue(KeyCode key, out DoublePressInfo info)
		{
			info = null;
			
			int idx = s_doublePressKeys.FindIndex (x => x._key == key);
			if (idx < 0)
				return false;
			
			info = s_doublePressKeys [idx];
			return true;
		}
		public static void Register(KeyCode key, float[] para)
		{
			int idx = s_doublePressKeys.FindIndex (x => x._key == key);
			if (idx < 0) {
				s_doublePressKeys.Add(new DoublePressInfo(key, para));
			} else {
				s_doublePressKeys[idx] = new DoublePressInfo(key, para);
			}
		}
		public static void Clear()
		{
			s_doublePressKeys.Clear();
		}	
	}
	public static bool IsDoublePress(this KeyCode key)
	{
		DoublePressInfo pressInfo;
		if(Input.GetKeyDown(key) && NotExcluderByOther() && DoublePressInfo.TryGetValue(key, out pressInfo))
		{
			pressInfo._lastDown1 = Time.time;
			
			float deltaTime = Time.time - pressInfo._lastDown0;
			if(deltaTime < pressInfo._interval)
			{
				//Debug.LogError("DoublePress 111111");
				return true;
			}
			//Debug.LogError("DoublePress 000000");
			return false;
		}
		else if(Input.GetKeyUp(key) && NotExcluderByOther() && DoublePressInfo.TryGetValue(key, out pressInfo))
		{
			pressInfo._lastDown0 = pressInfo._lastDown1;
		}
		return false;
	}

	// Long Press
	private class LongPressInfo
	{
		public KeyCode _key;
		public float _term;
		public float _startDown;
		public LongPressInfo(KeyCode key, float[] para)
		{
			_key = key;
			_term = para == null ? 0.3f : para[0];
			_startDown = 0.0f;
		}
		// static funcs
		static List<LongPressInfo> s_longPressKeys = new List<LongPressInfo>();
		public static bool TryGetValue(KeyCode key, out LongPressInfo info)
		{
			info = null;
			
			int idx = s_longPressKeys.FindIndex (x => x._key == key);
			if (idx < 0)
				return false;
			
			info = s_longPressKeys [idx];
			return true;
		}
		public static void Register(KeyCode key, float[] para)
		{
			int idx = s_longPressKeys.FindIndex (x => x._key == key);
			if (idx < 0) {
				s_longPressKeys.Add(new LongPressInfo(key, para));
			} else {
				s_longPressKeys[idx] = new LongPressInfo(key, para);
			}
		}
		public static void Clear()
		{
			s_longPressKeys.Clear();
		}	
	}
	public static bool IsLongPress(this KeyCode key)
	{
		LongPressInfo pressInfo;
		if(LongPressInfo.TryGetValue(key, out pressInfo))
		{
			if(!Input.GetKey(key) || !NotExcluderByOther())
			{
				pressInfo._startDown = 0.0f;
				return false;
			}
			if(pressInfo._startDown < PETools.PEMath.Epsilon)
			{
				pressInfo._startDown = Time.time;
				return false;
			}
			return Time.time - pressInfo._startDown > pressInfo._term;
		}
		return false;
	}

	private class JoyAxisStateInfo
	{
		public InputControlType controlType;
		public bool positiveDoubleDown;
		public bool negativeDoubleDown;
		public float axisValue;
		float m_LastCheckTime;
		float m_LastPositiveDownTime;
		float m_LastNegativeDownTime;
		float m_DoubleInterval;

		static readonly float ThresholdValue = 0.7f;

		static List<JoyAxisStateInfo> s_JoyAxisStateInfos = new List<JoyAxisStateInfo>();

		JoyAxisStateInfo(InputControlType type, float[] para)
		{
			controlType = type;
			m_DoubleInterval = para == null ? 0.3f : para[0];
		}

		JoyAxisStateInfo UpdateInfo()
		{
			if(Time.time - m_LastCheckTime > PETools.PEMath.Epsilon)
			{
				m_LastCheckTime = Time.time;
				float curentValue = InputManager.ActiveDevice.GetControl(controlType);
				bool positivePress = curentValue >= ThresholdValue;
				bool negativePress = curentValue <= -ThresholdValue;
				bool positiveDown = axisValue < ThresholdValue && positivePress;
				bool negativeDown = axisValue > -ThresholdValue && negativePress;

				if(positiveDown) { positiveDoubleDown = Time.time - m_LastPositiveDownTime <= m_DoubleInterval; m_LastPositiveDownTime = Time.time; }
				else positiveDoubleDown = false;

				if(negativeDown) { negativeDoubleDown = Time.time - m_LastNegativeDownTime <= m_DoubleInterval; m_LastNegativeDownTime = Time.time; }
				else negativeDoubleDown = false;

				axisValue = curentValue;
			}

			return this;
		}

		public static JoyAxisStateInfo GetValue(InputControlType type)
		{	
			int idx = s_JoyAxisStateInfos.FindIndex (x => x.controlType == type);
			return s_JoyAxisStateInfos [idx].UpdateInfo();
		}
		
		public static void Register(InputControlType type, float[] para)
		{
			int idx = s_JoyAxisStateInfos.FindIndex (x => x.controlType == type);
			if (idx < 0)
				s_JoyAxisStateInfos.Add(new JoyAxisStateInfo(type, para));
		}

		public static void Clear()
		{
			s_JoyAxisStateInfos.Clear();
		}
	}

	//Axis
#if false
	public static bool ArrowAxisEnable{ get{ return s_arrowAxisEnable; } set{ s_arrowAxisEnable = value; } }
	static bool s_arrowAxisEnable = true;
#else // No arrowKey input enabled
	public static bool ArrowAxisEnable{ get{ return s_arrowAxisEnable; } set{ s_arrowAxisEnable = false; } }
	static bool s_arrowAxisEnable = false;
#endif
	static float s_curAxisH = 0;
	static float s_curAxisV = 0;

	//static int s_curAxisHKeyState = 0;
	//static int s_curAxisVKeyState = 0;
	static KeyCode s_keyAxisU;
	static KeyCode s_keyAxisD;
	static KeyCode s_keyAxisR;
	static KeyCode s_keyAxisL;
	static void UpdateAxisWithoutArrowKey()
	{
		// Vertical
		if(Input.GetKeyDown(s_keyAxisU))
		{
			s_curAxisV = 1;
			PeInput.UsingJoyStick = false;
		}
		else if(Input.GetKeyDown(s_keyAxisD))
		{
			s_curAxisV = -1;
			PeInput.UsingJoyStick = false;
		}
		if(!Input.GetKey(s_keyAxisU) && !Input.GetKey(s_keyAxisD))
		{
#if JOYSTICK_ENABLE
			s_curAxisV = SystemSettingData.Instance.UseController ? InputManager.ActiveDevice.LeftStickY : 0f;
			if(Mathf.Abs(s_curAxisV) > 0.1f)
				PeInput.UsingJoyStick = true;
#else
			s_curAxisV = 0;
#endif
		}
		else if(!Input.GetKey(s_keyAxisU))
		{
			s_curAxisV = -1;
		}
		else if(!Input.GetKey(s_keyAxisD))
		{
			s_curAxisV = 1;
		}
		//Horizontal
		if(Input.GetKeyDown(s_keyAxisR))
		{
			s_curAxisH = 1;
			PeInput.UsingJoyStick = false;
		}
		else if(Input.GetKeyDown(s_keyAxisL))
		{
			s_curAxisH = -1;
			PeInput.UsingJoyStick = false;
		}
		if(!Input.GetKey(s_keyAxisR) && !Input.GetKey(s_keyAxisL))
		{
#if JOYSTICK_ENABLE
			s_curAxisH = SystemSettingData.Instance.UseController ? InputManager.ActiveDevice.LeftStickX : 0f;
			if(Mathf.Abs(s_curAxisH) > 0.1f)
				PeInput.UsingJoyStick = true;
#else
			s_curAxisH = 0;
#endif
		}
		else if(!Input.GetKey(s_keyAxisR))
		{
			s_curAxisH = -1;
		}
		else if(!Input.GetKey(s_keyAxisL))
		{
			s_curAxisH = 1;
		}
		
#if JOYSTICK_ENABLE
		if(Mathf.Abs(InputManager.ActiveDevice.RightStickX) > 0.1f ||  Mathf.Abs(InputManager.ActiveDevice.RightStickY) > 0.1f) PeInput.UsingJoyStick = true;
		else if(Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f) PeInput.UsingJoyStick = false;
#endif
	}
	static void UpdateAxisWithArrowKey()
	{
		// Vertical
		if(Input.GetKeyDown(s_keyAxisU) || Input.GetKeyDown(KeyCode.UpArrow))
		{
			s_curAxisV = 1;
			PeInput.UsingJoyStick = false;
		}
		else if(Input.GetKeyDown(s_keyAxisD) || Input.GetKeyDown(KeyCode.DownArrow))
		{
			s_curAxisV = -1;
			PeInput.UsingJoyStick = false;
		}
		if(!Input.GetKey(s_keyAxisU) && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(s_keyAxisD) && !Input.GetKey(KeyCode.DownArrow))
		{
#if JOYSTICK_ENABLE
			s_curAxisV = SystemSettingData.Instance.UseController ? InputManager.ActiveDevice.LeftStickY : 0f;
			if(Mathf.Abs(s_curAxisV) > 0.1f)
				PeInput.UsingJoyStick = true;
#else
			s_curAxisV = 0;
#endif
		}
		else if(!Input.GetKey(s_keyAxisU) && !Input.GetKey(KeyCode.UpArrow))
		{
			s_curAxisV = -1;
		}
		else if(!Input.GetKey(s_keyAxisD) && !Input.GetKey(KeyCode.DownArrow))
		{
			s_curAxisV = 1;
		}
		//Horizontal
		if(Input.GetKeyDown(s_keyAxisR) || Input.GetKeyDown(KeyCode.RightArrow))
		{
			s_curAxisH = 1;
			PeInput.UsingJoyStick = false;
		}
		else if(Input.GetKeyDown(s_keyAxisL) || Input.GetKeyDown(KeyCode.LeftArrow))
		{
			s_curAxisH = -1;
			PeInput.UsingJoyStick = false;
		}
		if(!Input.GetKey(s_keyAxisR) && !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(s_keyAxisL) && !Input.GetKey(KeyCode.LeftArrow))
		{
#if JOYSTICK_ENABLE
			s_curAxisH = SystemSettingData.Instance.UseController ? InputManager.ActiveDevice.LeftStickX : 0f;
			if(Mathf.Abs(s_curAxisH) > 0.1f)
				PeInput.UsingJoyStick = true;
#else
			s_curAxisH = 0;
#endif
		}
		else if(!Input.GetKey(s_keyAxisR) && !Input.GetKey(KeyCode.RightArrow))
		{
			s_curAxisH = -1;
		}
		else if(!Input.GetKey(s_keyAxisL) && !Input.GetKey(KeyCode.LeftArrow))
		{
			s_curAxisH = 1;
		}

#if JOYSTICK_ENABLE
		if(Mathf.Abs(InputManager.ActiveDevice.RightStickX) > 0.1f ||  Mathf.Abs(InputManager.ActiveDevice.RightStickY) > 0.1f) PeInput.UsingJoyStick = true;
		else if(Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f) PeInput.UsingJoyStick = false;
#endif
	}
#endregion

#region Press Type Extension
	enum KeyPressType
	{
		Down,		// the first frame of key down
		Up,			// the first frame of key up
		UpHPrior,	// for those pair logic key(down to start, up to end), these end should not be skipped by other logic key
		Press,		// GetKey
		PressHPrior,// should not be skipped by other logic key
		Click,
		ClickCD,
		DoublePress,
		LongPress,
		DirU,
		DirD,
		DirR,
		DirL,
		MouseWheelU,
		MouseWheelD,
		ClickNoMove,
		JoyStickBegin,
		JoyDown = JoyStickBegin,
		JoyUp,
		JoyPress,
		JoyStickUpDoublePress,
		JoyStickDownDoublePress,
		JoyStickRightDoublePress,
		JoyStickLeftDoublePress,
		Max,
	}
	static Func<bool> CreatePressTestFunc(KeyCode key, KeyPressType pressType, float[] para)
	{
		if(pressType < KeyPressType.JoyStickBegin)
		{
			if(key == KeyCode.None)	return ()=>false;
			int mask = (int)key&KeyMask;		// use excluder to check key mask
			if(mask != 0)						key = key-mask;
		}

		switch(pressType)
		{
		default:
		case KeyPressType.Up:				return ()=>Input.GetKeyUp(key)&&NotExcluderByOther();
		case KeyPressType.Down:				return ()=>Input.GetKeyDown(key)&&NotExcluderByOther();
		case KeyPressType.Press:			return ()=>Input.GetKey(key)&&NotExcluderByOther();
		case KeyPressType.UpHPrior:			return ()=>Input.GetKeyUp(key);
		case KeyPressType.PressHPrior:		return ()=>Input.GetKey(key);
		case KeyPressType.ClickNoMove:		return ()=>key.IsClickNoMove();
		case KeyPressType.Click:			ClickPressInfo.Register(key, para);		return ()=>key.IsClickPress();
		case KeyPressType.ClickCD:			ClickCDPressInfo.Register(key, para);	return ()=>key.IsClickCDPress();
		case KeyPressType.DoublePress:		DoublePressInfo.Register(key, para);	return ()=>key.IsDoublePress();
		case KeyPressType.LongPress:		LongPressInfo.Register(key, para);		return ()=>key.IsLongPress();
		case KeyPressType.DirU:				s_keyAxisU = key;	return delegate{return s_curAxisV > PETools.PEMath.Epsilon;};
		case KeyPressType.DirD:				s_keyAxisD = key;	return delegate{return s_curAxisV < -PETools.PEMath.Epsilon;};
		case KeyPressType.DirR:				s_keyAxisR = key;	return delegate{return s_curAxisH > PETools.PEMath.Epsilon;};
		case KeyPressType.DirL:				s_keyAxisL = key;	return delegate{return s_curAxisH < -PETools.PEMath.Epsilon;};
		case KeyPressType.MouseWheelU:		return delegate{return Input.GetAxis("Mouse ScrollWheel") > PETools.PEMath.Epsilon;};
		case KeyPressType.MouseWheelD:		return delegate{return Input.GetAxis("Mouse ScrollWheel") < -PETools.PEMath.Epsilon;};
		}
	}

	static Func<bool> CreatePressTestFuncJoy(InputControlType key, KeyPressType pressType, float[] para)
	{
		if(InputControlType.None == key)
			return ()=>false;
		
		switch(pressType)
		{
		default: return ()=>false;
		case KeyPressType.JoyDown:	return ()=>InputManager.ActiveDevice.GetControl(key).WasPressed;
		case KeyPressType.JoyUp:	return ()=>InputManager.ActiveDevice.GetControl(key).WasReleased;
		case KeyPressType.JoyPress: return ()=>InputManager.ActiveDevice.GetControl(key).IsPressed;
		case KeyPressType.JoyStickUpDoublePress:	JoyAxisStateInfo.Register(InputControlType.LeftStickY, para); return ()=>JoyAxisStateInfo.GetValue(InputControlType.LeftStickY).positiveDoubleDown;
		case KeyPressType.JoyStickDownDoublePress:	JoyAxisStateInfo.Register(InputControlType.LeftStickY, para); return ()=>JoyAxisStateInfo.GetValue(InputControlType.LeftStickY).negativeDoubleDown;
		case KeyPressType.JoyStickRightDoublePress:	JoyAxisStateInfo.Register(InputControlType.LeftStickX, para); return ()=>JoyAxisStateInfo.GetValue(InputControlType.LeftStickX).positiveDoubleDown;
		case KeyPressType.JoyStickLeftDoublePress:	JoyAxisStateInfo.Register(InputControlType.LeftStickX, para); return ()=>JoyAxisStateInfo.GetValue(InputControlType.LeftStickX).negativeDoubleDown;
		}
	}
	static void ResetPressInfo()
	{
		ClickPressInfo.Clear ();
		ClickCDPressInfo.Clear ();
		DoublePressInfo.Clear ();
		LongPressInfo.Clear ();
		JoyAxisStateInfo.Clear();
	}
	/*
	private class KeyPress
	{
		public KeyCode _key;
		public KeyPressType _pressType;
		public Func<bool> _pressTst;
		public KeyPress(KeyCode key = KeyCode.None, KeyPressType pressType = KeyPressType.Down)
		{
			_key = key;
			_pressType = pressType;
			_pressTst = CreatePressTestFunc(key, pressType, null);
		}
		public bool PressTst()
		{
			return _pressTst();
		}
		
		public static implicit operator KeyPress(KeyCode key)
		{
			return new KeyPress(key);
		}
	}
	*/
#endregion
}
