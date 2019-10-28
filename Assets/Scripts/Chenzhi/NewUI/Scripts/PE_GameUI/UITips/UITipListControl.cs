//#define TEXT_CODE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UITipListControl : MonoBehaviour
{
    [SerializeField]
    UISlicedSprite trumptNormSprite;
    [SerializeField]
    UISlicedSprite trumptSelectSprite;
    [SerializeField]
    List<UISprite> spriteList;
    [SerializeField]
    UITipList tipList;

    public UITipList TipList { get { return tipList; } }

    public float bgDefaultAlpha = 0.5f;

    public float keepOutTime = 10;

    public float fadeTime = 2;

    public AnimationCurve curve;

    float m_CurTime = 0;
    float m_CurKeepOutTime = 0;


    enum EPhase
    {
        none,
        keep,
        fadeIn,
        fadeOut,
        Clear
    }

    EPhase m_Phase = EPhase.none;
    EPhase m_FadeInNextPhase = EPhase.none;

    //float m_AddMsgInterval = 0;
    //bool m_AddMsg = false;
    public void AddMsg(PeTipMsg tipMsg)
    {
        tipList.AddMsg(tipMsg);
    }

    #region UNITY_INNER_FUNC

#if TEXT_CODE
	string  _testStr = "";

	void OnGUI ()
	{
		_testStr = GUI.TextField(new Rect(100, 100, 200, 50), _testStr);
		if (GUI.Button(new Rect(100, 300, 100, 50), "Add Tips"))
		{
			AddMsg(_testStr, "", Color.white);

		}

		if (GUI.Button(new Rect(100, 400, 100, 50), "Add Tips"))
		{
			tipList.AddMsg(_testStr, Color.white);
			
		}
	}
#endif

    void Awake()
    {
        if (m_AwalysOn)
        {
            trumptNormSprite.gameObject.SetActive(false);
            trumptSelectSprite.gameObject.SetActive(true);
        }
        else
        {
            trumptNormSprite.gameObject.SetActive(true);
            trumptSelectSprite.gameObject.SetActive(false);
            m_Phase = EPhase.fadeOut;
        }

        SetAlpha(bgDefaultAlpha);

        tipList.onAddShowingMsg += OnTipListAddMsg;
    }

    void OnDestroy()
    {
        tipList.onAddShowingMsg -= OnTipListAddMsg;
    }

    void Update()
    {
        if (m_Phase == EPhase.fadeIn)
        {
            m_CurTime += Time.deltaTime;

            SetAlpha(Mathf.Clamp01(m_CurTime / fadeTime));

            if (m_CurTime > fadeTime)
            {
                m_Phase = m_FadeInNextPhase;
                m_CurTime = 0;
            }
        }
        else if (m_Phase == EPhase.fadeOut)
        {
            m_CurTime += Time.deltaTime;

            SetAlpha(Mathf.Clamp01(1 - m_CurTime / fadeTime));

            if (m_CurTime > fadeTime)
            {
                m_Phase = EPhase.none;
                m_CurTime = 0;

                tipList.ClearMsg();
            }

        }
        else if (m_Phase == EPhase.keep)
        {
            m_CurKeepOutTime += Time.deltaTime;
            m_CurTime = fadeTime;

            if (m_CurKeepOutTime > keepOutTime)
            {
                m_CurKeepOutTime = 0;
                m_CurTime = 0;
                m_Phase = EPhase.fadeOut;
            }
        }
        else if (m_Phase == EPhase.Clear)
        {
            m_Phase = EPhase.none;
        }


    }

    #endregion

    #region HELP_FUNC

    void SetAlpha(float t)
    {
        float alpha = curve.Evaluate(t);
        foreach (UISprite spr in spriteList)
        {
            spr.alpha = alpha * bgDefaultAlpha;
        }

        foreach (UITipMsg msg in tipList.MsgList)
        {
            msg.content.alpha = alpha;
            msg.icon.alpha = alpha;
            msg.tex.alpha = alpha;
        }
    }



    #endregion

    #region CALL_BACK

    void OnTipListAddMsg()
    {
        if (!m_AwalysOn)
        {
            if (m_Phase == EPhase.fadeOut)
                m_CurTime = Mathf.Max(0, fadeTime - m_CurTime);

            m_Phase = EPhase.fadeIn;
            m_FadeInNextPhase = EPhase.keep;
            m_CurKeepOutTime = 0;

        }
    }


    //	bool m_MouseOn = false;
    //	void OnTipListMouseOver ()
    //	{
    //		m_MouseOn = true;
    //		if (m_AwalysOn)
    //			return;
    //
    //
    //		if (m_Phase == EPhase.fadeOut)
    //		{
    //			m_CurTime = Mathf.Max(0, fadeTime - m_CurTime);
    //			Debug.Log(m_CurTime);
    //		}
    //
    //		m_Phase = EPhase.fadeIn;
    //		m_FadeInNextPhase = EPhase.none;
    //	}
    //
    //	void OnTipListMouseOut ()
    //	{
    //		m_MouseOn = false;
    //		if (m_AwalysOn)
    //			return;
    //
    //		if (m_Phase == EPhase.fadeIn)
    //			m_CurTime = Mathf.Max(0, fadeTime - m_CurTime);
    //
    //			m_Phase = EPhase.fadeOut;
    //	}
    //
    bool m_AwalysOn = false;

    void OnTrumpetBtnClick()
    {
        //m_AwalysOn = !m_AwalysOn;
        //if (m_AwalysOn)
        //{
        //    trumptNormSprite.gameObject.SetActive(false);
        //    trumptSelectSprite.gameObject.SetActive(true);
        //}
        //else
        //{
        //    trumptNormSprite.gameObject.SetActive(true);
        //    trumptSelectSprite.gameObject.SetActive(false);
        //}

        if (GameUI.Instance != null && GameUI.Instance.mTipRecordsMgr != null)
            GameUI.Instance.mTipRecordsMgr.ChangeWindowShowState();

    }

    #endregion
}
