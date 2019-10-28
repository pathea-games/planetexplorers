using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;


public class UIServantTalk : UIBaseWidget
{
	public ServantTalkItem_N	mPrefab;
	
	ServantTalkItem_N			mCurrentItem;
	
	ServantTalkItem_N			mLastItem;

    List<TalkData> mTalkList = new List<TalkData>();
	
	float						mCountTime = 0;

    void Update()
	{
		if(mCountTime >= 0)
			mCountTime -= Time.deltaTime;
		
		if(mCountTime <= 0 && mTalkList.Count > 0)
		{
			AddOpBtn(mTalkList[0]);
			mTalkList.RemoveAt(0);
			mCountTime = 3f;
		}
		//use for test
//        mCountTime += Time.deltaTime;
//        if(mCountTime > 3)
//        {
//            mCountTime = 0;
//            AddOpBtn(null,"TestCountent");
//        }
//		if(Input.GetKeyDown(KeyCode.P))
//		{
//			TalkData talkdata = new TalkData();
//			talkdata.m_ID = 5555;
//			talkdata.m_Name = "hell";
//			talkdata.m_Content = "pppp";
//			talkdata.m_ClipName = "fuck";
//			mTalkList.Add(talkdata);
//		}
	}
	
	public void AddTalk(int id, string name = null)
	{		
        TalkData talkdata = TalkRespository.GetTalkData(id);
        if (talkdata == null)
        {
			if(Application.isEditor)
				Debug.LogError("ServantTalk ID" + id + " can't find talk data");
            return;
        }

        //if (!string.IsNullOrEmpty(name))
        //{
        //    talkdata.m_Name = name;
        //}
		
        //if(Application.isEditor)
        //    Debug.Log("AddTalk:name[" + talkdata.m_Name + "]" + "Content["+talkdata.m_Content+"]");
        mTalkList.Add(talkdata);
	}

    void AddOpBtn(TalkData talkdata)
	{
		if(mLastItem != null)
			mLastItem.Hide();
		if(mCurrentItem != null)
		{
			mCurrentItem.Hide();
			mLastItem = mCurrentItem;
		}

        if (talkdata == null)
            return;

        string content = "";
		string name = "";
        //if (talkdata.m_Name != "player")
        //{
        //    AiNpcObject npc;
        //    int outinfo;
        //    if (int.TryParse(talkdata.m_Name, out outinfo))
        //        npc = NpcManager.Instance.GetNpcRandom(outinfo);
        //    else
        //        npc = NpcManager.Instance.GetNpc(talkdata.m_Name);

        //    if (talkdata.m_Name == "0" && GameUI.Instance.mNPCTalk.m_CurSelNpc != null)
        //    {
        //        npc = GameUI.Instance.mNPCTalk.m_CurSelNpc;
        //    }

        //    content = talkdata.m_Content;

        //    if (npc != null)
        //    {
        //        name = npc.NpcName + ": ";
        //        npc.CmdFaceTo = PlayerFactory.mMainPlayer.transform;
        //        npc.ApplySound(talkdata.m_SoundID);
        //    }
        //}
        //else
        //{
        //    name = Pathea.PeCreature.Instance.mainPlayer.ToString();
        //    content = talkdata.m_Content;
        //}

		mCurrentItem = Instantiate(mPrefab) as ServantTalkItem_N;
		mCurrentItem.transform.parent = this.transform;
		mCurrentItem.transform.localPosition = Vector3.zero;
		mCurrentItem.transform.localRotation = Quaternion.identity;
		mCurrentItem.InitItem(name,content);
	}
}
