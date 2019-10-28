using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CSPowerPlantObject : CSEntityObject 
{
	
	[SerializeField]
	private EnergyArea m_EnergyArea;
	
	public CSPowerPlant		m_PowerPlant	{ get { return m_Entity == null ? null : m_Entity as CSPowerPlant;}}
								
	public CSPowerPlantInfo	m_Info;
	
	
	#region ENTITY_OBJECT_FUNC

	public override int Init (CSBuildingLogic csbl, CSCreator creator, bool bFight)
	{
		int a = 0;
		a++;
        return base.Init(csbl, creator, bFight);
	}

    public override int Init(int id, CSCreator creator, bool bFight)
    {
        int a = 0;
        a++;
        return base.Init(id, creator, bFight);
    }
	#endregion
	
	// Use this for initialization
	new void Start ()
	{
		base.Start();
		if(m_ItemID==ColonyIDInfo.COLONY_FUSION)
			m_Info = CSInfoMgr.m_ppFusion;
		else
			m_Info = CSInfoMgr.m_ppCoal;
	}
	
	// Update is called once per frame
	new void Update ()
	{
		base.Update();

		if (m_EnergyArea == null)
			return;

		if (m_PowerPlant == null)
			m_EnergyArea.radius = m_Info.m_Radius;
		else
		{
            if (!m_PowerPlant.bShowElectric)
            {
                m_EnergyArea.gameObject.SetActive(false);
            }
            else{
                m_EnergyArea.gameObject.SetActive(true);
            }
			m_EnergyArea.radius = m_PowerPlant.Radius;
			
			if (m_Type == CSConst.ObjectType.PowerPlant_Coal)
			{
				CSPPCoal cspp = m_PowerPlant as CSPPCoal;
				if (cspp.isWorking() && cspp.IsRunning)
					m_EnergyArea.energyScale = ( 1F - cspp.Data.m_CurWorkedTime  / cspp.Data.m_WorkedTime ) * 0.5F;
				else
					m_EnergyArea.energyScale = 0F;
			}else if(m_Type ==CSConst.ObjectType.PowerPlant_Fusion)
			{
				CSPPFusion cspp = m_PowerPlant as CSPPFusion;
				if (cspp.isWorking() && cspp.IsRunning)
					m_EnergyArea.energyScale = ( 1F - cspp.Data.m_CurWorkedTime  / cspp.Data.m_WorkedTime ) * 0.5F;
				else
					m_EnergyArea.energyScale = 0F;
			}
			
			m_Power = m_PowerPlant.m_RestPower;
		}
		
	}
}
