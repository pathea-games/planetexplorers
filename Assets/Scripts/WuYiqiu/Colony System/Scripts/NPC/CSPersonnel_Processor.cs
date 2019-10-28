using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CSRecord;
using ItemAsset;

using Pathea;
using Pathea.PeEntityExt;
using Pathea.Operate;

public partial class CSPersonnel
{
    public RandomItemObj resultItems;
    public delegate void LineStateChangedDel(CSPersonnel person,int oldLine, int index);
    static event LineStateChangedDel m_ProcessingIndexChangeListenner;

    public static void RegisterProcessingIndexChangedListener(LineStateChangedDel listener)
    {
        m_ProcessingIndexChangeListenner += listener;
    }

    public static void UnRegisterProcessingIndexChangedListener(LineStateChangedDel listener)
    {
        m_ProcessingIndexChangeListenner -= listener;
    }


    public delegate void LineStateInitDel(CSPersonnel person);
    static event LineStateInitDel m_ProcessingIndexInitListenner;

    public static void RegisterProcessingIndexInitListener(LineStateInitDel listener)
    {
        m_ProcessingIndexInitListenner += listener;
    }

    public static void UnRegisterProcessingIndexInitListener(LineStateInitDel listener)
    {
        m_ProcessingIndexInitListenner -= listener;
    }




    private int processingIndex {
        get
        {
            return Data.m_ProcessingIndex;
        }
        set
        {
            Data.m_ProcessingIndex = value;
        }
    }
    public int ProcessingIndex
    {
        get { return processingIndex; }
        set{
            SetProcessingIndex(value);
        }
    }

    public void TrySetProcessingIndex(int index)
    {
        if (processingIndex == index)
        {
            return;
        }

		if(index>=0){
			//--to do: can't work
			if(!CanProcess)
			{
				CSUtils.ShowCannotWorkReason(CannotWorkReason,FullName);
				return;
			}
		}
        if (PeGameMgr.IsMulti)
        {
			if((AiAdNpcNetwork)NetworkInterface.Get(ID)!=null)
            	((AiAdNpcNetwork)NetworkInterface.Get(ID)).RPCServer(EPacketType.PT_CL_CLN_SetProcessingIndex,index);
        }
        else
        {
            ProcessingIndex = index;
        }
    }

    private bool isProcessing
    {
        get { return Data.m_IsProcessing; }
        set
        {
            Data.m_IsProcessing = value;
        }
    }
    public bool IsProcessing
    {
        get { return isProcessing; }
        set {
            SetProcessing(value);
        }
    }

    private void SetProcessing(bool curValue)
    {
        //if(curValue!=isProcessing){
            isProcessing = curValue;
            UpdateNpcCmptProcessing();
        //}
    }

    private void SetProcessingIndex(int index)
    {
        if (processingIndex != index)
        {
            int oldLine = processingIndex;
            processingIndex = index;
            if (m_ProcessingIndexChangeListenner!=null)
                m_ProcessingIndexChangeListenner(this,oldLine, processingIndex);
        }
    }
//    public void InitProcessingState()
//    {
//        if (processingIndex != -1&& (WorkRoom as CSProcessing) != null)
//        {
//            m_ProcessingIndexInitListenner(this);
//        }
//    }
    public void UpdateNpcCmptProcessing()
    {
        //--to do: updateNpccmpt
        //m_NpcCmpt.IsProcessing = ture;
        m_NpcCmpt.Processing = isProcessing;

        //m_NpcCmpt.mRandomItemObj = resultItems;
    }

	public void StopWork(){
		TrySetProcessingIndex(-1);
	}

	public void ShowTips(string content){
		CSUI_MainWndCtrl.ShowStatusBar(content);
	}
}
