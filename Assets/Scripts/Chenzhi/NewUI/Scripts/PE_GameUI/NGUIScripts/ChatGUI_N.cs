using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using CustomData;

public class ChatGUI_N : UIStaticWnd
{
	public ChartItem_N mPrefab;
	public UIInput mContentAll;
//	public UIInput mContentSomeOne;
//	public UILabel mNameTalkWith;
	
	public UITable 		mListTable;
	public UIScrollBar 	mScrollBar;
	
//	public UICheckbox	mAllTab;
//	public UICheckbox	mGrounpTab;
//	public UICheckbox	mSystemTab;
//	public UICheckbox	mSomeOneTab;
	
	public TweenColor mOpBtnCol;
//	public TweenScale mOpBtnScl;
	
//	public GameObject	mInputWnd;
	
	//bool	mTalkToALL;
	
	int 	mNumCount; // for resort Table
	
	int 	MaxListLenth = 50;
	
	
//	float testTimeCount = 3;
	
	List<string>[] mChatStr = new List<string>[5];
	int  mChatType = 0;
	List<ChartItem_N> mChatItem = new List<ChartItem_N>();
	
	protected override void InitWindow ()
	{
		base.InitWindow ();
				//mTalkToALL = true;
		mNumCount = 0;
		for(int i=0;i<mChatStr.Length;i++)
			mChatStr[i] = new List<string>();
//		if(!GameConfig.IsMultiMode)
//			mInputWnd.SetActive(false);
	}
	
	public override void Show ()
	{
		if(!Application.isEditor && !GameConfig.IsMultiMode )
			return;
//		mInputWnd.SetActive(false);
		base.Show();
		
		mOpBtnCol.Reset();
		mOpBtnCol.enabled = false;
		mListTable.Reposition();
	}
	
	void OnAllBtn()
	{
		mChatType = 0;
		ResetList(mChatType);
	}
	
	void OnGroupBtn()
	{
		mChatType = 1;
		ResetList(mChatType);
	}
	
	void OnSomeOneBtn()
	{
		mChatType = 3;
		ResetList(mChatType);
	}
	
	void OnSystemBtn()
	{
		mChatType = 2;
		ResetList(mChatType);
	}
	
	void OnSendBtn()
	{
		SendMessage();
	}
	
	void OnSubmit()
	{
		SendMessage();
	}
	
	void OnTalkToSomeone()
	{
		//mTalkToALL = false;
	}
	void OnTalkToAll()
	{
		//mTalkToALL = true;
	}

    public void AddChat(PromptData.PromptType promptType, string name, int num = 0, int meat = 0, int tdTime = 0, int tdNum = 0, string extStr = "", int type = 2)
    {
        string content = PromptRepository.GetPromptContent((int)promptType);
        //if(content == "")
            return ;

        string itemName = "\"ItemName%\"";          //道具名称
        string itemNum = "\"ItemNum%\"";            //道具名称
        string playerName = "\"PlayerName%\"";      //角色名称
        string heroName = "\"HeroName%\"";          //仆从名称
        string missionName = "\"MissionName%\"";    //任务名称
        string skillName = "\"SkillName%\"";        //技能名称
        string meatNum = "\"MeatNum%\"";            //肉的数量
        string TdTime = "\"TdTime%\"";              //塔防时间
        string TdNum = "\"TdNum%\"";                //怪物个数
        

        if (content.Contains(itemName))
        {
            content = content.Replace(itemName, name);
        }

        if (content.Contains(itemNum))
        {
            content = content.Replace(itemNum, num.ToString());
        }
            
        if (content.Contains(playerName))
        {
            content = content.Replace(playerName, name);
        }

        if (content.Contains(heroName))
        {
            content = content.Replace(heroName, name);
        }

        if (content.Contains(missionName))
        {
            content = content.Replace(missionName, name);
        }

        if (content.Contains(skillName))
        {
            content = content.Replace(skillName, name);
        }

        if (content.Contains(meatNum))
        {
            content = content.Replace(meatNum, meat.ToString());
        }

        if (content.Contains(TdTime))
        {
            content = content.Replace(TdTime, tdTime.ToString());
        }

        if (content.Contains(TdNum))
        {
            content = content.Replace(TdNum, tdNum.ToString());
        }

        if(extStr != "")
            content += extStr;

        AddChat("Mission", content, type);
    }
	
	public void AddChat(string name, string content,int type = 0)//0:all 1:Group 2:Someone 3:Sytem 4:Red Notice
	{
		switch(type)
		{
		case 0:
			name = "[DE625B][" + name + "]:[-]";
			break;
		case 1:
			name = "[7F63FF][" + name + "]:[-]";
			content = "[7F63FF]" + content + "[-]";
			break;
		case 2:
			name = "[8DF3FE][" + name + "]:[-]";	
			content = "[8DF3FE]" + content + "[-]";	
			break;
		case 3:
			name = "[FEFF93][" + name + "]:[-]";
			content = "[FEFF93]" + content + "[-]";
			break;
		case 4:
			name = "[FF0000][" + name + "]:[-]";
			content = "[FF0000]" + content + "[-]";
			break;
		}

		AddChatItem(name , content,0);
		if(type != 0)
			AddChatItem(name, content,type);
		
		if(!IsOpen())
			mOpBtnCol.Play(true);
	}
	
	void AddChatItem(string name, string chatString,int type)
	{
		mChatStr[type].Add(name + "&" + chatString);
		if(mChatType == type)
		{	
			ChartItem_N AddItem = Instantiate(mPrefab) as ChartItem_N;
			AddItem.gameObject.name = "ChatItem" + mNumCount;
			AddItem.transform.parent = mListTable.transform;
			AddItem.transform.localPosition = Vector3.zero;
			AddItem.transform.localRotation = Quaternion.identity;
			AddItem.transform.localScale = Vector3.one;
			AddItem.SetText(name,chatString);
			mChatItem.Add(AddItem);
			if(mChatItem.Count > MaxListLenth)
			{
				mChatItem[0].transform.parent = null;
				Destroy(mChatItem[0].gameObject);
				mChatItem.RemoveAt(0);
				mChatStr[type].RemoveAt(0);
			}
			mListTable.Reposition();
			mScrollBar.scrollValue = 1;
		}	
		mNumCount++;
		if(mNumCount > 200000000)
		{
			mNumCount = 0;
			for(int i=mChatItem.Count - 1;i>=0;i--)
			{
				mChatItem[i].transform.parent = null;
				Destroy(mChatItem[i].gameObject);
				mChatItem.RemoveAt(i);
			}
			ResetList(mChatType);
		}
	}
	
	void ResetList(int type)
	{
		if(mChatStr[type].Count >= mChatItem.Count)
		{
			for(int i=0;i<mChatItem.Count;i++)
			{
				string[] strList = mChatStr[type][i].Split('&');
				mChatItem[i].SetText(strList[0],strList[1]);
			}
			for(int i=mChatItem.Count;i<mChatStr[type].Count;i++)
			{
				ChartItem_N AddItem = Instantiate(mPrefab) as ChartItem_N;
				AddItem.gameObject.name = "ChatItem" + mNumCount;
				AddItem.transform.parent = mListTable.transform;
				AddItem.transform.localPosition = Vector3.zero;
				AddItem.transform.localRotation = Quaternion.identity;
				AddItem.transform.localScale = Vector3.one;
				
				string[] strList = mChatStr[type][i].Split('&');
				AddItem.SetText(strList[0],strList[1]);
				mChatItem.Add(AddItem);
			}
		}
		else
		{
			for(int i=0;i<mChatStr[type].Count;i++)
			{
				string[] strList = mChatStr[type][i].Split('&');
				mChatItem[i].SetText(strList[0],strList[1]);
			}
			for(int i=mChatItem.Count - 1;i>=mChatStr[type].Count;i--)
			{
				mChatItem[i].transform.parent = null;
				Destroy(mChatItem[i].gameObject);
				mChatItem.RemoveAt(i);
			}
		}
		mListTable.Reposition();
		mScrollBar.scrollValue = 1;
	}
	
	void SendMessage()
	{
		if(mContentAll.text == "")
			return;

		if (null == PlayerNetwork.mainPlayer)
            return;

		string msg = mContentAll.text;
		PlayerNetwork.mainPlayer.RequestSendMsg(EMsgType.ToAll, msg);

		mContentAll.text = "";
	}
}