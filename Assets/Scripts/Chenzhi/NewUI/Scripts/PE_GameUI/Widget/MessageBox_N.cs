using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MsgBoxType
{
    Msg_OK,
    Msg_YN,
    Msg_Mask
};

public enum MsgInfoType
{
    Null,
    NoticeOnly,

    // Mask
    LobbyLoginMask,
    ServerLoginMask,
    ServerDeleteMask,
    FastTravelMask,
	DungeonGeneratingMask
};

public class MessageBox_N : UIStaticWnd
{
    static MessageBox_N mInstance;

    public static MessageBox_N Instance { get { return mInstance; } }

    public static bool IsShowing { get { return null != mInstance ? (mInstance.mMsgs.Count > 0) : false; } }

    public UILabel mContent;
    public N_ImageButton mOkBtn;
    public N_ImageButton mYseBtn;
    public N_ImageButton mNoBtn;

    public class Message
    {
        public MsgBoxType mType;
        public MsgInfoType mInfotype;
        public string mContent;

        public CallBackFunc mYesFunc;
        public CallBackFunc mNoFunc;
        public CallBackFunc mOkFunc;
        public CallBackFunc mTimeOutFunc;

        public UTimer mTimer;
    }

    //	bool mInit = false;

    List<Message> mMsgs = new List<Message>();

    public delegate void CallBackFunc();

    void Awake()
    {
        mInstance = this;
        mMsgs.Clear();
    }

    void OnNoOrOkBtn()
    {
        if (mMsgs.Count > 0)
        {
            if (null != mMsgs[0].mNoFunc)
                mMsgs[0].mNoFunc();
            if (null != mMsgs[0].mOkFunc)
                mMsgs[0].mOkFunc();
            mMsgs.RemoveAt(0);
        }
        ResetMsgBox();
    }

    void OnYesBtn()
    {
        if (mMsgs.Count > 0)
        {
            if (null != mMsgs[0].mYesFunc)
                mMsgs[0].mYesFunc();
            mMsgs.RemoveAt(0);
        }
        ResetMsgBox();
    }

	public static Message ShowOkBox(string text, CallBackFunc func = null)
    {
        if (mInstance)
        {
            Message msg = new Message();
            msg.mType = MsgBoxType.Msg_OK;
            msg.mInfotype = MsgInfoType.NoticeOnly;
            msg.mContent = text;
            msg.mOkFunc = func;
            mInstance.mMsgs.Add(msg);
            mInstance.ResetMsgBox();
			return msg;
        }
		return null;
    }

	public static Message ShowYNBox(string text, CallBackFunc yesFunc = null, CallBackFunc noFunc = null)
    {
        if (mInstance)
        {
            Message msg = new Message();
            msg.mType = MsgBoxType.Msg_YN;
            msg.mInfotype = MsgInfoType.NoticeOnly;
            msg.mContent = text;
            msg.mYesFunc = yesFunc;
            msg.mNoFunc = noFunc;
            mInstance.mMsgs.Add(msg);
            mInstance.ResetMsgBox();
			return msg;
        }

		return null;
    }

	public static Message ShowMaskBox(MsgInfoType type, string text, float waitTime = 600f, CallBackFunc timeOutFunc = null)
    {
        if (mInstance)
        {
			for(int i = 0; i < mInstance.mMsgs.Count; ++i)
				if(mInstance.mMsgs[i].mInfotype == type)
					return null;
            Message msg = new Message();
            msg.mType = MsgBoxType.Msg_Mask;
            msg.mInfotype = type;
            msg.mContent = text;
            msg.mTimeOutFunc = timeOutFunc;
            msg.mTimer = new UTimer();
            msg.mTimer.Second = waitTime;
            msg.mTimer.ElapseSpeed = -1;
            mInstance.mMsgs.Add(msg);
            mInstance.ResetMsgBox();
			return msg;
        }
		return null;
    }

	public static void CancelMessage(Message msg)
	{
		if(null != mInstance)
			mInstance.CancelMsg(msg);
	}

    public static void CancelMask(MsgInfoType info)
    {
        if (mInstance)
            mInstance.CancelMaskP(info);
    }

    public void CancelMaskP(MsgInfoType info)
    {
        for (int i = 0; i < mMsgs.Count; i++)
        {
            if (mMsgs[i].mInfotype == info)
            {
                //lz-2016.05.25 按Esc隐藏的时候，调用No或者Ok事件
                if (null != mMsgs[i].mNoFunc)
                    mMsgs[i].mNoFunc();
                if (null != mMsgs[i].mOkFunc)
                    mMsgs[i].mOkFunc();

                mMsgs.RemoveAt(i);
                if (i == 0)
                    ResetMsgBox();
                break;
            }
        }
    }

	public void CancelMsg(Message msg)
	{
		if(null == msg)
			return;
		for (int i = 0; i < mMsgs.Count; i++)
		{
			if (mMsgs[i] == msg)
			{
				mMsgs.RemoveAt(i);
				if (i == 0)
					ResetMsgBox();
				break;
			}
		}
	}

    public MsgInfoType GetCurrentInfoTypeP()
    {
        if (mMsgs.Count > 0)
            return mMsgs[0].mInfotype;
        return MsgInfoType.Null;
    }

    void ResetMsgBox()
    {
        if (mMsgs.Count > 0)
        {
            Show();
            switch (mMsgs[0].mType)
            {
                case MsgBoxType.Msg_OK:
                    mYseBtn.gameObject.SetActive(false);
                    mNoBtn.gameObject.SetActive(false);
                    mOkBtn.gameObject.SetActive(true);
                    break;
                case MsgBoxType.Msg_YN:
                    mYseBtn.gameObject.SetActive(true);
                    mNoBtn.gameObject.SetActive(true);
                    mOkBtn.gameObject.SetActive(false);
                    break;
                case MsgBoxType.Msg_Mask:
                    mYseBtn.gameObject.SetActive(false);
                    mNoBtn.gameObject.SetActive(false);
                    mOkBtn.gameObject.SetActive(false);
                    break;
            }
            mContent.text = mMsgs[0].mContent;

            GlobalEvent.NoticeMouseUnlock();
        }
        else
            Hide();
    }
    void Update()
    {
        if (mMsgs.Count > 0)
        {
            if (null == mMsgs[0])
            {
                mMsgs.RemoveAt(0);
                return;
            }

            if (mMsgs[0].mType == MsgBoxType.Msg_OK && Input.GetKeyDown(KeyCode.Return))
            {
                OnNoOrOkBtn();
                return;
            }

            if (mMsgs[0].mType == MsgBoxType.Msg_Mask && null != mMsgs[0].mTimer)
            {
                uint preSec = (uint)mMsgs[0].mTimer.Second;
                mMsgs[0].mTimer.Update(Time.deltaTime);
                uint curSec = (uint)mMsgs[0].mTimer.Second;
                if (preSec != curSec)
                {
                    mContent.text += ".";
                    if (mContent.text.Length - mMsgs[0].mContent.Length > 6)
                    {
                        mContent.text = mMsgs[0].mContent;
                    }
                }
                if (mMsgs[0].mTimer.Second < 0)
                {
                    mMsgs[0].mTimer = null;
                    if (null != mMsgs[0].mTimeOutFunc)
                        mMsgs[0].mTimeOutFunc();
                    CancelMaskP(mMsgs[0].mInfotype);
                    return;
                }
            }
        }
    }
}
