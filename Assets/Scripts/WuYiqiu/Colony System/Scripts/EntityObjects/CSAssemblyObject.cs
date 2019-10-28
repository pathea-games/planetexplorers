using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class CSAssemblyObject : CSEntityObject 
{
	public PolarShield m_EnergySheildPrfab;
	
	private PolarShield m_CurEnergySheild;	
	public PolarShield CurEnergySheild		{ get { return m_CurEnergySheild;} }
	
	public CSAssembly		m_Assembly	{ get { return m_Entity == null ? null : m_Entity as CSAssembly;}}
	
	public CSAssemblyInfo 	m_Info;
	

	#region ENTITY_OBJECT_FUNC

	public override int Init(CSBuildingLogic csbul, CSCreator creator, bool bFight = true)
	{
        int r = base.Init(csbul, creator, bFight);

		if (r == CSConst.rrtSucceed)
			CreateEnergySheild();

		return r;
	}
    public override int Init(int id, CSCreator creator, bool bFight = true)
    {
        int r = base.Init(id, creator, bFight);

        if (r == CSConst.rrtSucceed)
            CreateEnergySheild();

        return r;
    }
	#endregion
	
	public void CreateEnergySheild()
	{
		if (m_CurEnergySheild == null )
		{
			m_CurEnergySheild = Instantiate(m_EnergySheildPrfab) as PolarShield;
			m_CurEnergySheild.transform.parent = transform.parent;
			m_CurEnergySheild.transform.localPosition = Vector3.zero;
			if(m_Assembly==null){
				m_CurEnergySheild.SetRadius(m_CurEnergySheild.min_Radius);
				m_CurEnergySheild.SetLevel(0);
			}
			else{
				if(m_Assembly.gameLogic!=null){
					if(m_Assembly.gameLogic.GetComponent<CSBuildingLogic>().IsFirstConstruct){
						m_CurEnergySheild.SetLerpRadius(m_Assembly.Radius);
						m_CurEnergySheild.SetLevel(m_Assembly.Level);
					}else{
						m_CurEnergySheild.SetRadius(m_Assembly.Radius);
						m_CurEnergySheild.SetLevel(m_Assembly.Level);
					}
				}else{
					m_CurEnergySheild.SetRadius(m_Assembly.Radius);
					m_CurEnergySheild.SetLevel(m_Assembly.Level);
				}
			}
            m_CurEnergySheild.m_Model.SetActive(false);
			//m_CurEnergySheild.localRotation = ;
		}
	}
	public void RefreshObject(){
		if(m_Assembly==null){
			m_CurEnergySheild.SetLerpRadius(m_CurEnergySheild.min_Radius);
			m_CurEnergySheild.SetLevel(0);
		}
		else{
			m_CurEnergySheild.SetLerpRadius(m_Assembly.Radius);
			m_CurEnergySheild.SetLevel(m_Assembly.Level);
            m_CurEnergySheild.AfterUpdate();
		}
	}
	#region GUARDTRIGGER_CALLBACK

	UTimer m_triggerTimer;

	void OnEnterTrigger(PeEntity monster, int skillId)
	{
		if(m_Assembly!=null&&m_Assembly.gameLogic!=null)
		{
			CSBuildingLogic csb = m_Assembly.gameLogic.GetComponent<CSBuildingLogic>();

			csb.ShieldOn(monster,skillId);

			PeEntity buidEntity =  m_Assembly.gameLogic.GetComponent<PeEntity>();
			if(buidEntity != null && PeNpcGroup.Instance != null)
				PeNpcGroup.Instance.OnCSAttackEnmey(buidEntity,monster);

		}
	}

	void OnExitTrigger(PeEntity monster)
	{
		if(m_Assembly!=null&&m_Assembly.gameLogic!=null)
		{
			CSBuildingLogic csb = m_Assembly.gameLogic.GetComponent<CSBuildingLogic>();
			
			csb.ShieldOff(monster);

//			if(PeNpcGroup.Instance != null)
//				PeNpcGroup.Instance.OnCsAttackEnd();
		}
	}

	#endregion
	
	// Use this for initialization
	new void Start () 
	{	
		base.Start();

		m_Info = CSInfoMgr.m_AssemblyInfo;
		
		CreateEnergySheild();

		m_CurEnergySheild.onEnterTrigger += OnEnterTrigger;
		m_CurEnergySheild.onExitTrigger  += OnExitTrigger;

		m_triggerTimer = new UTimer();
		m_triggerTimer.ElapseSpeed = 1;
	}
	
	// Update is called once per frame
	new void Update () 
	{
		base.Update();

		if (m_CurEnergySheild != null)
		{
			if (m_Assembly != null)
			{
				if (m_Assembly.bShowShield)
				{
					m_CurEnergySheild.m_Model.SetActive(true);
				}
				else
					m_CurEnergySheild.m_Model.SetActive(false);
			}
			else
			{
				m_CurEnergySheild.m_Model.SetActive(true);
			}
		}
		
		if (m_Assembly != null)
		{
			m_Assembly.Position = transform.position;

		}
	}

	void FixedUpdate()
	{
		if (m_Assembly != null)
		{
			if (m_triggerTimer.Second >= m_Assembly.damageCD)
				m_triggerTimer.Second = 0;

			m_triggerTimer.Update(Time.deltaTime);
		}
	}
}
