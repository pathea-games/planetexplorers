using UnityEngine;
using System.Collections;
using System;

public class UIWndTutorialTip_N : MonoBehaviour
{
    [SerializeField]
    private TweenAlpha m_HideTween;
    [SerializeField]
    private UISprite m_ContentBg;
    [SerializeField]
    private UILabel m_ContentLb;
    [SerializeField]
    private int m_ContentID = -1;
    [SerializeField]
    private float m_WaitHideTime = 5f;
    public Action DeleteEvent=null;

    #region mono methods
    void Start()
    {
        m_ContentLb.text = PELocalization.GetString(m_ContentID);
        m_ContentLb.MakePixelPerfect();
        Vector3 scale = m_ContentBg.transform.localScale;
        scale.y = m_ContentLb.relativeSize.y * m_ContentLb.font.size + 26;
        m_ContentBg.transform.localScale = scale;
        gameObject.SetActive(true);
        StartCoroutine("HideTutorialTipIterator");
    }
    #endregion

    #region private methods
    private IEnumerator HideTutorialTipIterator()
    {
        float startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < m_WaitHideTime)
        {
            yield return null;
        }
        m_HideTween.Play(true);
        startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < m_HideTween.duration )
        {
            yield return null;
        }

        if (null != DeleteEvent)
        {
            DeleteEvent();
        }
        Destroy(gameObject);
    }
    #endregion
}
