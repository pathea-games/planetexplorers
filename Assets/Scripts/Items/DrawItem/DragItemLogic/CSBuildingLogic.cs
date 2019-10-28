using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ItemAsset;
using Pathea;
using SkillSystem;
using System.Collections;

public class CSBuildingLogic : DragItemLogic
{
	public bool InTest=false;

    public PESkEntity _skEntity;
    public PeTrans _peTrans;
    public PeEntity _peEntity;
    public CSEntity m_Entity;

	public CSConst.ObjectType m_Type;
	public Transform travelTrans;
	public Transform[] m_ResultTrans;
    public Transform[] m_WorkTrans;
    public Transform[] m_NPCTrans;
	public bool IsFirstConstruct= true;

    public bool GetNpcPos(int index,out Vector3 pos) 
    {
        bool result = CSMain.GetAssemblyPos(out pos);
        if(!result)
            return false;
        if (index < m_NPCTrans.Length) 
        {
            //index:0 playerPos
            //index:1-18 npcPos
            //index:19 pujaPos
            pos = m_NPCTrans[index].position;
        }
        return true;
    }

    GameObject _mainGo;
	public GameObject ModelObj{
		get{return _mainGo;}
	}
    public ColonyNetwork network
    {
        get { return mNetlayer as ColonyNetwork; }
    }
    public int TeamId
    {
        get
        {
            return mNetlayer.TeamId;
        }
    }
    public int InstanceId
    {
        get { return itemDrag.itemObj.instanceId; }
    }

    public int protoId
    {
        get { return itemDrag.itemObj.protoId; }
    }

	public bool HasModel{
		get{
			return _mainGo!=null;
		}
	}
	#region monsterskill
	Dictionary<PeEntity,SkillSystem.SkInst> monsterSkillDict = new Dictionary<PeEntity,SkillSystem.SkInst>();
	#endregion


    #region itemscript
    public override void OnActivate()
    {
        base.OnActivate();
        ItemScript s = GetComponentInChildren<ItemScript>();
        if (null != s)
        {
            s.OnActivate();
        }
    }

    public override void OnDeactivate()
    {
        ItemScript s = GetComponentInChildren<ItemScript>();
        if (null != s)
        {
            s.OnDeactivate();
        }
    }

    public override void OnConstruct()
    {
        base.OnConstruct();

        _mainGo = itemDrag.CreateViewGameObject(null);

        if (_mainGo == null)
        {
            return;
		}
        
		if(_peTrans==null)
			_peTrans = gameObject.AddComponent<PeTrans>(); 
		if(_peTrans!=null)
        	_peTrans.SetModel(_mainGo.transform);

        _mainGo.transform.parent = transform;

        _mainGo.transform.position = transform.position;
        _mainGo.transform.rotation = transform.rotation;
        _mainGo.transform.localScale = transform.localScale;

        ItemScript itemScript = _mainGo.GetComponentInChildren<ItemScript>();
        if (null != itemScript)
        {
            itemScript.SetItemObject(itemDrag.itemObj);
            itemScript.InitNetlayer(mNetlayer);
            itemScript.id = id;
            itemScript.OnConstruct();
		}
		IsFirstConstruct = false;
    }

    public override void OnDestruct()
    {
        ItemScript s = GetComponentInChildren<ItemScript>();
        if (null != s)
        {
            s.OnDestruct();
        }
        if (_mainGo != null)
        {
            GameObject.Destroy(_mainGo);
        }
        //use base
        base.OnDestruct();
    }
    #endregion


    #region init
    void Start()
    {
        if (!GameConfig.IsMultiMode)
        {
            m_Type = GetMTypeFromProtoId(itemDrag.itemObj.protoId);
            CSMgCreator creator = CSMain.s_MgCreator;
            if (creator != null)
            {
                CSEntityAttr attr = new CSEntityAttr();
                attr.m_InstanceId = InstanceId;
                attr.m_protoId = protoId;
                attr.m_Type = (int)m_Type;
                attr.m_Pos = transform.position;
                attr.m_LogicObj = gameObject;
                //attr.m_Bound = GetObjectBounds();
                //attr.m_Bound.center = transform.TransformPoint(attr.m_Bound.center);
                //attr.m_ColonyBase = _ColonyObj;
                int r = creator.CreateEntity(attr, out m_Entity); 
                if (r != CSConst.rrtSucceed)
                {
                    Debug.LogError("Error with Init Entities");
                    return;
                }
                _peEntity = gameObject.GetComponent<PeEntity>();
                _peTrans = gameObject.GetComponent<PeTrans>(); 
                _skEntity = gameObject.GetComponent<PESkEntity>();
                _skEntity.m_Attrs = new PESkEntity.Attr[5];
                _skEntity.m_Attrs[0] = new PESkEntity.Attr();
                _skEntity.m_Attrs[1] = new PESkEntity.Attr();
                _skEntity.m_Attrs[2] = new PESkEntity.Attr();
				_skEntity.m_Attrs[3] = new PESkEntity.Attr();
				_skEntity.m_Attrs[4] = new PESkEntity.Attr();
                _skEntity.m_Attrs[0].m_Type = AttribType.HpMax; 
                _skEntity.m_Attrs[1].m_Type = AttribType.Hp;
                _skEntity.m_Attrs[2].m_Type = AttribType.CampID;
                _skEntity.m_Attrs[3].m_Type = AttribType.DefaultPlayerID;
				_skEntity.m_Attrs[4].m_Type = AttribType.DamageID;
                _skEntity.m_Attrs[0].m_Value = m_Entity.MaxDurability;
                _skEntity.m_Attrs[1].m_Value = m_Entity.CurrentDurability;
                _skEntity.m_Attrs[2].m_Value = PeCreature.Instance.mainPlayer.GetAttribute(AttribType.CampID);
                _skEntity.m_Attrs[3].m_Value = ForceConstant.PLAYER;
				_skEntity.m_Attrs[4].m_Value = PeCreature.Instance.mainPlayer.GetAttribute(AttribType.DamageID);

                _skEntity.onHpChange += OnHpChange;
				if(m_Type == CSConst.ObjectType.Assembly)
					_skEntity.onHpChange +=SendHpChangeMessage;
                _skEntity.deathEvent += OnDeathEvent;
                _skEntity.InitEntity();
                m_Entity.onDuraChange = SetHp;
				OnHpChange(_skEntity,0);
				int entityId = Pathea.WorldInfoMgr.Instance.FetchNonRecordAutoId();
				EntityMgr.Instance.InitEntity(entityId,_peEntity.gameObject);
				creator.AddLogic(id,this);
			}
			StartCoroutine(SetFirstConstruct());
        }
    }
	IEnumerator SetFirstConstruct(){
		yield return new WaitForSeconds(10.0f);
		IsFirstConstruct = false;
	}
    public void OnHpChange(SkillSystem.SkEntity caster, float hpChange)
    {
        m_Entity.CurrentDurability = _skEntity.GetAttribute(AttribType.Hp);
    }

	public void SendHpChangeMessage(SkillSystem.SkEntity caster, float hpChange){
        PeNpcGroup.Instance.OnAssemblyHpChange(caster, hpChange);
	}

    public void OnDeathEvent(SkillSystem.SkEntity skSelf, SkillSystem.SkEntity skCaster)
    {
        m_Entity.m_Creator.RemoveEntity(InstanceId);
    }

    public void SetHp(float hp)
    {
        _skEntity.SetAttribute(AttribType.Hp, hp);
    }
    #endregion

    
    #region multiMode
    public void InitInMultiMode(CSEntity m_Entity,int ownerId)
    {
        m_Type = GetMTypeFromProtoId(itemDrag.itemObj.protoId);
        this.m_Entity = m_Entity;
        m_Entity.gameLogic = gameObject;

		_peEntity = gameObject.GetComponent<PeEntity>();
		_peTrans = gameObject.GetComponent<PeTrans>(); 
		_skEntity = gameObject.GetComponent<PESkEntity>();
		_skEntity.m_Attrs = new PESkEntity.Attr[5];
		_skEntity.m_Attrs[0] = new PESkEntity.Attr();
		_skEntity.m_Attrs[1] = new PESkEntity.Attr();
		_skEntity.m_Attrs[2] = new PESkEntity.Attr();
		_skEntity.m_Attrs[3] = new PESkEntity.Attr();
		_skEntity.m_Attrs[4] = new PESkEntity.Attr();
		_skEntity.m_Attrs[0].m_Type = AttribType.HpMax; 
		_skEntity.m_Attrs[1].m_Type = AttribType.Hp;
		_skEntity.m_Attrs[2].m_Type = AttribType.CampID;
		_skEntity.m_Attrs[3].m_Type = AttribType.DefaultPlayerID;
		_skEntity.m_Attrs[4].m_Type = AttribType.DamageID;
		_skEntity.m_Attrs[0].m_Value = m_Entity.MaxDurability;
		_skEntity.m_Attrs[1].m_Value = m_Entity.CurrentDurability;
		_skEntity.m_Attrs[2].m_Value = 1;
		_skEntity.m_Attrs[3].m_Value = ownerId;
		PeEntity ownerEntity = EntityMgr.Instance.Get(ownerId);
		if(ownerEntity!=null){
			_skEntity.m_Attrs[2].m_Value = ownerEntity.GetAttribute(AttribType.CampID);
			_skEntity.m_Attrs[4].m_Value = ownerEntity.GetAttribute(AttribType.DamageID);
		}

		_skEntity.onHpChange += OnHpChange;
		if(m_Type == CSConst.ObjectType.Assembly)
			_skEntity.onHpChange +=SendHpChangeMessage;
		_skEntity.deathEvent += OnDeathEvent;
		_skEntity.InitEntity();
		OnHpChange(_skEntity,0);
		EntityMgr.Instance.InitEntity(InstanceId,_peEntity.gameObject);

        //if(m_Type == CSConst.ObjectType.Processing)
        //{
        //    network.InitResultPos(m_ResultTrans);
        //}
		
		m_Entity.m_MgCreator.AddLogic(id,this);

		if (CSMain.s_MgCreator == m_Entity.m_MgCreator && m_Entity as CSAssembly != null)
		{
			//Vector3 travelPos = gameObject.transform.position + new Vector3(0, 2, 0);
			if (travelTrans != null)
			{
				//travelPos = travelTrans.position;
			}
		}

		StartCoroutine(SetFirstConstruct());
    }
    #endregion



    #region interface
    public static CSConst.ObjectType GetMTypeFromProtoId(int protoId)
    {
        CSConst.ObjectType m_Type = CSConst.ObjectType.None;
        switch (protoId)
        {
            case ColonyIDInfo.COLONY_ASSEMBLY:
                m_Type = CSConst.ObjectType.Assembly;
                break;
            case ColonyIDInfo.COLONY_PPCOAL:
                m_Type = CSConst.ObjectType.PowerPlant_Coal;
                break;
            case ColonyIDInfo.COLONY_STORAGE:
                m_Type = CSConst.ObjectType.Storage;
                break;
            case ColonyIDInfo.COLONY_REPAIR:
                m_Type = CSConst.ObjectType.Repair;
                break;
			case ColonyIDInfo.COLONY_DWELLINGS:
				m_Type = CSConst.ObjectType.Dwelling;
                break;
            case ColonyIDInfo.COLONY_ENHANCE:
                m_Type = CSConst.ObjectType.Enhance;
                break;
            case ColonyIDInfo.COLONY_RECYCLE:
                m_Type = CSConst.ObjectType.Recyle;
                break;
			case ColonyIDInfo.COLONY_FARM:
				m_Type = CSConst.ObjectType.Farm;
	                break;
			case ColonyIDInfo.COLONY_FACTORY:
				m_Type = CSConst.ObjectType.Factory;
	                break;
			case ColonyIDInfo.COLONY_PROCESSING:
				m_Type = CSConst.ObjectType.Processing;
	                break;
			case ColonyIDInfo.COLONY_TRADE:
				m_Type = CSConst.ObjectType.Trade;
	                break;
			case ColonyIDInfo.COLONY_TRAIN:
				m_Type = CSConst.ObjectType.Train;
	                break;
			case ColonyIDInfo.COLONY_CHECK:
				m_Type = CSConst.ObjectType.Check;
	                break;
			case ColonyIDInfo.COLONY_TREAT:
				m_Type = CSConst.ObjectType.Treat;
	                break;
			case ColonyIDInfo.COLONY_TENT:
				m_Type = CSConst.ObjectType.Tent;
	                break;
			case ColonyIDInfo.COLONY_FUSION:
				m_Type = CSConst.ObjectType.PowerPlant_Fusion;
				break;
            default:
                break;
        }
        return m_Type;
    }
    #endregion

    public void DestroySelf()
    {
        DragArticleAgent.Destory(id);
    }


	public void ShieldOn(PeEntity monster,int skillId){
		SkillSystem.SkInst skill =  _skEntity.StartSkill(monster.skEntity,skillId);
		monsterSkillDict[monster] = skill;
	}
	public void ShieldOff(PeEntity monster){
		if(monsterSkillDict.ContainsKey(monster)){
			_skEntity.CancelSkill(monsterSkillDict[monster]);
			monsterSkillDict.Remove(monster);
		}
	}
}
