using UnityEngine;
using System.Collections;

public class PlayerScoreItem_N : MonoBehaviour 
{
	public UILabel mName;
	public UILabel mKillAndDead;
	public UILabel mScore;
	
	public void SetInfo(string name, int killNum, int deadNum, int score)
	{
		mName.text = name;
		mKillAndDead.text = killNum.ToString() + " / " + deadNum.ToString();
		mScore.text = score.ToString();
	}
}
