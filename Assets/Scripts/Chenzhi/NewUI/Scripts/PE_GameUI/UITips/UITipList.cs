using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITipList : MonoBehaviour
{

    public enum EDirection
    {
        Up,
        Down
    }

    public EDirection direction = EDirection.Up;

    public int yPadding = 0;

    [SerializeField]
    UITipMsg msgPrefab;

    List<UITipMsg> m_MsgList = new List<UITipMsg>();
    List<UITipMsg> m_WaitList = new List<UITipMsg>();

    public List<UITipMsg> MsgList { get { return m_MsgList; } }

    public AnimationCurve curve;

    public float duration = 1;

    public int MaxMsgCount = 5;

    public bool Play = true;

    public delegate void DNotify();
    public event DNotify onAddShowingMsg;


    public void AddMsg(PeTipMsg peTipMsg)
    {
        UITipMsg uiTipMsg = Instantiate(msgPrefab) as UITipMsg;
        uiTipMsg.transform.parent = transform;
        uiTipMsg.transform.localScale = Vector3.one;
        uiTipMsg.transform.localPosition = Vector3.zero;


        uiTipMsg.content.text = peTipMsg.GetContent();
        uiTipMsg.content.color = peTipMsg.GetColor();
        uiTipMsg.musicID = peTipMsg.GetMusicID();
        switch (peTipMsg.GetEStyle())
        {
            case PeTipMsg.EStyle.Text:
                uiTipMsg.tex.mainTexture = null;
                uiTipMsg.icon.spriteName ="";
                break;
            case PeTipMsg.EStyle.Icon:
                uiTipMsg.icon.spriteName =peTipMsg.GetIconName();
                uiTipMsg.tex.mainTexture = null;
                break;
            case PeTipMsg.EStyle.Texture:
                 uiTipMsg.icon.spriteName ="";
                uiTipMsg.tex.mainTexture = peTipMsg.GetIconTex();
                break;
        }
        uiTipMsg.SetStyle(peTipMsg.GetEStyle());
        uiTipMsg.gameObject.SetActive(false);

        m_WaitList.Add(uiTipMsg);

        if (GameUI.Instance.mTipRecordsMgr != null)
            GameUI.Instance.mTipRecordsMgr.AddMsg(peTipMsg);

    }

    public void ClearMsg()
    {
        foreach (UITipMsg msg in m_MsgList)
        {
            Destroy(msg.gameObject);
        }

        foreach (UITipMsg msg in m_WaitList)
        {
            Destroy(msg.gameObject);
        }


        m_MsgList.Clear();
        m_WaitList.Clear();
    }

    float EvaluateOffset(float t)
    {
        return curve.Evaluate(t) * m_MotionOffset;
    }

    #region UINITY_INNER_FUNC


    void OnGUI()
    {

    }

    void Awake()
    {
    }

    List<float> m_DefaultHeight = new List<float>();
    float m_CurDura = 0;
    float m_MotionOffset;

    bool m_PlayEnd = true;

    void Update()
    {
        //		if (direction == EDirection.Up)
        //		{
        //			for (int i = 0; i < m_MsgList.Count; i++)
        //			{
        //				m_MsgList[i].defaultAlpha =  Mathf.Pow( (i + 1) / (float)m_MsgList.Count, 2);
        //				m_MsgList[i].content.alpha = m_MsgList[i].defaultAlpha * m_MsgList[i].alpha;
        //			}
        //		}
        //		else if (direction == EDirection.Down)
        //		{
        //			for (int i = 0; i < m_MsgList.Count; i++)
        //			{
        //				m_MsgList[i].defaultAlpha = Mathf.Pow(  (m_MsgList.Count - i) / (float)m_MsgList.Count, 2);
        //				m_MsgList[i].content.alpha = m_MsgList[i].defaultAlpha * m_MsgList[i].alpha;
        //			}
        //		}


        if (Play)
        {
            if (direction == EDirection.Up)
            {
                if (duration + 0.1f > m_CurDura)
                {
                    m_CurDura += Time.deltaTime;
                    float adder = EvaluateOffset(Mathf.Clamp(m_CurDura / duration, 0, 1));

                    for (int i = 0; i < m_MsgList.Count; i++)
                    {
                        Vector3 pos = m_MsgList[i].transform.localPosition;
                        m_MsgList[i].transform.localPosition = new Vector3(pos.x, Mathf.Round(m_DefaultHeight[i] + adder), pos.z);
                    }

                    m_PlayEnd = true;
                }
                else
                {

                    for (int i = m_MsgList.Count - 1; i >= 0; i--)
                        m_DefaultHeight[i] = m_MsgList[i].transform.localPosition.y;

                    if (m_PlayEnd && m_WaitList.Count == 0)
                    {
                        m_PlayEnd = false;
                    }

                    while (m_WaitList.Count != 0)
                    {
                        UITipMsg ui_msg = m_WaitList[0];
                        m_WaitList.RemoveAt(0);
                        _addMsgDirUp(ui_msg);
                        PlayAudio(ui_msg.musicID);
                        break;
                    }


                    if (m_MsgList.Count > 5)
                    {
                        Destroy(m_MsgList[0].gameObject);
                        m_MsgList.RemoveAt(0);
                        m_DefaultHeight.RemoveAt(0);
                    }

                }
            }
            else if (direction == EDirection.Down)
            {
                if (duration + 0.1f > m_CurDura)
                {
                    m_CurDura += Time.deltaTime;
                    float adder = EvaluateOffset(Mathf.Clamp(m_CurDura / duration, 0, 1));

                    for (int i = 0; i < m_MsgList.Count; i++)
                    {
                        Vector3 pos = m_MsgList[i].transform.localPosition;
                        m_MsgList[i].transform.localPosition = new Vector3(pos.x, Mathf.Round(m_DefaultHeight[i] + adder), pos.z);
                    }

                    m_PlayEnd = true;
                }
                else
                {
                    for (int i = m_MsgList.Count - 1; i >= 0; i--)
                        m_DefaultHeight[i] = m_MsgList[i].transform.localPosition.y;

                    if (m_PlayEnd && m_WaitList.Count == 0)
                    {
                        m_PlayEnd = false;
                    }

                    while (m_WaitList.Count != 0)
                    {
                        UITipMsg ui_msg = m_WaitList[0];
                        m_WaitList.RemoveAt(0);
                        _addMsgDirDown(ui_msg);
                        PlayAudio(ui_msg.musicID);
                        break;
                    }


                    if (m_MsgList.Count > 5)
                    {
                        Destroy(m_MsgList[m_MsgList.Count - 1].gameObject);
                        m_MsgList.RemoveAt(m_MsgList.Count - 1);
                        m_DefaultHeight.RemoveAt(m_MsgList.Count - 1);
                    }
                }
            }
        }
    }

    void _addMsgDirUp(UITipMsg ui_msg)
    {
        ui_msg.gameObject.SetActive(true);
        Bounds bound = ui_msg.GetBounds();
        if (m_MsgList.Count == 0)
        {
            ui_msg.transform.localPosition = new Vector3(0, 0, 0);
        }
        else
        {
            UITipMsg prev_ui = m_MsgList[m_MsgList.Count - 1];
            Bounds prv_bound = prev_ui.GetBounds();
            ui_msg.transform.localPosition = new Vector3(0, prev_ui.transform.localPosition.y - prv_bound.size.y - yPadding, 0);
        }

        m_MotionOffset = bound.size.y + yPadding;
        m_DefaultHeight.Add(ui_msg.transform.localPosition.y);

        m_CurDura = 0;

        m_MsgList.Add(ui_msg);

        if (onAddShowingMsg != null)
            onAddShowingMsg();
    }

    void _addMsgDirDown(UITipMsg ui_msg)
    {
        ui_msg.gameObject.SetActive(true);

        Bounds bound = ui_msg.GetBounds();

        if (m_MsgList.Count == 0)
        {
            ui_msg.transform.localPosition = new Vector3(0, bound.size.y, 0);
        }
        else
        {
            UITipMsg prev_ui = m_MsgList[0];
            //			Bounds prv_bound = prev_ui.GetBounds();
            ui_msg.transform.localPosition = new Vector3(0, prev_ui.transform.localPosition.y + bound.size.y + yPadding, 0);
        }

        m_MotionOffset = -bound.size.y - yPadding;
        m_MsgList.Insert(0, ui_msg);
        m_DefaultHeight.Insert(0, ui_msg.transform.localPosition.y);

        m_CurDura = 0;

        if (onAddShowingMsg != null)
            onAddShowingMsg();
    }

    void PlayAudio(int musicID)
    {
        if (musicID != -1)
        {
            AudioManager.instance.Create(Vector3.zero, musicID);
        }
    }

    #endregion

}
