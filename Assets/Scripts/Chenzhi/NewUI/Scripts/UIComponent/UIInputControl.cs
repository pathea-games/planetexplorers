using UnityEngine;
using System.Collections;
using System;

public class UIInputControl : MonoBehaviour 
{

	public UIInput mInput = null;
	public UILabel mlabel = null;
    public UILabel mDefaultlabel = null;
	public BoxCollider mBoxCollider;


	public GameObject mPartentBg;
	public float OtherObjectWdith;

	private float mLabelWidth;
	// Use this for initialization
	void Start () 
	{
        if (null != mDefaultlabel)
        {
            mDefaultlabel.color = new Color32(180, 180, 180, 255);
            //lz-2016.07.01 使用两个label控制，一个显示默认提示，一个用来输入，避免在程序中判断和赋值默认内容的情况
            UIEventListener.Get(mInput.gameObject).onSelect += (go, isSelect) =>
            {
                if (null != mDefaultlabel)
                {
                    if (isSelect)
                    {
                        mDefaultlabel.gameObject.SetActive(false);
                        mlabel.gameObject.SetActive(true);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(mlabel.text))
                        {
                            mDefaultlabel.gameObject.SetActive(true);
                            mlabel.gameObject.SetActive(false);
                        }
                    }
                }
            };
        }
        else
        {
            if (mInput != null && mlabel != null)
            {
                if (mInput.selected)
                    mlabel.color = new Color32(255, 255, 255, 255);
                else
                    mlabel.color = new Color32(111, 111, 111, 255);
            }
        }
            if (mPartentBg != null)
                mLabelWidth = mPartentBg.transform.localScale.x - OtherObjectWdith;
	}
	
	// Update is called once per frame
	void Update () 
	{
        //lz-2016.07.01 如果点外部的按钮设置或清空mInput，mInput会首先失去selected状态，然后执行的设置mInput.text,所以上面的onSelect事件执行不到检测
        if (!mInput.selected && null != mDefaultlabel && !mDefaultlabel.gameObject.activeSelf&& string.IsNullOrEmpty(mlabel.text))
        {
            mDefaultlabel.gameObject.SetActive(true);
            mlabel.gameObject.SetActive(false);
        }

		if(mPartentBg != null)
		{
			mLabelWidth = mPartentBg.transform.localScale.x - OtherObjectWdith;
			if(Convert.ToInt32(mLabelWidth) !=  (mlabel.lineWidth - 10) )
			{
//				Vector3 lbScale  = mlabel.gameObject.transform.localScale;
//				lbScale.x = mLabelWidth;
//				mlabel.gameObject.transform.localScale = lbScale;
				mlabel.lineWidth = Convert.ToInt32(mLabelWidth) - 10;
				if(mBoxCollider != null)
				{
					Vector3 bcSize = mBoxCollider.size;
					mBoxCollider.size = new Vector3(mLabelWidth,bcSize.y,bcSize.z);
					mBoxCollider.center = new Vector3(mLabelWidth/2,0,-1.5f);
				}
			}
		}
	}
}
