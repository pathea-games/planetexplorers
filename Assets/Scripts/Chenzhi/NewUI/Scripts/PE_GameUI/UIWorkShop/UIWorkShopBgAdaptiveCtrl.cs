using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIWorkShopBgAdaptiveCtrl : MonoBehaviour
{
    public Transform Bg0Go;
    public Transform Bg1Go;
    public Transform Bg2Go;
    public Transform RightGo;
    public Transform LeftGo;
    public Transform LeftBG;
    public Transform BtnGo;

    public struct PosStruct
    {
        public Vector3 Bg0GoSize;
        public Vector3 RightGoPos;
        public Vector3 LeftGoPos;
        public Vector3 LeftGoSize;
        public Vector3 LeftBgPos;
        public Vector3 LeftBgSize;
        public Vector3 BtnGoPos;
    }

    private int m_BaseColumnCount = 3;
    private PosStruct m_BasePosInfo;
    private bool m_InitData = false;

    #region private methods

    void InitBaseData()
    {
        this.m_BasePosInfo = new PosStruct();
        this.m_BasePosInfo.Bg0GoSize = this.Bg0Go.localScale;
        this.m_BasePosInfo.RightGoPos =this.RightGo.localPosition;
        this.m_BasePosInfo.LeftGoPos = this.LeftGo.localPosition;
        this.m_BasePosInfo.LeftGoSize = this.LeftGo.localScale;
        this.m_BasePosInfo.LeftBgSize = this.LeftBG.localScale;
        this.m_BasePosInfo.LeftBgPos = this.LeftBG.localPosition;
        this.m_BasePosInfo.BtnGoPos = this.BtnGo.localPosition;
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
            Vector3 halfWidth = new Vector3(width.x* 0.5f, 0, 0);

            this.Bg0Go.localScale = this.m_BasePosInfo.Bg0GoSize + width;
            this.Bg0Go.localPosition = Vector3.zero;
            this.Bg1Go.localScale = this.Bg0Go.localScale;
            this.Bg1Go.localPosition = Vector3.zero;
            this.Bg2Go.localScale = this.Bg0Go.localScale;
            this.Bg2Go.localPosition = Vector3.zero;
            this.LeftBG.localScale = this.m_BasePosInfo.LeftBgSize + width;
            this.LeftBG.localPosition = this.m_BasePosInfo.LeftBgPos;

            this.RightGo.localPosition = this.m_BasePosInfo.RightGoPos + halfWidth;
            this.LeftGo.localPosition = this.m_BasePosInfo.LeftGoPos - halfWidth;
            this.BtnGo.localPosition = this.m_BasePosInfo.BtnGoPos + halfWidth;
        }
    }
    #endregion
}
