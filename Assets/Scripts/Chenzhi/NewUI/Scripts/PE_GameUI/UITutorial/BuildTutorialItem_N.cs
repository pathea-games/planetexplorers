using UnityEngine;
using System.Collections;

public class BuildTutorialItem_N : MonoBehaviour
{
    [SerializeField]
    private TweenScale m_Tween;
    [SerializeField]
    private UISprite m_BgSprite;
    [SerializeField]
    private UILabel m_ContentLbl;
    [SerializeField]
    private int m_ContentID=-1;

    private bool m_Forword = false;
    public bool IsShow = false;


    #region mono methods

    void Awake()
    {
        Init();
    }
    #endregion

    #region private methods

    private void Init()
    {
        UpdateContent();
        if (null != m_Tween)
        {
            m_Tween.onFinished = TweenFinish;
        }
    }

    private void UpdateContent()
    {
        if (m_ContentID != -1)
        {
            m_ContentLbl.text = PELocalization.GetString(m_ContentID);
            m_ContentLbl.MakePixelPerfect();
            AdjustContentBg();
        }
    }

    private void AdjustContentBg()
    {
        Vector3 scale= m_BgSprite.transform.localScale;
        scale.y = m_ContentLbl.relativeSize.y * m_ContentLbl.font.size + 26;
        m_BgSprite.transform.localScale = scale;
    }

    private void TweenFinish(UITweener tween)
    {
        IsShow = m_Forword;
        gameObject.SetActive(IsShow);
    }

    #endregion

    #region public methods

    public void ShowTween(bool show)
    {
        if (null != m_Tween)
        {
            gameObject.SetActive(true);
            m_Tween.Play(show);
            m_Forword = show;
        }
    }

    public float GetTweenTime()
    {
        return m_Tween.duration;
    }

    #endregion

}
