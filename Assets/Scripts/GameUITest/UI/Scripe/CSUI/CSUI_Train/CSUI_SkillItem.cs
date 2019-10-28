using UnityEngine;
using System.Collections;
using SkillSystem;
using Pathea;

public class CSUI_SkillItem : MonoBehaviour
{

    [SerializeField]
    UISlicedSprite skillIcon;
    [SerializeField]
    GameObject deleteBtn;

    [HideInInspector]
    public int m_Index = -1;//用于表示当前技能所在位置的索引

    [HideInInspector]
    public bool _ableToClick = true;

    //[HideInInspector]
    //public bool mCanShooseSkill = false;

    public event System.Action<NpcAbility> onLeftMouseClicked;

    private bool _active = false;
    public bool Active
    {
        get { return _active; }
        set
        {
            _active = value;
            if (value)
                deleteBtn.SetActive(value);
        }
    }

    NpcAbility _localSkill;
    public void SetSkill(string _icon, NpcAbility _skill = null)
    {
        _localSkill = _skill;
        skillIcon.spriteName = _icon;
    }

    public void SetIcon(string _name)
    {
        skillIcon.spriteName = _name;
    }
    public void DeleteIcon(NpcAbility _skill = null)
    {
        _localSkill = _skill;
        skillIcon.spriteName = "Null";
    }

    public delegate void SkillGridEvent(CSUI_SkillItem skillGrid);
    public SkillGridEvent OnDestroySelf;

    void OnDeleteBtn()
    {
        if (OnDestroySelf != null)
            OnDestroySelf(this);
        OnHideBtn();
    }

    public void OnHideBtn()
    {
        deleteBtn.SetActive(false);
    }
    void OnShowBtn()
    {
        CSUI_TrainMgr.Instance.HideAllDeleteBtn();
        if (skillIcon.spriteName != "Null" && _ableToClick)
            deleteBtn.SetActive(true);
    }
	void Awake(){
		skillIcon.spriteName = "Null";
		deleteBtn.SetActive(false);
	}

    void Start()
    {
    }

    void Update()
    {

    }

    void OnClick()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (onLeftMouseClicked != null)
            {
                onLeftMouseClicked(_localSkill);
            }
        }
    }

    void OnTooltip(bool show)
    {
        if (show && skillIcon.spriteName != "Null")
        {
            string _desc = PELocalization.GetString(_localSkill.desc);
            ToolTipsMgr.ShowText(_desc);
        }
        else
        {
            ToolTipsMgr.ShowText(null);
        }
    }
}
