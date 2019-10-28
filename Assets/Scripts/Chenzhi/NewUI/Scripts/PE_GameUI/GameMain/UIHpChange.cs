using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using SkillSystem;

public class HPChangeEventData
{
    public SkEntity m_Self;
    public float m_HPChange;
    public Transform m_Transfrom;
    public EEntityProto m_Proto;
    public float m_AddTime;
}

public interface IHPEventData
{
    HPChangeEventData Pop();
}

public class UIHpChange : MonoBehaviour
{
    private static UIHpChange s_Instance = null;
    public static UIHpChange Instance { get { return s_Instance; } }

    private List<UIHpNode> m_Nodes;

    public bool m_ShowHPChange { get { return SystemSettingData.Instance.HPNumbers; } }
    [SerializeField]
    GameObject nodePrefab = null;
    [SerializeField]
    Transform contentTs = null;
    [SerializeField]
    float m_DataOutTime = 1f;  //lz-2016.11.03 数据超时时间

    public Color MonsterHurt = Color.red;
    public Color NPCHurt = Color.blue;
    public Color CreationHurt = Color.yellow;
    public Color PlayerHurt = Color.red;
	public Color PlayerHeal = Color.green;
	public Color DoodadHurt = Color.red;

    IHPEventData m_HPEventData;
    bool m_CurFrameHasShow = false;

    void Awake()
    {
        s_Instance = this;
        m_Nodes = new List<UIHpNode>();
        m_HPEventData = HPChangeEventDataMan.Instance as IHPEventData;
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
        {
            //UpdateTest();
        }

        //lz-2016.11.03 有数据的情况下保证每一帧都有显示有效数据
        m_CurFrameHasShow = false;
        while(!m_CurFrameHasShow&& null!=m_HPEventData) //lz-2016.11.07 有可能会变空
        {
            HPChangeEventData data = m_HPEventData.Pop();
            //lz-2016.11.03 因为这里是用栈处理的，只显示最新的，如果堆积超时的就不要了
            if (null == data||Time.realtimeSinceStartup-data.m_AddTime>= m_DataOutTime)
            {
                m_CurFrameHasShow = true;
            }
            else
            {
                if (null != data.m_Transfrom && null != data.m_Self)
                {
                    Pathea.PeEntity entity = data.m_Self.GetComponent<Pathea.PeEntity>();
                    //lz-2016.11.03 entity没有死，并且Transfrom还存在的时候才显示HPChange效果
                    if (null != entity && !entity.IsDeath())
                    {
                        if (ShowHPChange(data.m_HPChange, data.m_Transfrom.position, data.m_Proto))
                        {
                            m_CurFrameHasShow = true;
                        }
                    }
                }
            }
        }
    }


    public void RemoveNode(UIHpNode node)
    {
        if (node != null)
        {
            node.transform.parent = null;
            GameObject.Destroy(node.gameObject);
        }
        m_Nodes.Remove(node);
    }

    void AddNode(Vector3 position, float hpChange, Color col)
    {
        GameObject obj = GameObject.Instantiate(nodePrefab) as GameObject;
        obj.transform.parent = contentTs;
        obj.transform.localScale = Vector3.one;
        obj.SetActive(true);
        UIHpNode node = obj.GetComponent<UIHpNode>();
        node.color = col;
        node.worldPostion = position + Random.insideUnitSphere + Vector3.up;
        node.text = hpChange.ToString("0");
        node.isHurt = hpChange < 0;
        m_Nodes.Add(node);
    }

    bool ShowHPChange(float hpChange, Vector3 position, EEntityProto entityProto)
    {
        if (!m_ShowHPChange)
            return false;
        if (GameUI.Instance == null)
            return false;
        if (GameUI.Instance.mMainPlayer == null)
            return false;
        if (GameUI.Instance.mUIWorldMap == null)
            return false;
        if (GameUI.Instance.mUIWorldMap.isShow)
            return false;
        if ((position - GameUI.Instance.mMainPlayer.position).magnitude > 100)
            return false;
        if (s_Instance == null)
            return false;
        if (hpChange == 0f)
            return false;
        if (hpChange < 1.0f && hpChange > 0f)
        {
            hpChange = 1f;
        }
        if (hpChange > -1f && hpChange < 0f)
        {
            hpChange = -1f;
        }

        Color col = Color.red;
        if (CanShowHpChange(hpChange, entityProto, out col))
        {
            AddNode(position, hpChange, col);
            return true;
        }
        return false;
    }

    bool CanShowHpChange(float hpChange, EEntityProto entityProto, out Color col)
    {
        // 非玩家加血不显示数字
        col = Color.red;
        //lz-2016.11.03 游戏暂停的时候不显示
        if (hpChange > 0 && entityProto != EEntityProto.Player|| Pathea.PeGameMgr.gamePause)
            return false;

        if (entityProto == EEntityProto.Player)
			col = hpChange < 0 ? PlayerHurt : PlayerHeal;
		else if (entityProto == EEntityProto.Npc || entityProto == EEntityProto.RandomNpc)
			col = NPCHurt;
		else if (entityProto == EEntityProto.Tower)
			col = CreationHurt;
		else if (entityProto == EEntityProto.Monster)
			col = MonsterHurt;
		else if (entityProto == EEntityProto.Doodad)
			col = DoodadHurt;
        return true;
    }

    // test code 
    [SerializeField]
    EEntityProto testProto = EEntityProto.Player;
    [SerializeField]
    bool testAdd = false;
    [SerializeField]
    int testValue = -200;
    [SerializeField]
    Transform testTs = null;

    void UpdateTest()
    {
        if (testAdd && testTs != null)
        {
            Color col = Color.red;
            if (CanShowHpChange(testValue, testProto, out col))
                AddNode(testTs.localPosition, testValue, col);
            testAdd = false;
        }
    }
}

