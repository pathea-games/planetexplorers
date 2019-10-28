using UnityEngine;
using System.Collections;
using AiAsset;

public class LimitMapInfoItem_N : MonoBehaviour
{
	public 			UISprite mIcon;
	[HideInInspector]
	public int		mMissionId;
	public void SetMissionTrack(int missionId)
	{
		mMissionId = missionId;
		mIcon.spriteName = "MissionTrack";
	}
}
