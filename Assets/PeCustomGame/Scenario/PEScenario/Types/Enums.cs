using UnityEngine;
using System.Collections;

namespace PeCustom
{
	public enum EAxis : int { Left = -1, Right = 1, Up = 2, Down = -2, Forward = 4, Backward = -4, None = 0, Any = 7 }
	public enum EScope : int { Global = 0, Mission = 1 }
	public enum EMissionResult : int { Any = -1, Accomplished = 0, Failed = 1, Aborted = 2 }
    public enum ECommand : int { None = 0, MoveTo, FaceAt, Attack }
	public enum EUIType : int { None = 0, Label = 1, Button = 2, Box = 3 }
	public enum EUIStyle : int { Default = 0 }
	public enum EUIAnchor : int
	{
		Streched = 0,
		LowerLeft,
		LowerCenter,
		LowerRight,
		MiddleLeft,
		Center,
		MiddleRight,
		UpperLeft,
		UpperCenter,
		UpperRight
	}
	public enum ESystemUI : int
	{
		None = 0,
		WorldMap = 1,
		CharacterInfo = 2,
		MissionWindow = 3,
		ItemPackage = 4,
		Replicator = 5,
		Phone = 6,
		CreationSystem = 7,
		BuildMode = 8
	}
}