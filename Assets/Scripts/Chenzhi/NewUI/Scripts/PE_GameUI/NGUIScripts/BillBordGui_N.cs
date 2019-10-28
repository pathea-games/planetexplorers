using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BillBordGui_N : MonoBehaviour
{
	static BillBordGui_N mInstance;
	public static BillBordGui_N Instance{get{return mInstance;}}
	public Transform mBillbordWnd;
	
	public NpcHeadInfo_N 	mPerfab;
	public HeadMask_N mHeadPerfab;
	
	Dictionary<Transform, NpcHeadInfo_N> mNpcHeadInfoList = new Dictionary<Transform, NpcHeadInfo_N>();
	
	Dictionary<Transform, HeadMask_N> mHeadMaskList = new Dictionary<Transform, HeadMask_N>();

    public float hideDistance = 15f;
    private float distanceMagnitude;

	void Awake()
	{
        distanceMagnitude = hideDistance * hideDistance;
		mInstance = this;
	}
	
	public void AddNpcHeadInfo(Transform targetTran, string npcName, string iconName = null)
	{
		if(!mNpcHeadInfoList.ContainsKey(targetTran))
		{
			if(null == iconName)
				iconName = "Null";
			NpcHeadInfo_N addItem = Instantiate(mPerfab) as NpcHeadInfo_N;
			addItem.transform.parent = mBillbordWnd;
			addItem.transform.localPosition = 100f * Vector3.down;
			addItem.transform.localScale = Vector3.one;
			addItem.SetInfo(npcName, iconName);
			mNpcHeadInfoList.Add(targetTran, addItem);
		}
	}
	
	public void RemoveNpcHeadInfo(Transform targetTran)
	{
		if(mNpcHeadInfoList.ContainsKey(targetTran))
		{
			if(null != targetTran && null != mNpcHeadInfoList[targetTran])
			{
				Destroy(mNpcHeadInfoList[targetTran].gameObject);
				mNpcHeadInfoList.Remove(targetTran);
			}
		}
	}
	
	public HeadMask_N AddHeadMask(Transform targetTran)
	{
		HeadMask_N addItem = Instantiate(mHeadPerfab) as HeadMask_N;
		addItem.transform.parent = mBillbordWnd;
		addItem.transform.localPosition = 100f * Vector3.down;
		addItem.transform.localScale = Vector3.one;
		mHeadMaskList.Add(targetTran, addItem);
		return mHeadMaskList[targetTran];
	}
	
	public void RemoveHeadMask(Transform targetTran)
	{
		if(mHeadMaskList.ContainsKey(targetTran))
		{
			if(null != targetTran && null != mHeadMaskList[targetTran])
			{
				Destroy(mHeadMaskList[targetTran].gameObject);
				mHeadMaskList.Remove(targetTran);
			}
		}
	}
	
	void OnPreRender()
	{
        //if(null == PlayerFactory.mMainPlayer)
        //{
        //    return;
        //}

		foreach(Transform tran in mNpcHeadInfoList.Keys)
		{
			if(null == tran || null == mNpcHeadInfoList[tran])
				continue;
			//Vector3 pos = PETools.PEUtil.MainCamTransform.position;
            Vector3 pos = Vector3.zero;//PlayerFactory.mMainPlayer.transform.position;

            if ((pos - tran.position).sqrMagnitude > distanceMagnitude)
			{
				mNpcHeadInfoList[tran].gameObject.SetActive(false);
				continue;
			}
			else
			{
				mNpcHeadInfoList[tran].gameObject.SetActive(true);
            }

			Vector3 screenPos = Camera.main.WorldToScreenPoint(tran.position);
            if (screenPos.z < 0.5f)
            {
                mNpcHeadInfoList[tran].gameObject.SetActive(false);
                continue;
            }

			screenPos.z = 0;
			mNpcHeadInfoList[tran].transform.localPosition = screenPos;
		}
		
		foreach(Transform tran in mHeadMaskList.Keys)
		{
			if(null == tran || null == mHeadMaskList[tran])
				continue;
			Vector3 screenPos = Camera.main.WorldToScreenPoint(tran.position);
			bool showEnanble = (screenPos.z > 0.5f && screenPos.z < 50f);
			if(mHeadMaskList[tran].gameObject.activeSelf != showEnanble)
				mHeadMaskList[tran].gameObject.SetActive(showEnanble);
			screenPos.z = 0;
			mHeadMaskList[tran].transform.localPosition = screenPos;
		}
	}
}
