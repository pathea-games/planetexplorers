using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;

public class CSUI_CompoundItem : MonoBehaviour
{
    public delegate void ClickEvent(CSUI_CompoundItem ci);

    #region UI_CONTENT

    [SerializeField]
    UISprite m_IconUI;
    [SerializeField]
    UILabel m_CountUI;
    [SerializeField]
    UISlider m_Slider;
    [SerializeField]
    GameObject m_DeleteBtn;
    [SerializeField]
    UICheckbox m_CheckBox;

    #endregion

    public object m_RefObj;

    public ClickEvent RightBtnClickEvent;

    public event System.Action<CSUI_CompoundItem> onDeleteBtnClick;

    public string IcomName
    {
        get { return m_IconUI.spriteName; }

        set { m_IconUI.spriteName = value; }
    }

    public int Count
    {
        get
        {
            if (CSUtils.IsNumber(m_CountUI.text))
            {
                int num = int.Parse(m_CountUI.text);
                return num;
            }

            m_IconUI.gameObject.SetActive(false);
            return 0;
        }

        set
        {
            if (value == 0)
                m_CountUI.gameObject.SetActive(false);
            else
                m_CountUI.gameObject.SetActive(true);

            m_CountUI.text = value.ToString();
        }
    }

    public float SliderValue
    {
        get { return m_Slider.sliderValue; }
        set
        {
            m_Slider.sliderValue = value;
        }
    }

    public bool ShowSlider
    {
        get { return m_Slider.gameObject.activeInHierarchy; }
        set
        {
            m_Slider.gameObject.SetActive(value);
        }
    }

    void CheckDeleteBtnState()
    {
        //lz-2016.11.08 只要不是空的，合成进度满没满都可以删
        if (m_CheckBox.isChecked && IcomName != "" && IcomName != "Null")
        {
            if (!m_DeleteBtn.gameObject.activeSelf)
                m_DeleteBtn.gameObject.SetActive(true);
        }
        else
        {
            if (m_DeleteBtn.gameObject.activeSelf)
                m_DeleteBtn.gameObject.SetActive(false);
        }
    }

    #region event methods
    void ItemOnClick()
    {
        if (Input.GetMouseButton(1))
        {
            if (RightBtnClickEvent != null)
                RightBtnClickEvent(this);
        }
    }

    void OnDeleteBtnClick()
    {
        if (Input.GetMouseButton(0))
        {
            if (onDeleteBtnClick != null)
                onDeleteBtnClick(this);
        }
    }

    #endregion


    #region mono methods
    void Update()
    {
        CheckDeleteBtnState();
    }

    #endregion
}
