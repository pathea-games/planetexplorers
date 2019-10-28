using UnityEngine;
using System.Collections;

public class VCEInput : MonoBehaviour
{
	public static Ray s_PickRay;
	public static Ray s_UIRay;
	public static bool s_MouseOnUI;
	public static bool s_MouseLeftPress;
	public static bool s_Cancel;
	private static Vector3 s_RightDownPos;
	public static bool s_Shift;
	public static bool s_Alt;
	public static bool s_Control;
	public static bool s_Left;
	public static bool s_Right;
	public static bool s_Forward;
	public static bool s_Back;
	public static bool s_Up;
	public static bool s_Down;
	public static bool s_Increase = false;
	public static bool s_Decrease = false;
	public static bool s_Undo = false;
	public static bool s_Redo = false;
	public static bool s_Delete = false;
	public static KeyCode s_LeftKeyCode;
	public static KeyCode s_RightKeyCode;
	public static KeyCode s_ForwardKeyCode;
	public static KeyCode s_BackKeyCode;
	private static float s_IncreasePressTime = 0;
	private static float s_DecreasePressTime = 0;
	public static bool s_RightDblClick = false;
//	private static float s_LastRightClickTime = -1;
//	private static Vector3 s_LastRightClickPos = new Vector3 (-100,-100,-100);
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		s_PickRay = VCEditor.Instance.m_MainCamera.ScreenPointToRay(Input.mousePosition);
		s_UIRay = VCEditor.Instance.m_UI.m_UICamera.ScreenPointToRay(Input.mousePosition);
		s_MouseOnUI = Physics.Raycast(s_UIRay, 1000, VCConfig.s_UILayerMask);
		if ( Input.GetMouseButtonDown(1) )
		{
			s_RightDownPos = Input.mousePosition;
		}
		if ( Input.GetMouseButtonUp(1) )
		{
			if ( (Input.mousePosition - s_RightDownPos).magnitude > 4.1f )
			{
				s_Cancel = false;
			}
			else
			{
				s_Cancel = true;
			}
		}
		else
		{
			s_Cancel = false;
		}
		if ( Input.GetKeyDown(KeyCode.Escape) )
		{
			s_Cancel = true;
		}
		s_MouseLeftPress = Input.GetMouseButton(0);
		
		s_Shift = ( Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ) && !UICamera.inputHasFocus;
		s_Control = ( Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ) && !UICamera.inputHasFocus;
		s_Alt = ( Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) ) && !UICamera.inputHasFocus;

		if ( Application.isEditor )
		{
			s_Undo = s_Shift && Input.GetKeyDown(KeyCode.Z) && !UICamera.inputHasFocus;
			s_Redo = s_Shift && Input.GetKeyDown(KeyCode.Y) && !UICamera.inputHasFocus;
			s_Delete = Input.GetKeyDown(KeyCode.Comma) && !UICamera.inputHasFocus;
		}
		else
		{
			s_Undo = s_Control && Input.GetKeyDown(KeyCode.Z) && !UICamera.inputHasFocus;
			s_Redo = s_Control && Input.GetKeyDown(KeyCode.Y) && !UICamera.inputHasFocus;
			s_Delete = Input.GetKeyDown(KeyCode.Delete) && !UICamera.inputHasFocus;
		}
		
		// Calc Direction Input
#if false
		#region _6_Direction_
		Vector3[] directions = new Vector3 [6] { Vector3.left, Vector3.right, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };
		Vector3[] camera_dirs = new Vector3 [6] { 
			-VCEditor.Instance.m_MainCamera.transform.right, 
			VCEditor.Instance.m_MainCamera.transform.right, 
			VCEditor.Instance.m_MainCamera.transform.up, 
			-VCEditor.Instance.m_MainCamera.transform.up, 
			VCEditor.Instance.m_MainCamera.transform.forward, 
			-VCEditor.Instance.m_MainCamera.transform.forward };
		int[] rep_index = new int [6];
		bool[] inputs = new bool [6] { 
			Input.GetKeyDown(KeyCode.LeftArrow), 
			Input.GetKeyDown(KeyCode.RightArrow), 
			Input.GetKeyDown(KeyCode.Space),  
			Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt), 
			Input.GetKeyDown(KeyCode.UpArrow),  
			Input.GetKeyDown(KeyCode.DownArrow) };
		
		for ( int i = 0; i < 6; ++i )
		{
			float minangle = 360;
			int minangle_idx = -1;
			for ( int j = 0; j < 6; ++j )
			{
				float angle = Vector3.Angle(directions[i], camera_dirs[j]);
				if ( angle < minangle )
				{
					minangle = angle;
					minangle_idx = j;
				}
			}
			rep_index[i] = minangle_idx;
		}
		s_Left = inputs[rep_index[0]];
		s_Right = inputs[rep_index[1]];
		s_Forward = inputs[rep_index[4]];
		s_Back = inputs[rep_index[5]];
		s_Up = inputs[rep_index[2]];
		s_Down = inputs[rep_index[3]];
		#endregion
#endif
		
#if true
		#region _4_Direction_
		Vector3[] directions = new Vector3 [4] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
		Vector3[] camera_dirs = new Vector3 [4] { 
			-VCEditor.Instance.m_MainCamera.transform.right, 
			VCEditor.Instance.m_MainCamera.transform.right, 
			VCEditor.Instance.m_MainCamera.transform.forward, 
			-VCEditor.Instance.m_MainCamera.transform.forward };
		int[] rep_index = new int [4];
		bool[] inputs = new bool [6] {
			Input.GetKeyDown(KeyCode.LeftArrow),
			Input.GetKeyDown(KeyCode.RightArrow),
			Input.GetKeyDown(KeyCode.UpArrow),
			Input.GetKeyDown(KeyCode.DownArrow),
			Input.GetKeyDown(KeyCode.PageUp),
			Input.GetKeyDown(KeyCode.PageDown) };
		KeyCode[] keycodes = new KeyCode[4] { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };
		
		for ( int i = 0; i < 4; ++i )
		{
			float minangle = 360;
			int minangle_idx = -1;
			for ( int j = 0; j < 4; ++j )
			{
				Vector3 up_proj = Vector3.Dot(Vector3.up, camera_dirs[j]) * Vector3.up;
				Vector3 horz_dir = (camera_dirs[j] - up_proj).normalized;
				float angle = Vector3.Angle(directions[i], horz_dir);
				if ( angle < minangle )
				{
					minangle = angle;
					minangle_idx = j;
				}
			}
			rep_index[i] = minangle_idx;
		}
		s_Left = inputs[rep_index[0]];
		s_Right = inputs[rep_index[1]];
		s_Forward = inputs[rep_index[2]];
		s_Back = inputs[rep_index[3]];
		s_Up = inputs[4];
		s_Down = inputs[5];

		s_LeftKeyCode = keycodes[rep_index[0]];
		s_RightKeyCode = keycodes[rep_index[1]];
		s_ForwardKeyCode = keycodes[rep_index[2]];
		s_BackKeyCode = keycodes[rep_index[3]];

		#endregion
#endif
		s_Increase = false;
		s_Decrease = false;
		if ( Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.UpArrow) )
			s_Increase = true;
		if ( Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus) || Input.GetKeyDown(KeyCode.DownArrow) )
			s_Decrease = true;
		if ( Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus) || Input.GetKey(KeyCode.UpArrow) )
			s_IncreasePressTime += Time.deltaTime;
		else
			s_IncreasePressTime = 0;
		if ( Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus) || Input.GetKey(KeyCode.DownArrow) )
			s_DecreasePressTime += Time.deltaTime;
		else
			s_DecreasePressTime = 0;
		
		if ( s_IncreasePressTime > 0.65f )
		{
			if ( Time.frameCount % 2 == 0 )
				s_Increase = true;
		}
		if ( s_DecreasePressTime > 0.65f )
		{
			if ( Time.frameCount % 2 == 0 )
				s_Decrease = true;
		}

		s_RightDblClick = false;
//		if ( Input.GetMouseButtonDown(1) )
//		{
//			s_LastRightClickPos = Input.mousePosition;
//			float t = Time.time;
//			if ( t - s_LastRightClickTime < 0.3f )
//			{
//				s_RightDblClick = true;
//				s_LastRightClickTime = -1;
//			}
//			else
//			{
//				s_LastRightClickTime = t;
//			}
//		}
	}
}
