using UnityEngine;
using System.Collections;

public class CSUI_BuildingNum : MonoBehaviour
{
    [SerializeField]
    private UILabel m_Label;

    public string m_Description;

    public int m_Count;

    public int m_LimitCnt;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (m_LimitCnt != 0)
            m_Label.text = m_Description + "  " + m_Count.ToString() + " / " + m_LimitCnt.ToString();
        else
            m_Label.text = "[858585]" + m_Description + "  " + m_Count.ToString() + " / " + m_LimitCnt.ToString() + "[-]";
    }
}
