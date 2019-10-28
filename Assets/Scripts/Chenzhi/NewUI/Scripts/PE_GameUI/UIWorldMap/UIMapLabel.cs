using UnityEngine;
using System.Collections;
using System;
using Pathea;
using PeUIMap;

public class UIMapLabel : MonoBehaviour
{
    [SerializeField]
    UISprite mSpr;
    [SerializeField]
    UIButton mButton;
    [SerializeField]
    UISprite mFriendSpr;
    [SerializeField]
    BoxCollider mBoxCollider;
    [SerializeField]
    //float mBigMapIconRangeScale = 1.5f;

    private float m_WorldMapConvert; // Distanse with one pixel
    private PeMap.ILabel m_Label;
    private bool inMinMap;
    private const int m_MinRadius = 30; //lz-2016.09.30 修改任务范围最大最小值
    private const int m_MaxRadius = 150;
    private int m_NpcID;
    private Color m_Col;


    public PeMap.ILabel _ILabel { get { return m_Label; } }
    public PeMap.ELabelType type { get { return m_Label.GetType(); } }
    public bool fastTrval { get { return m_Label.FastTravel(); } }
    public Vector3 worldPos { get { return m_Label.GetPos(); } }
    public string descText { get { return m_Label.GetText(); } }
    public int NpcID { get { return m_NpcID; } }
    public string iconStr
    {
        get
        {
            PeMap.MapIcon mapIcon = PeMap.MapIcon.Mgr.Instance.iconList.Find(itr => (itr.id == m_Label.GetIcon()));
            return mapIcon != null ? mapIcon.iconName : "";
        }
    }
    public PeMap.EShow eShow { get { return m_Label.GetShow(); } }

    public delegate void OnMouseOverEvent(UIMapLabel sender, bool isOver);
    public event OnMouseOverEvent e_OnMouseOver;
    public delegate void OnClickEvent(UIMapLabel sender);
    public event OnClickEvent e_OnClick;

    //lz-2016.08.31 显示联盟友好度
    public static string[] FriendlyLevelIconName = new string[]
    {
        "FriendlyLevel_bad",
        "FriendlyLevel_nomal",
        "FriendlyLevel_good"
    };


    public void Init()
    {
        this.e_OnMouseOver = null;
        this.e_OnClick = null;
        this.m_WorldMapConvert = 1f;
        this.m_Label = null;
        this.inMinMap = false;
        this.m_NpcID = -1;
        SetColor(new Color32(255, 255, 255, 255));
        SetEnableClick(true);
        SetFriendlyLevelIcon(-1);
        mSpr.depth = 0;
        transform.localRotation = Quaternion.identity;
    }

    public bool Openable
    {
        get
        {
            if (m_Label == null)
                return false;
            return !(type == PeMap.ELabelType.Npc || type == PeMap.ELabelType.Vehicle
                     || type == PeMap.ELabelType.Mark);
        }
    }

    public void SetLabel(PeMap.ILabel _label, bool _inMinMap = false)
    {
        this.Init();
        m_Label = _label;
        inMinMap = _inMinMap;
       
        if (m_Label == null)
            return;

        UpdateIcon();

        if (_label is TownLabel)
        {
            if (Pathea.PeGameMgr.IsAdventure)
            {
                TownLabel townLabel = _label as TownLabel;
                int colorIndex = townLabel.GetAllianceColor();
                if (colorIndex >= 0 && colorIndex < AllyColor.AllianceCols.Length)
                {
                    Color32 color32 = AllyColor.AllianceCols[colorIndex];
                    SetColor(color32);
                }
                SetFriendlyLevelIcon(townLabel.GetFriendlyLevel());
            }
        }
        else if (_label is MissionLabel)
        {
            MissionLabel missionLabel = _label as MissionLabel;
            SetColor(missionLabel.GetMissionColor());
        }

        //SetEnableClick(m_Label.GetType()!= PeMap.ELabelType.Mission);
        SetEnableClick(true);

        UpdateRadiusSize();
        // label depth 处理

        //从高到底:User,Mission,NPC,Revive,Vehicle,Mark,FastTravel
        switch (_label.GetType())
        {
            case PeMap.ELabelType.FastTravel:
                mSpr.depth = 1;
                break;
            case PeMap.ELabelType.Mark:
                mSpr.depth = 2;
                break;
            case PeMap.ELabelType.Vehicle:
                mSpr.depth = 3;
                break;
            case PeMap.ELabelType.Revive:
                mSpr.depth = 4;
                break;
            case PeMap.ELabelType.Npc:
                mSpr.depth = 5;
                break;
            case PeMap.ELabelType.Mission:
                //lz-2016.10.11 如果有任务覆盖的情况，主线在支线上面
                MissionLabel missionLabel = _label as MissionLabel;
                mSpr.depth = MissionManager.IsMainMission(missionLabel.m_missionID)?7:6;
                break;
            case PeMap.ELabelType.User:
                mSpr.depth = 8;
                break;
            default:
                mSpr.depth = 0;
                break;
        }
    }

    //lz-2016.09.06 更新当前图标
    public void UpdateIcon(string iconName=null)
    {
        mSpr.spriteName = null==iconName? iconStr:iconName;
        mBoxCollider.size = new Vector3(mSpr.transform.localScale.x, mSpr.transform.localScale.y, mBoxCollider.size.z);
    }

    public void SetColor(Color32 color32)
    {
        m_Col= new Color(color32.r / 255.0f, color32.g / 255.0f, color32.b / 255.0f, color32.a / 255.0f);
        mSpr.color = m_Col;
    }


    public void SetFriendlyLevelIcon(int iconIndex)
    {
        if (iconIndex < 0 || iconIndex > FriendlyLevelIconName.Length)
        {
            mFriendSpr.enabled = false;
        }
        else
        {
            mFriendSpr.enabled = true;
            mFriendSpr.spriteName = FriendlyLevelIconName[iconIndex];
            mFriendSpr.MakePixelPerfect();
        }
    }

    public void SetLabelPosByNPC(int npcid)
    {
        this.m_NpcID = npcid;
    }

    void UpdateLabelPosByNPC()
    {
        if (m_NpcID == -1)
            return;
        Pathea.PeEntity npc = Pathea.EntityMgr.Instance.Get(m_NpcID);
        if (npc == null || (npc.proto != Pathea.EEntityProto.Npc && npc.proto != Pathea.EEntityProto.RandomNpc))
            return;
        MissionLabel misLabel = m_Label as MissionLabel;
        if (misLabel!=null)
            misLabel.SetLabelPos(npc.position);
    }


    void Update()
    {
        if (m_Label != null && inMinMap && GameUI.Instance.mMainPlayer != null)
        {
            UpdatePosInMinMap();
        }
        UpdateLabelPosByNPC();
        UpdateMisLabel();
        UpdateReviveLabel();
    }

    void UpdateRadiusSize()
    {
        if (_ILabel.GetRadius() != -1)
        {
            mSpr.transform.localScale = GetDiameterSize(_ILabel.GetRadius());
        }
        else
            mSpr.MakePixelPerfect();

        BoxCollider collider = gameObject.GetComponent<BoxCollider>();
        collider.size = new Vector3(mSpr.transform.localScale.x, mSpr.transform.localScale.y, collider.size.z);
    }

    //lz-2016.09.30 获取直径大小
    Vector3 GetDiameterSize(float radius)
    {
        float size = Mathf.Clamp(radius, m_MinRadius, m_MaxRadius);
        return new Vector3(size, size, 1);

        //lz-2016.10.12 数据库填的值从米转成像素比较小，暂时直接设置像素
        //radius = Mathf.Clamp(radius, m_MinRadius, m_MaxRadius);
        //float diameterPx = ConvetMToPx(radius)*2;
        //if (!inMinMap)
        //{
        //    diameterPx *= mBigMapIconRangeScale;
        //}
        //return new Vector3(diameterPx, diameterPx, 1);
    }

    //lz-2016.09.30 米转换成像素单位
    float ConvetMToPx(float m)
    {
        float texSize = 0f;
        float mapSize = 0f;
        if (PeGameMgr.IsAdventure || PeGameMgr.IsBuild)
        {
            if (null != RandomMapConfig.Instance)
            {
                texSize = 3140;
                mapSize = RandomMapConfig.Instance.MapSize.x;
            }
        }
        else
        {
            texSize = 4096;
            mapSize=UIStroyMap.StoryWorldSize;
        }
        return texSize / mapSize * m;
    }

    void UpdatePosInMinMap()
    {
        Vector3 dis = worldPos - GameUI.Instance.mMainPlayer.position;
        dis *= m_WorldMapConvert;
        float pos_x = dis.x * GameUI.Instance.mUIMinMapCtrl.mMapScale.x;
        float pos_z = dis.z * GameUI.Instance.mUIMinMapCtrl.mMapScale.y;
        transform.localPosition = new Vector3(pos_x, pos_z, 0);
    }

    void OnHover(bool isOver)
    {
        if (e_OnMouseOver != null && !inMinMap)
            e_OnMouseOver(this, isOver);
    }

    void OnClick()
    {
        if (e_OnClick != null && !inMinMap)
            e_OnClick(this);
    }

    //处理MissionLabel
    #region MissionLabel
    void UpdateMisLabel()
    {
        if (null==m_Label||m_Label.GetType() != PeMap.ELabelType.Mission)
            return;

        MissionLabel misLabel = m_Label as MissionLabel;
        if (misLabel != null)
        {
            UIMissionMgr.MissionView view = UIMissionMgr.Instance.GetMissionView(misLabel.m_missionID);
            if (misLabel.m_type == MissionLabelType.misLb_target && misLabel.m_target != null)
            {
                if (misLabel.m_target.mComplete)
                {
                    PeMap.LabelMgr.Instance.Remove(this._ILabel);
                    return;
                }
                if (view != null)
                {
                    mSpr.enabled = view.mMissionTag;
                }
            }
            else if (misLabel.m_type == MissionLabelType.misLb_end)
            {
                if (view != null&&view.mComplete!= misLabel.IsComplete)
                {
                    misLabel.IsComplete = view.mComplete;
                    SetColor(misLabel.GetMissionColor());
                    SetEnableClick(true);
                }
            }
        }
    }
    #endregion

    #region ReviveLabel
    void UpdateReviveLabel() 
    {
        if (m_Label==null||m_Label.GetType() != PeMap.ELabelType.Revive)
            return;

        ReviveLabel revLabel = m_Label as ReviveLabel;
        if (revLabel != null && Vector3.Distance(revLabel.GetPos(), Pathea.PeCreature.Instance.mainPlayer.position) < 50)
        {
            ReviveLabel.Mgr.Instance.Remove(revLabel);
        }
    }
    #endregion

    #region pulic Mehtods

    /// <summary>
    /// lz-2016.08.05 这个方法是设置Label不可点击，但是颜色不变灰的方法
    /// </summary>
    /// <param name="enable"></param>
    void SetEnableClick(bool enable)
    {
        if (enable)
		{
			mButton.defaultColor = m_Col;
			mButton.hover = new Color(m_Col.r,m_Col.g,m_Col.b,m_Col.a*0.5f);
			mButton.disabledColor = new Color(m_Col.r*0.6f,m_Col.g*0.6f,m_Col.b*0.6f,m_Col.a);;
			mButton.pressed = new Color(m_Col.r*0.6f,m_Col.g*0.6f,m_Col.b*0.6f,m_Col.a);

        }
        else
		{
			mButton.defaultColor = m_Col;
            mButton.hover = m_Col;
            mButton.disabledColor = m_Col ;
            mButton.pressed = m_Col;
        }
        mButton.isEnabled = enable;
    }
    #endregion
}
