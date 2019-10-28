using UnityEngine;
using System.Collections;

public class NetPlayerInfoItem_N : MonoBehaviour 
{
	public UILabel 	mName;
	public UISlider	mLife;
	
	public UILabel	mXPos;
	public UILabel	mZPos;
	
    //Player							mPlayer;
	NETPlayerShowGui_N		mParent;
	
    //public void SetPlayer(Player netPlayer, NETPlayerShowGui_N parent)
    //{
    //    mPlayer = netPlayer;
    //    mParent = parent;
    //    mName.text = netPlayer.name;
    //}
	
	// Update is called once per frame
    //void Update () {
    //    if(null != mPlayer)
    //    {
    //        mLife.sliderValue = mPlayer.GetAttribute(Pathea.AttribType.Hp)/mPlayer.GetAttribute(Pathea.AttribType.HpMax);
    //        mXPos.text = "X: " + (int)mPlayer.transform.position.x;
    //        mZPos.text = "Y: " + (int)mPlayer.transform.position.z;
    //    }
    //    else
    //        mParent.RemovePlayer(this);
    //}
}
