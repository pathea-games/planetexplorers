using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// lz-2016.11.04 为了优化UIPanel子集物体负载，减少重绘次数 【优化深度为两层】
/// </summary>
public class UIPanelChildLoadBalancing : MonoBehaviour
{
    [Header("子UIPanel最大数量")]
    [SerializeField]
    private int m_ChildPanelMaxCount = 20; 

    [Header("每个子UIPanel最大的子数量")]
    [SerializeField]
    private int m_ChildPanelChildMaxCount = 10;

    [Header("每个子UIPanel的子数量正常饱和度")]
    [SerializeField]
    private float m_ChildPanelChildSaturability = 0.8f; 

    [Header("检测频率")]
    [SerializeField]
    private float m_CheckTime = 0.3f;

    [Header("优先使用新UIPanel")]
    [SerializeField]
    private bool m_FirstUseNewPanel=true;

    private List<UIPanel> m_Panels = new List<UIPanel>();
    private float m_StartTime;
    private int m_SaturabilityCount;

    #region mono methods

    void Awake()
    {
        m_StartTime = Time.realtimeSinceStartup;
        m_SaturabilityCount = Mathf.CeilToInt(m_ChildPanelChildMaxCount * m_ChildPanelChildSaturability);
    }

    void Update()
    {
        if (Time.realtimeSinceStartup - m_StartTime > m_CheckTime)
        {
            CheckChildInfo();
            m_StartTime = Time.realtimeSinceStartup;
        }
    }

    #endregion

    #region private methods

    private void CheckChildInfo()
    {
        List<Transform> trans = new List<Transform>();
        if (transform.childCount > 0)
        {
            Transform tempTrans;
            for (int i = 0; i < transform.childCount; i++)
            {
                tempTrans = transform.GetChild(i);
                if (null == tempTrans.GetComponent<UIPanel>()&&null!=tempTrans.GetComponentInChildren<UIWidget>())
                {
                    trans.Add(tempTrans);
                }
            }
        }
        if (trans.Count > 0)
        {
            for (int i = 0; i < trans.Count; i++)
            {
                UIPanel minChildPanel = null;
                if (m_Panels.Count < m_ChildPanelMaxCount)
                {
                    //面板未上限之前优先使用新面板
                    if (m_FirstUseNewPanel)
                    {
                        minChildPanel = AddNewPanel();
                    }
                    else
                    {
                        //面板未上限之前优先使用最小子集的界面
                        minChildPanel = FindMinChildPanel();
                        //最小子集饱和了就使用新界面
                        if (minChildPanel.transform.childCount >= m_SaturabilityCount)
                        {
                            minChildPanel = AddNewPanel();
                        }
                    }
                }
                else
                {
                    //面板上限后优先使用最小子集的界面
                    minChildPanel = FindMinChildPanel();
                }

                if (null != minChildPanel)
                {
                    trans[i].gameObject.name = trans[i].gameObject.name + "_" + minChildPanel.transform.childCount;
                    trans[i].parent = minChildPanel.transform;
                    if (minChildPanel.transform.childCount == m_SaturabilityCount)
                    {
                        Debug.LogFormat("<color=blue>{0} 已经进入饱和状态</color>", gameObject.name);
                    }
                }
            }
        }
    }

    private UIPanel AddNewPanel()
    {
        //添加新的面板
        GameObject go = new GameObject("UIPanel_" + m_Panels.Count);
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        go.transform.localRotation = Quaternion.identity;
        UIPanel newPanel = go.AddComponent<UIPanel>();
        m_Panels.Add(newPanel);
        return newPanel;
    }

    private UIPanel FindMinChildPanel()
    {
        UIPanel minChildPanel = null;
        int minChildCount = int.MaxValue;
        if (m_Panels.Count > 0)
        {
            for (int i = 0; i < m_Panels.Count; i++)
            {
                if (m_Panels[i].transform.childCount < minChildCount)
                {
                    minChildCount = m_Panels[i].transform.childCount;
                    minChildPanel = m_Panels[i];
                }
            }
        }
        return minChildPanel;
    }

    #endregion

}
