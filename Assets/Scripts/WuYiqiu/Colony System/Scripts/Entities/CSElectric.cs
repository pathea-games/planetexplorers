using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class CSElectric : CSCommon 
{
	// Reference power power plant
	public CSPowerPlant m_PowerPlant;
	
	public List<CSPowerPlant>	m_PowerPlants = new List<CSPowerPlant>();
	
	
	public override void ChangeState ()
	{
		bool oldState = m_IsRunning;
		if (Assembly != null && Assembly.IsRunning
			&& m_PowerPlant != null && m_PowerPlant.IsRunning)
			m_IsRunning = true;
		else
			m_IsRunning = false;
		if (oldState && !m_IsRunning){
			DestroySomeData();//from on to off
			//UpdateDataToUI();
		}
		else if (!oldState && m_IsRunning)
			UpdateDataToUI();//from off to on
	}
	
	
}
