using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptionUIHintFadeCtrl : MonoBehaviour
{

    static OptionUIHintFadeCtrl mInstance;
    public static OptionUIHintFadeCtrl Instance { get { return mInstance; } }

    [SerializeField]
    OptionUIHintCtrl mHintCtrl;
    [SerializeField]
    List<UISprite> spriteList;

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


    float m_CurTime = 0;
    float m_CurKeepOutTime = 0;

    public float fadeTime = 2;
    public float bgDefaultAlpha = 0.5f;
    public float keepOutTime = 10;

    public AnimationCurve curve;
    bool m_AwalysOn = false;

    public void AddOneHint(string _content)
    {
        mHintCtrl.AddOneHint(_content);
    }

    void SetAlpha(float t)
    {
        float alpha = curve.Evaluate(t);
        //foreach (UISprite spr in spriteList)
        //{
        //    spr.alpha = alpha * bgDefaultAlpha;
        //}

        foreach (OptionUIHintItem msg in mHintCtrl.MsgList)
        {
            msg.mLabel.alpha = alpha;
        }
    }

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

    void Awake()
    {
        mInstance = this;

        if (!m_AwalysOn)
        {
            m_Phase = EPhase.fadeOut;
        }

        SetAlpha(bgDefaultAlpha);
        mHintCtrl.onAddShowingMsg += OnTipListAddMsg;
    }

    void OnDestroy()
    {
        mHintCtrl.onAddShowingMsg -= OnTipListAddMsg;
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    AddOneHint("This Is Test Message");
        //}

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

                mHintCtrl.ClearMsg();
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
}
