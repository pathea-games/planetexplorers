using UnityEngine;
using System.Collections;

public class ClickBtnInGameAidCmpt : MonoBehaviour
{
    [SerializeField]
    private int m_BtnID=0;

    #region mono methods
    void Awake()
    {
        UIEventListener.Get(this.gameObject).onClick += (go) =>
        {
            if (this.m_BtnID != 0)
            {
                InGameAidData.CheckClickBtn(this.m_BtnID);
            }
        };
    }
    #endregion

    #region public methods

    public void SetBtnID(int id)
    {
        this.m_BtnID = id; 
    }
    #endregion

}
