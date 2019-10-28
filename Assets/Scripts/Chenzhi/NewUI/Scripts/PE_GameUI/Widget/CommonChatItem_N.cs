using UnityEngine;
using System.Collections;

public class CommonChatItem_N : MonoBehaviour
{
    [SerializeField]
    private UILabel m_ContentLb;
    [SerializeField]
    UIFont m_ChineseFont;
    [SerializeField]
    UIFont m_OtherFont;

    #region public methods

    public void UpdateText(bool isChinese, string info)
    {
        m_ContentLb.font = isChinese ? m_ChineseFont : m_OtherFont;
        m_ContentLb.text = info;
        m_ContentLb.MakePixelPerfect();
    }

    public void SetLineWidth(int width)
    {
        m_ContentLb.lineWidth = width;
        m_ContentLb.MakePixelPerfect();
    }

    public void ResetItem()
    {
        m_ContentLb.text = string.Empty;
    }

    #endregion

}
