using UnityEngine;
using System.Collections;
using Pathea;

public class MissionSelItem_N : MonoBehaviour
{
	public UILabel 	mMissionContent;
	
	UINPCTalk 		mParentNPCTalk;
//	MissionMainGui_N	mParentMissionMain;
	UINpcWnd			mNPCGui;
	
	public int mMissionId;
	
	public UISprite mMissionMarke;

    public BoxCollider mCollider;

    public PeEntity m_Player;
	
	public void SetMission(int missionId, UIBaseWidget parent)
	{
		mMissionId = missionId;
		mParentNPCTalk = parent as UINPCTalk;
//		mParentMissionMain = parent as MissionMainGui_N;
		mNPCGui = parent as UINpcWnd;
		mMissionContent.text = MissionRepository.GetMissionNpcListName (mMissionId, false);

        MissionCommonData data = MissionManager.GetMissionCommonData(missionId);

		if(data == null)
			return ;
		
		if(mMissionMarke != null)
		{
            if (m_Player != null)
			{
                if (MissionManager.Instance.HasMission(missionId))
				{
					if(data.m_Type == MissionType.MissionType_Main/* && !MissionManager.HasRandomMission(missionId)*/)
					{
                        if (MissionManager.Instance.IsReplyMission(missionId))
							mMissionMarke.spriteName = "MainMissionEnd";
						else
							mMissionMarke.spriteName = "MissionNotEnd";
					}
                    else if (data.IsTalkMission())
						mMissionMarke.spriteName = "SubMission";
					else
					{
                        if (MissionManager.Instance.IsReplyMission(missionId))
							mMissionMarke.spriteName = "SubMissionEnd";
						else
							mMissionMarke.spriteName = "MissionNotEnd";
					}
				}
				else
				{
                    if (data.m_Type == MissionType.MissionType_Main)
						mMissionMarke.spriteName = "MainMissionGet";
                    else if(data.m_Type == MissionType.MissionType_Sub)
						mMissionMarke.spriteName = "SubMissionGet";
                    else
                        mMissionMarke.spriteName = "SubMission";
				}
			}
			else
				mMissionMarke.gameObject.SetActive(false);
			mMissionMarke.MakePixelPerfect();
		}
		else
		{
            if (MissionManager.Instance.HasMission(missionId))
			{
                if (data.m_Type == MissionType.MissionType_Main
                    && !MissionManager.HasRandomMission(missionId))
				{
                    if (MissionManager.Instance.IsReplyMission(missionId))
						mMissionContent.color = Color.yellow;
					else
						mMissionContent.color = Color.white;
				}
                else if (data.IsTalkMission())
					mMissionContent.color = Color.white;
				else
				{
                    if (MissionManager.Instance.IsReplyMission(missionId))
						mMissionContent.color = Color.yellow;
					else
						mMissionContent.color = Color.white;
				}
			}
			else
				mMissionContent.color = Color.white;
			
			UIButton button = mMissionContent.GetComponent<UIButton>();
			if(button)
				button.defaultColor = mMissionContent.color;
		}
        RefreshCollider();
    }
	
	public void ActiveMask()
	{
		mMissionMarke.gameObject.SetActive(true);
	}
	
	public void SetMission(int missionId, string content, UIBaseWidget parent, PeEntity player)
	{
        m_Player = player;
		SetMission(missionId,parent);
        content = GameUI.Instance.mNPCTalk.ParseStrDefine(content, MissionManager.GetMissionCommonData(missionId));
        mMissionContent.text = content;
        RefreshCollider();
    }

    public void SetMission(int missionId, string content, string icon, UIBaseWidget parent, PeEntity player)
	{
        m_Player = player;
		SetMission(missionId,parent);
		mMissionContent.text = content;
		if(mMissionMarke != null)
		{
			if(icon == "Null")
				mMissionMarke.enabled = false;
			else
				mMissionMarke.spriteName = icon;
			mMissionMarke.MakePixelPerfect();
		}
        RefreshCollider();
    }

    void RefreshCollider()
    {
        Vector3 size = mCollider.size;
        size.x = mMissionContent.relativeSize.x;
        size.y = mMissionContent.relativeSize.y;
        mCollider.size = size;
        Vector3 center = mCollider.center;
        center.x = size.x * 0.5f;
        center.y = -size.y * 0.5f;
        mCollider.center = center;
    }

//    public void SetMissionTitle(MissionMainGui_N.stMissionView view, UIBaseWidget parent, PeEntity player)
//	{
//        m_Player = player;
//		SetMission(view.MissionID,parent);
//		mMissionContent.text = view.MissionTitle;
//		if(mMissionMarke != null)
//			mMissionMarke.enabled = false;
//	}
	
	void OnBtnClick()
	{
		if(mParentNPCTalk != null)
			mParentNPCTalk.OnMutexBtnClick(mMissionId, mMissionContent.text);
		
//		if(mParentMissionMain != null)
//			mParentMissionMain.OnMutexBtnClick(mMissionId);
		
		if(mNPCGui != null)
			mNPCGui.OnMutexBtnClick(mMissionId);
	}
}

