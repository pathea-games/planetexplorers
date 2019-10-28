using UnityEngine;
using System.Collections;

public class LifeShowItem_N : MonoBehaviour
{
	public UISprite 		mHeadSpr;
	public UISlider 		mLife;
	public MonoBehaviour	mShowObj;
	
	public void InitItem(AiObject enemy)
	{
		mShowObj = enemy;

        //AiNpcObject npc = mShowObj as AiNpcObject;
        //if (npc != null)
        //    mHeadSpr.spriteName = npc.m_NpcIcon;
        //else
        //{
        //    AiDataObject monster = mShowObj as AiDataObject;
        //    if (monster != null)
        //    {
        //        mHeadSpr.spriteName = AiAsset.AiDataBlock.GetIconName(monster.dataId);
        //    }
        //    else {
        //        Debug.LogError("mShowObj is not npc or monster");
        //    }
        //}
		mHeadSpr.MakePixelPerfect();
		mHeadSpr.transform.localScale = new Vector3(32,32,1);
	}
	
	void Update ()
	{
		if(mShowObj == null)
		{
			mLife.sliderValue = 0;
			Destroy(gameObject);
		}
		else
		{
			if(mShowObj is AiObject)
				mLife.sliderValue = ((AiObject)mShowObj).lifePercent;
		}
	}
}
