using UnityEngine;
using System.Collections;
using Pathea;

public class ServantShowItem_N : MonoBehaviour 
{
	public UITexture	mHead;
	public UISprite		mDeadSpr;
	
	public UISlider		mLife;
	public UISlider		mComfort;
	public UISlider		mOxygen;
	
	public UISprite		mState;

    PeEntity mNpc;
    EntityInfoCmpt entityInfo;
    public PeEntity NPC { get { return mNpc; } }
	//bool 				mIsDead;
	
	// Update is called once per frame
	void Update () 
	{
        //if(mNpc)
        //{
        //    mLife.sliderValue = mNpc.lifePercent;
        //    mComfort.sliderValue = mNpc.comfortPercent;
        //    mOxygen.sliderValue = mNpc.oxygenPercent;
			
        //    if(mHead.mainTexture != mNpc.GetHeadTex())
        //        mHead.mainTexture = mNpc.GetHeadTex();
	
        //    if (mIsDead != mNpc.dead)
        //    {
        //        mIsDead = mNpc.dead;
        //        mHead.enabled = !mIsDead;
        //        mDeadSpr.enabled = mIsDead;
        //    }
	
        //    switch (mNpc.AttackMode)
        //    {
        //        case EAttackMode.Attack: // attack
        //            mState.spriteName = "ServantAttack_on";
        //            break;
        //        case EAttackMode.Defence: // def
        //            mState.spriteName = "ServantDef_on";
        //            break;
        //        default: // rest
        //            mState.spriteName = "ServantRest_on";
        //            break;
        //    }
        //}
	}
	
	public void SetNpc(PeEntity npc)
	{
        mNpc = npc;
        if (mNpc)
        {
//            EntityInfoCmpt entityInfo = npc.GetCmpt<EntityInfoCmpt>();
            
            //mIsDead = false;
            mHead.mainTexture = npc.GetCmpt<EntityInfoCmpt>().faceTex;
            mHead.enabled = true;
            mDeadSpr.enabled = false;
            //mLife.sliderValue = npc.lifePercent;
            //mComfort.sliderValue = npc.comfortPercent;
            //mOxygen.sliderValue = npc.oxygenPercent;
            mState.enabled = true;
        }
        else
        {
            mHead.mainTexture = null;
            mHead.enabled = false;
            mDeadSpr.enabled = false;
            mLife.sliderValue = 0;
            mComfort.sliderValue = 0;
            mOxygen.sliderValue = 0;
            mState.enabled = false;
        }
	}
	
	void OnServantStateBtn()
	{
        //if(null != mNpc && Input.GetMouseButtonUp(0))
        //    MainLeftGui_N.Instance.OnServantStateBtn(this);
	}
	
	void OnServanteHead()
	{
        //if(null != mNpc && Input.GetMouseButtonUp(0))
        //    MainLeftGui_N.Instance.OnServantHead(this);
	}
}
