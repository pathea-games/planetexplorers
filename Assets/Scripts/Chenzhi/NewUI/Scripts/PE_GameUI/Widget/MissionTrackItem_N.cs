//using UnityEngine;
//using System.Collections;
//
//public class MissionTrackItem_N : MonoBehaviour 
//{
//	public UILabel 	mMissionName;
//	public UILabel 	mMissionContent;
//	public Vector3 	mMissionPos;
//	public float   	mMissionRadius;
//	
//	int 				mId;
//	MissionTrackGui_N	mParent;
//	
//	public void Init(int Id, MissionTrackGui_N parent)
//	{
//		mId = Id;
//		mParent = parent;
//	}
//	
//	public void RestTrack(string missionName, string missionContent)
//	{
//		mMissionName.text = missionName;
//		mMissionContent.text = missionContent;
//	}
//	
//	public void SetMissionPos(Vector3 missionPos,int radius = 0)
//	{
//		mMissionPos = missionPos;
//		mMissionRadius = radius;
//	}
//	void OnCloseMissiontrack()
//	{
//		mParent.OnRemoveTrack(mId);
//	}
//	
//	void OnClick()
//	{
//		mParent.OnMissionSel(mId);
//	}
//}
