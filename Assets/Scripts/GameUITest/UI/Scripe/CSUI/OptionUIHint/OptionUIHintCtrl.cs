using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptionUIHintCtrl : MonoBehaviour
{

    [SerializeField]
    OptionUIHintItem mPrefab;
    [SerializeField]
    Transform mParent;

    public float duration = 1;
    float m_CurDura = 0;
    public AnimationCurve curve;
    float m_MotionOffset;
    bool m_PlayEnd = true;
    public int yPadding = 0;
    List<float> m_DefaultHeight = new List<float>();

    List<OptionUIHintItem> m_MsgList = new List<OptionUIHintItem>(10);
    List<OptionUIHintItem> m_WaitList = new List<OptionUIHintItem>(10);

    public List<OptionUIHintItem> MsgList { get { return m_MsgList; } }

    public delegate void DNotify();
    public event DNotify onAddShowingMsg;

    public void AddOneHint(string _content)
    {
        OptionUIHintItem go = GoCreat();
        go.SetHintInfo(_content);
        m_WaitList.Add(go);
    }

    OptionUIHintItem GoCreat()
    {
        OptionUIHintItem item = Instantiate(mPrefab) as OptionUIHintItem;
        item.transform.parent = mParent;
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = Vector3.one;
        item.gameObject.SetActive(false);
        return item;
    }

    void Update()
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
                OptionUIHintItem ui_msg = m_WaitList[0];
                m_WaitList.RemoveAt(0);
                _addMsgDirDown(ui_msg);
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

    float EvaluateOffset(float t)
    {
        return curve.Evaluate(t) * m_MotionOffset;
    }

    void _addMsgDirDown(OptionUIHintItem ui_msg)
    {
        ui_msg.gameObject.SetActive(true);

        Bounds bound = ui_msg.GetBounds();

        if (m_MsgList.Count == 0)
        {
            ui_msg.transform.localPosition = new Vector3(0, bound.size.y, 0);
        }
        else
        {
            OptionUIHintItem prev_ui = m_MsgList[0];
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

    public void ClearMsg()
    {
        foreach (OptionUIHintItem msg in m_MsgList)
        {
            Destroy(msg.gameObject);
        }

        foreach (OptionUIHintItem msg in m_WaitList)
        {
            Destroy(msg.gameObject);
        }

        m_MsgList.Clear();
        m_WaitList.Clear();
    }
}
