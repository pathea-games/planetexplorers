using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITalkwithctr : MonoBehaviour
{
    static UITalkwithctr mInstance;
    public static UITalkwithctr Instance { get { return mInstance; } }
    // Use this for initialization
    public UIInput mMsgText;
    public UITalkBoxCtrl mTalkBoxControl;
    public UIAnchor mTalkBoxAnchor;

    List<string> mTalkNameColorList = new List<string>();

    


    void Awake()
    {
        if (Pathea.PeGameMgr.IsMulti)
        {
            this.gameObject.SetActive(true);
            mTalkBoxAnchor.depthOffset = 1.2f;
            //lz-2016.07.28 输入框选中的时候，聊天窗口在前面，没有选中的时候在后面
            UIEventListener.Get(mMsgText.gameObject).onSelect += (go, isSelect) =>
            {
                mTalkBoxAnchor.depthOffset = isSelect ? 0.2f : 1.2f;
            };
        }
        else
            this.gameObject.SetActive(false);
        InitTalkNameColorList();
        mInstance = this;
    }

    public bool isShow
    {
        get
        {
            return gameObject.activeInHierarchy;
        }
    }

    public void ShowMenu()
    {
        if (Pathea.PeGameMgr.IsMulti)
        {
            this.gameObject.SetActive(true);
            mMsgText.selected = true;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }

    public void AddTalk(string name, string content, string colorStr)
    {
        if (mTalkBoxControl == null)
            return;

        mTalkBoxControl.AddMsg(name, content, colorStr);
    }


    void SendMsg()
    {
        if (mMsgText.text == "")
            return;

        string chatStr = mMsgText.text;
        //lz-2016.12.9 发消息的时候加入语言识别，方便显示处理
        chatStr += SystemSettingData.Instance.IsChinese ? UITalkBoxCtrl.LANGE_CN : UITalkBoxCtrl.LANGE_OTHER;

        PlayerNetwork.mainPlayer.RequestSendMsg(CustomData.EMsgType.ToAll,chatStr);

        mMsgText.text = "";

        //string msg = "";
        //PlayerNetwork.MainPlayer.RequestSendMsg(CustomData.EMsgType.ToAll, msg);
        Invoke("GetInputCursor", 0.1f);
    }

    void GetInputCursor()
    {
        mMsgText.selected = true;
    }

    void Recive()
    {
        //PlayerNetwork.();
    }

    void BtnInputOnClick()
    {
        //StartTime = 0;
        SendMsg();
    }


    void OnSubmit(string inputString)
    {
        //
        //StartTime = 0;
        SendMsg();
    }
    // Update is called once per frame

    //float StartTime = 0;

    void Update()
    {

        if (mMsgText.selected)
        {
            //StartTime = 0;
            GameUI.Instance.IsInput = true;
        }
        else
        {
            //StartTime += Time.deltaTime;

            //if (StartTime > 10)
            //{
            //    this.gameObject.SetActive(false);
            //}

            GameUI.Instance.IsInput = false;
        }

        //int width = Screen.width;
        //if (width > 1920)
        //    width = 1920;
        //int screen_x = (1920 - width) / 2;
        //if (gameObject.transform.localPosition.x != screen_x)
        //{
        //    Vector3 pos = gameObject.transform.localPosition;
        //    gameObject.transform.localPosition = new Vector3(screen_x, pos.y, pos.z);
        //}

    }

    void InitTalkNameColorList()
    {
        mTalkNameColorList.Clear();
        mTalkNameColorList.Add("FFB6C1");
        mTalkNameColorList.Add("DC143C");
        mTalkNameColorList.Add("DB7093");
        mTalkNameColorList.Add("FF69B4");
        mTalkNameColorList.Add("FF1493");
        mTalkNameColorList.Add("DA70D6");
        mTalkNameColorList.Add("FF00FF");
        mTalkNameColorList.Add("8B008B");
        mTalkNameColorList.Add("8A2BE2");
        mTalkNameColorList.Add("7B68EE");
        mTalkNameColorList.Add("0000FF");
        mTalkNameColorList.Add("00008B");
        mTalkNameColorList.Add("778899");
        mTalkNameColorList.Add("00BFFF");
        mTalkNameColorList.Add("5F9EA0");
        mTalkNameColorList.Add("00FFFF");
        mTalkNameColorList.Add("008B8B");
        mTalkNameColorList.Add("F5FFFA");
        mTalkNameColorList.Add("228B22");
        mTalkNameColorList.Add("FFFF00");
        mTalkNameColorList.Add("FFD700");
        mTalkNameColorList.Add("FF8C00");
        mTalkNameColorList.Add("BC8F8F");
        mTalkNameColorList.Add("A52A2A");
        mTalkNameColorList.Add("B0E0E6");
        mTalkNameColorList.Add("00CED1");
        mTalkNameColorList.Add("40E0D0");
        mTalkNameColorList.Add("2E8B57");
        mTalkNameColorList.Add("8FBC8F");
        mTalkNameColorList.Add("ADFF2F");
        mTalkNameColorList.Add("FAFAD2");
        mTalkNameColorList.Add("BDB76B");
    }

}
