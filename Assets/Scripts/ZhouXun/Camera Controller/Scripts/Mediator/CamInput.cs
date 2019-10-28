using UnityEngine;
using System.Collections;

public enum ECamKey
{
	CK_Null,
	CK_Mouse0,
	CK_Mouse1,
	CK_MouseWheel,
	CK_MouseX,
	CK_MouseY,
	CK_JoyStickX,
	CK_JoyStickY,
	CK_MoveLeft,
	CK_MoveRight,
	CK_MoveUp,
	CK_MoveDown,
	CK_MoveForward,
	CK_MoveBack
}

public static class CamInput
{
	public static bool GetKey (ECamKey key)
	{
		switch (key)
		{
		case ECamKey.CK_Mouse0: return Input.GetMouseButton(0);
		case ECamKey.CK_Mouse1: return Input.GetMouseButton(1);
		case ECamKey.CK_MoveLeft: return PeInput.Get(PeInput.LogicFunction.MoveLeft) || Input.GetKey(KeyCode.LeftArrow) || (SystemSettingData.Instance.UseController && (Input.GetAxis("LeftStickHorizontal") < -0.1f));
		case ECamKey.CK_MoveRight: return PeInput.Get(PeInput.LogicFunction.MoveRight) || Input.GetKey(KeyCode.RightArrow) || (SystemSettingData.Instance.UseController && (Input.GetAxis("LeftStickHorizontal") > 0.1f));
		case ECamKey.CK_MoveUp: return Input.GetKey(KeyCode.Space);
		case ECamKey.CK_MoveDown: return Input.GetKey(KeyCode.LeftAlt);
		case ECamKey.CK_MoveForward: return PeInput.Get(PeInput.LogicFunction.MoveForward) || Input.GetKey(KeyCode.UpArrow) || (SystemSettingData.Instance.UseController && (Input.GetAxis("LeftStickVertical") > 0.1f));
		case ECamKey.CK_MoveBack: return PeInput.Get(PeInput.LogicFunction.MoveBackward) || Input.GetKey(KeyCode.DownArrow) || (SystemSettingData.Instance.UseController && (Input.GetAxis("LeftStickVertical") < -0.1f));
		}
		return false;
	}

	public static float GetAxis (ECamKey key)
	{
		if ( key == ECamKey.CK_MouseWheel )
		{
			return -Input.GetAxis("Mouse ScrollWheel");
		}
		else if ( key == ECamKey.CK_MouseX )
		{
			return Input.GetAxis("Mouse X") * 
					(SystemSettingData.Instance.CameraHorizontalInverse ? -1.0f : 1.0f) * 
					SystemSettingData.Instance.CameraSensitivity;
		}
		else if ( key == ECamKey.CK_MouseY )
		{
			return Input.GetAxis("Mouse Y") * 
					(SystemSettingData.Instance.CameraVerticalInverse ? 1.0f : -1.0f) * 
					SystemSettingData.Instance.CameraSensitivity;
		}
		else if ( key == ECamKey.CK_JoyStickX )
		{
			return SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickX * 180 * Time.deltaTime *
					(SystemSettingData.Instance.CameraHorizontalInverse ? -1.0f : 1.0f) * 
					SystemSettingData.Instance.CameraSensitivity : 0f;
		}
		else if ( key == ECamKey.CK_JoyStickY )
		{
			return SystemSettingData.Instance.UseController ? InControl.InputManager.ActiveDevice.RightStickY * 180 * Time.deltaTime *
					(SystemSettingData.Instance.CameraVerticalInverse ? -1.0f : 1.0f) * 
					SystemSettingData.Instance.CameraSensitivity : 0f;
		}
		else
		{
			return GetKey(key) ? 1.0f : 0.0f;
		}
	}
}
