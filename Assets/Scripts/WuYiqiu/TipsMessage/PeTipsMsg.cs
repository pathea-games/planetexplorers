using UnityEngine;
using System.Collections;


public class PeTipMsg
{
    public enum EMsgType
    {
        Misc,
        Stroy,
        Colony,

    }

    public enum EMsgLevel
    {
        Norm, // white
        Warning, // yellow 
        Error, 	 // red
        HighLightRed
    }

    public enum EStyle
    {
        Text,
        Icon,
        Texture
    }

    public string GetIconName()
    {
        return m_IconName;
    }

    public Texture GetIconTex()
    {
        return m_IconTex;
    }

    public string GetContent()
    {
        return m_Content;
    }

    public Color GetColor()
    {
        return m_Color;
    }

    public int GetMusicID()
    {
        return m_MusicID;
    }

    public EStyle GetEStyle()
    {
        return m_Style;
    }

    static Color s_NormalColor = Color.white;
    static Color s_WarningColor = Color.yellow;
    static Color s_ErrorColor = new Color(0.99f, 0.30f, 0.30f, 1f); //lz-2016.10.23 吴哥说红色看的不太清，改成亮红色
    static Color s_HighLightRed = new Color(1f, 0.35f, 0.35f, 1f);

    string m_IconName;
    string m_Content;
    Texture m_IconTex;
    EMsgType m_MsgType;
    Color m_Color;
    int m_MusicID;
    private EStyle m_Style = EStyle.Text;

    public EMsgType MsgType { get { return m_MsgType; } }

    public static void Register(string content, string iconName, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
    {
        new PeTipMsg(content, iconName, level, msgType, musicID);
    }

    public static void Register(string content, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
    {
        new PeTipMsg(content, level, msgType, musicID);
    }

    public static void Register(string content, Texture iconTex, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
    {
        new PeTipMsg(content, iconTex, level, msgType, musicID);
    }

    public PeTipMsg(string content, string iconName, EMsgLevel level, EMsgType msgType = EMsgType.Misc,int musicID=-1)
    {
        m_IconName = iconName;
        m_Content = content;
        m_MsgType = msgType;
        m_MusicID = musicID;
        m_Style = EStyle.Icon;

        if (level == EMsgLevel.Norm)
            m_Color = s_NormalColor;
        else if (level == EMsgLevel.Warning)
            m_Color = s_WarningColor;
        else if (level == EMsgLevel.Error)
            m_Color = s_ErrorColor;
        else if (level == EMsgLevel.HighLightRed)
            m_Color = s_HighLightRed;

        PeTipsMsgMan.Instance.AddTipMsg(this);
    }

    public PeTipMsg(string content, EMsgLevel level, EMsgType msgType = EMsgType.Misc, int musicID = -1)
    {
        m_IconName = "";
        m_Content = content;
        m_MsgType = msgType;
        m_MusicID = musicID;
        m_Style = EStyle.Text;

        if (level == EMsgLevel.Norm)
            m_Color = s_NormalColor;
        else if (level == EMsgLevel.Warning)
            m_Color = s_WarningColor;
        else if (level == EMsgLevel.Error)
            m_Color = s_ErrorColor;
        else if (level == EMsgLevel.HighLightRed)
            m_Color = s_HighLightRed;

        PeTipsMsgMan.Instance.AddTipMsg(this);
    }

    public PeTipMsg(string content, Texture iconTex, EMsgLevel level, EMsgType msgType = EMsgType.Misc,int musicID=-1)
    {
        m_IconTex = iconTex;
        m_Content = content;
        m_MsgType = msgType;
        m_MusicID = musicID;
        m_Style = EStyle.Texture;

        if (level == EMsgLevel.Norm)
            m_Color = s_NormalColor;
        else if (level == EMsgLevel.Warning)
            m_Color = s_WarningColor;
        else if (level == EMsgLevel.Error)
            m_Color = s_ErrorColor;
        else if (level == EMsgLevel.HighLightRed)
            m_Color = s_HighLightRed;

        PeTipsMsgMan.Instance.AddTipMsg(this);
    }
}