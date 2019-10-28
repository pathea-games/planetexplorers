using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIWorkShopPage0AdaptiveCtrl : MonoBehaviour
{
    public Transform TopGo;
    public Transform BottomGo;
    public Transform LeftGo;
    public Transform RightGo;
    public Transform GridBg0;
    public Transform GridBg1;
    public UIGrid Grid;

    public struct PosStruct
    {
        public Vector3 TopGoPos;
        public Vector3 BottomGoPos;
        public Vector3 LeftGoPos;
        public Vector3 RightGoPos;
        public Vector3 GridPos;
        public Vector3 GridBgSize;
        public Vector3 GridBgPos;
    }

    private int m_BaseColumnCount = 3;
    //private int m_BaseRowCount = 3;
    private bool m_InitData=false;

    private PosStruct m_BasePosInfo;

    #region private methods

    void InitBaseData()
    {
        this.m_BasePosInfo = new PosStruct();
        this.m_BasePosInfo.TopGoPos = this.TopGo.localPosition;
        this.m_BasePosInfo.BottomGoPos = this.BottomGo.localPosition;
        this.m_BasePosInfo.LeftGoPos = this.LeftGo.localPosition;
        this.m_BasePosInfo.RightGoPos = this.RightGo.localPosition;
        this.m_BasePosInfo.GridPos = this.Grid.transform.localPosition;
        this.m_BasePosInfo.GridBgSize = this.GridBg0.localScale;
        this.m_BasePosInfo.GridBgPos = this.GridBg0.localPosition;
        this.m_InitData = true;
    }

    #endregion

    #region public methods

    public void UpdateSizeByScreen(int columnCount,int gridWidth)
    {
        if (m_BaseColumnCount != columnCount)
        {
            if (!this.m_InitData) this.InitBaseData();

            Vector3 width = new Vector3((columnCount - m_BaseColumnCount) * gridWidth, 0, 0);
            Vector3 halfWidth = new Vector3(width.x * 0.5f, 0, 0);

            this.TopGo.localPosition = this.m_BasePosInfo.TopGoPos - halfWidth;
            this.BottomGo.localPosition = this.m_BasePosInfo.BottomGoPos - halfWidth;
            this.RightGo.localPosition = this.m_BasePosInfo.RightGoPos + halfWidth;
            this.LeftGo.localPosition = this.m_BasePosInfo.LeftGoPos - halfWidth;
            this.GridBg0.localScale = this.m_BasePosInfo.GridBgSize + width;
            this.GridBg0.localPosition = this.m_BasePosInfo.GridBgPos;
            this.GridBg1.localPosition = this.GridBg0.localPosition;
            this.GridBg1.localScale = this.GridBg0.localScale;
            this.Grid.transform.localPosition = this.m_BasePosInfo.GridPos - halfWidth;
        }
    }
    #endregion
}
