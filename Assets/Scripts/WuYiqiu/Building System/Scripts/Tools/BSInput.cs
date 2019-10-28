using UnityEngine;
using System.Collections;

public class BSInput : MonoBehaviour
{
	public static Ray s_PickRay;
	public static Ray s_UIRay;
	public static bool s_MouseOnUI;

	public static bool s_Cancel;
	public static bool s_MouseLeftPress;

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

//	public static KeyCode s_LeftKeyCode;
//	public static KeyCode s_RightKeyCode;
//	public static KeyCode s_ForwardKeyCode;
//	public static KeyCode s_BackKeyCode;

	public static bool s_Increase = false;
	public static bool s_Decrease = false;

	public static bool s_Undo = false;
	public static bool s_Redo = false;

	public static bool s_Delete = false;

	static readonly Vector3[] c_directions = new Vector3[4] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
	static Vector3[] s_cameraDirs = new Vector3[4];
	static int[] s_repIndex = new int[4];
	static bool[] s_inputs = new bool[6];

    void Update()
    {
        if (Camera.main != null)
            s_PickRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (UICamera.mainCamera != null)
            s_UIRay = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);

        s_MouseOnUI = Physics.Raycast(s_UIRay, 1000, 1 << Pathea.Layer.GUI);


        if (Input.GetMouseButtonDown(1) && !VCEditor.s_Active)
        {
            s_RightDownPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1) && !VCEditor.s_Active)
        {
            if ((Input.mousePosition - s_RightDownPos).magnitude > 4.1f)
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            s_Cancel = true;
        }

        s_MouseLeftPress = Input.GetMouseButton(0) && !VCEditor.s_Active;

        s_Shift = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && !UICamera.inputHasFocus && !VCEditor.s_Active;
        s_Control = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && !UICamera.inputHasFocus && !VCEditor.s_Active;
        s_Alt = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && !UICamera.inputHasFocus && !VCEditor.s_Active;

        if (Application.isEditor)
        {
            s_Undo = s_Shift && Input.GetKeyDown(KeyCode.Z) && !UICamera.inputHasFocus && !VCEditor.s_Active;
			s_Redo = s_Shift && Input.GetKeyDown(KeyCode.X) && !UICamera.inputHasFocus && !VCEditor.s_Active;
            s_Delete = Input.GetKeyDown(KeyCode.Comma) && !UICamera.inputHasFocus;
        }
        else
        {
            s_Undo = s_Control && Input.GetKeyDown(KeyCode.Z) && !UICamera.inputHasFocus && !VCEditor.s_Active;
			s_Redo = s_Control && Input.GetKeyDown(KeyCode.X) && !UICamera.inputHasFocus && !VCEditor.s_Active;
            s_Delete = Input.GetKeyDown(KeyCode.Delete) && !UICamera.inputHasFocus && !VCEditor.s_Active;
        }

        //		#region _4_Direction_
        //		Vector3[] directions = new Vector3 [4] { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        //		Vector3[] camera_dirs = new Vector3 [4] { 
        //			-Camera.main.transform.right, 
        //			Camera.main.transform.right, 
        //			Camera.main.transform.forward, 
        //			-Camera.main.transform.forward };
        //		int[] rep_index = new int [4];
        //		bool[] inputs = new bool [6] {
        //			Input.GetKeyDown(KeyCode.LeftArrow),
        //			Input.GetKeyDown(KeyCode.RightArrow),
        //			Input.GetKeyDown(KeyCode.UpArrow),
        //			Input.GetKeyDown(KeyCode.DownArrow),
        //			Input.GetKeyDown(KeyCode.PageUp),
        //			Input.GetKeyDown(KeyCode.PageDown)};
        //		KeyCode[] keycodes = new KeyCode[4] { KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.UpArrow, KeyCode.DownArrow };
        //		
        //		for ( int i = 0; i < 4; ++i )
        //		{
        //			float minangle = 360;
        //			int minangle_idx = -1;
        //			for ( int j = 0; j < 4; ++j )
        //			{
        //				Vector3 up_proj = Vector3.Dot(Vector3.up, camera_dirs[j]) * Vector3.up;
        //				Vector3 horz_dir = (camera_dirs[j] - up_proj).normalized;
        //				float angle = Vector3.Angle(directions[i], horz_dir);
        //				if ( angle < minangle )
        //				{
        //					minangle = angle;
        //					minangle_idx = j;
        //				}
        //			}
        //			rep_index[i] = minangle_idx;
        //		}
        //		s_Left = inputs[rep_index[0]];
        //		s_Right = inputs[rep_index[1]];
        //		s_Forward = inputs[rep_index[2]];
        //		s_Back = inputs[rep_index[3]];
        //		s_Up = inputs[4];
        //		s_Down = inputs[5];
        //		
        //		s_LeftKeyCode = keycodes[rep_index[0]];
        //		s_RightKeyCode = keycodes[rep_index[1]];
        //		s_ForwardKeyCode = keycodes[rep_index[2]];
        //		s_BackKeyCode = keycodes[rep_index[3]];
        //		
        //		#endregion

        #region _4_Direction_
		s_cameraDirs[0] = -Camera.main.transform.right;
		s_cameraDirs[1] = Camera.main.transform.right;
		s_cameraDirs[2] = Camera.main.transform.forward;
		s_cameraDirs[3] = -Camera.main.transform.forward;
		s_inputs[0] = PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_Lt);
		s_inputs[1] = PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_Rt);
		s_inputs[2] = PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_Up);
		s_inputs[3] = PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_Dn);
		s_inputs[4] = Input.GetKeyDown(KeyCode.PageUp); //PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_PgUp),
		s_inputs[5] = Input.GetKeyDown(KeyCode.PageDown);//PeInput.Get(PeInput.LogicFunction.Build_TweakSelectionArea_PgDn)};

//        PeInput.LogicFunction[] keycodes = new PeInput.LogicFunction[4] { PeInput.LogicFunction.Build_TweakSelectionArea_Lt, PeInput.LogicFunction.Build_TweakSelectionArea_Rt, 
//															PeInput.LogicFunction.Build_TweakSelectionArea_Up, PeInput.LogicFunction.Build_TweakSelectionArea_Dn };
		
		for ( int i = 0; i < 4; ++i )
		{
			float minangle = 360;
			int minangle_idx = -1;
			for ( int j = 0; j < 4; ++j )
			{
				Vector3 up_proj = Vector3.Dot(Vector3.up, s_cameraDirs[j]) * Vector3.up;
				Vector3 horz_dir = (s_cameraDirs[j] - up_proj).normalized;
				float angle = Vector3.Angle(c_directions[i], horz_dir);
				if ( angle < minangle )
				{
					minangle = angle;
					minangle_idx = j;
				}
			}
			s_repIndex[i] = minangle_idx;
		}
		s_Left = s_inputs[s_repIndex[0]];
		s_Right = s_inputs[s_repIndex[1]];
		s_Forward = s_inputs[s_repIndex[2]];
		s_Back = s_inputs[s_repIndex[3]];
		s_Up = s_inputs[4];
		s_Down = s_inputs[5];		
		#endregion
	}
}
