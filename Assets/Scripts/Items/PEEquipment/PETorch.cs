using UnityEngine;
using System.Collections;

[RequireComponent(typeof(torch))]
public class PETorch : PeSword 
{
    torch mTorch;
    torch torch
    {
        get
        {
            if (mTorch == null)
            {
                mTorch = GetComponent<torch>();
            }

            return mTorch;
        }
    }
	
	void Update()
	{
		bool holdTorch = false;
		if(null != m_MotionMgr)
			holdTorch = m_MotionMgr.GetMaskState(m_HandChangeAttr.m_HoldActionMask);
		else
			holdTorch = true;

        torch.SetBurning(holdTorch);		
	}
}
