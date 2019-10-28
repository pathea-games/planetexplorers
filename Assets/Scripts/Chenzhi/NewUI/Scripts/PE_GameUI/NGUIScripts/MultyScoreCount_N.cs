using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MultyScoreCount_N : UIBaseWnd 
{
	public UILabel mKillGroupTitle;
	public UILabel mScoreGroupTitle;
	public UILabel mKillNumContent;
	public UILabel mScoreNumContent;
	public UILabel mConditionContent;
	
	public UILabel mScoreRule;
	public UILabel mMeatRule;
	
	public UISprite	mResultSpr;
	public UISprite mTopBg;
	public UILabel	mTopTitle;
		
	public PersonalWnd_N mPerfab;
	public UIGrid		 mGrid;
	List<PersonalWnd_N>	 mWndList = new List<PersonalWnd_N>();
	
	public bool 		ReAwake = false;
	
	static public string EncodeColor (Color32 col32)
	{
        Color col = new Color(col32.r / 255f, col32.g / 255f, col32.b / 255f, col32.a / 255f);
        int i = 0xFFFFFF & (NGUIMath.ColorToInt(col) >> 8);
		return NGUIMath.DecimalToHex(i);
	}
	
	public override void Show ()
	{
		base.Show ();
		mScoreRule.text = BattleConstData._scoreInfo;
		mMeatRule.text = BattleConstData._meatInfo;
		mConditionContent.text = "Score: " + BattleConstData.Instance._win_point 
			+ "     Kills: " + BattleConstData.Instance._win_kill 
			+ "     Hold: " + BattleConstData.Instance._win_site + "Field";
		
		for(int i = mWndList.Count - 1; i >= 0; i--)
		{
			mWndList[i].transform.parent = null;
			Destroy(mWndList[i].gameObject);
		}
		mWndList.Clear();
		
		string groupTitle = "";
		string killStr = "";
		string scoreStr = "";
		
		foreach(int groupId in BattleManager.CampList.Keys)
		{
			groupTitle += "[" + EncodeColor(ForceSetting.Instance.GetForceColor(groupId)) + "]" + "TEAM" + (groupId + 1) + "[-] / ";
            killStr += "[" + EncodeColor(ForceSetting.Instance.GetForceColor(groupId)) + "]" + BattleManager.CampList[groupId]._killCount + "[-] / ";
            scoreStr += "[" + EncodeColor(ForceSetting.Instance.GetForceColor(groupId)) + "]" + (int)BattleManager.CampList[groupId]._point + "[-] / ";
		}
		
		if(groupTitle != "")
		{
			groupTitle = groupTitle.Remove(groupTitle.Length - 3);
			killStr = killStr.Remove(killStr.Length - 3);
			scoreStr = scoreStr.Remove(scoreStr.Length - 3);
		}
			
		mKillGroupTitle.text = groupTitle;
		mScoreGroupTitle.text = groupTitle;
		mKillNumContent.text = killStr;
		mScoreNumContent.text = scoreStr;

		foreach (KeyValuePair<int, List<PlayerBattleInfo>> kv in BattleManager.BattleInfoDict)
		{
			if(null == kv.Value)
				continue;
			PersonalWnd_N addWnd = Instantiate(mPerfab) as PersonalWnd_N;
			addWnd.transform.parent = mGrid.transform;
			addWnd.transform.localPosition = Vector3.zero;
			addWnd.transform.localScale = Vector3.one;
            addWnd.mGroupName.text = "[" + EncodeColor(ForceSetting.Instance.GetForceColor(kv.Key)) + "]" + "TEAM" + (kv.Key + 1) + "[-]";
			addWnd.Init();
			addWnd.SetInfo(kv.Value);
			mWndList.Add(addWnd);
		}
		mGrid.Reposition();
	}
	
	public void SetGameResult(bool isWinner)
	{
		mResultSpr.gameObject.SetActive(true);
		if(isWinner)
			mResultSpr.spriteName = "Title_Victory";
		else
			mResultSpr.spriteName = "Title_Defeat";
			
		mTopBg.gameObject.SetActive(false);
		mTopTitle.gameObject.SetActive(false);
	}
	
	void Update()
	{
		if(ReAwake)
		{
			ReAwake = false;
			Show();
		}
	}
}
