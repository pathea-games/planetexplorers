using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using CSRecord;

public abstract class CSPowerPlant : CSCommon
{
    public int m_PPType;

    public float m_RestPower;

    // Data
    protected CSPowerPlanetData m_PPBaseData;
    public CSPowerPlanetData PPBaseData
    {
        get
        {
            if (m_PPBaseData == null)
                m_PPBaseData = m_Data as CSPowerPlanetData;
            return m_PPBaseData;
        }
    }

    //  information
    public CSPowerPlantInfo m_PPInfo;
    public CSPowerPlantInfo Info
    {
        get
        {
            if (m_PPInfo == null)
                m_PPInfo = m_Info as CSPowerPlantInfo;
            return m_PPInfo;
        }
    }

    public bool bShowElectric { get { return PPBaseData.bShowElectric; } set { PPBaseData.bShowElectric = value; } }
    public float Radius { get { return Info.m_Radius; } }

    // Colony Entities
    public List<CSElectric> m_Electrics;

    // ItemObject
    public Energy[] m_ChargingItems;

    public CSPowerPlant()
    {
		m_PPType = CSConst.etPowerPlant;
        m_Electrics = new List<CSElectric>();
        m_ChargingItems = new Energy[12];

        m_Grade = CSConst.egtMedium;
    }

    // A point in power plant range? Just x, z 
    public bool InRange(Vector3 pos)
    {
        Vector2 sourcePos = new Vector2(pos.x, pos.z);
        Vector2 orl = new Vector2(Position.x, Position.z);

        if (Vector2.Distance(sourcePos, orl) <= Radius)
            return true;

        return false;
    }

    public void RemoveElectric(CSElectric csel)
    {
        m_RestPower += csel.m_Power;
        m_Electrics.Remove(csel);
    }

    public void AddElectric(CSElectric csel)
    {
        m_RestPower -= csel.m_Power;
        m_Electrics.Add(csel);
    }

    public virtual bool isWorking()
    {
        return true;
    }

    #region CHARGING_ITEMS_FUNC

    public void SetChargingItem(int index, ItemObject item)
    {
        if (PPBaseData == null)
            return;

		if(item==null){
			m_ChargingItems[index] = null;
			PPBaseData.m_ChargingItems.Remove(index);
			return;
		}

        Energy energyItem = item.GetCmpt<Energy>();
        if (energyItem == null)
        {
            Debug.Log("Item cannot be charged!");
            return;
        }
        if (m_ChargingItems.Length <= index || index < 0)
        {
            Debug.Log("The giving index is out of arange!");
            return;
        }


        //if (!GameConfig.IsMultiMode)
        //{
        m_ChargingItems[index] = energyItem;

        if (PPBaseData.m_ChargingItems.ContainsKey(index))
        {
            if (item != null)
                PPBaseData.m_ChargingItems[index] = item.instanceId;
            else
                PPBaseData.m_ChargingItems.Remove(index);
        }
        else
        {
            if (item != null)
                PPBaseData.m_ChargingItems.Add(index, item.instanceId);
        }
        //}
        //else
        //{
        //    if (PPBaseData.m_ChargingItems.ContainsKey(index))
        //    {
        //        //addchargeitem
        //        if (item != null)
        //        {
        //            _ColonyObj._Network.POW_AddChargItem(index, item);
        //        }
        //        else
        //        {
        //            //remove item
        //            _ColonyObj._Network.POW_RemoveChargItem(PPBaseData.m_ChargingItems[index]);
        //        }
        //    }
        //    else
        //    {
        //        if (item != null)
        //        {
        //            _ColonyObj._Network.POW_AddChargItem(index, item);
        //        }
        //        else
        //        {
        //            m_ChargingItems[index] = null;
        //        }
        //    }
        //}
    }

    public ItemObject GetChargingItem(int index)
    {
        if (PPBaseData == null)
            return null;

        if (m_ChargingItems.Length <= index || index < 0)
        {
            Debug.Log("The giving index is out of arange!");
            return null;
        }
		if(m_ChargingItems[index]==null){
			return null;
		}

        return m_ChargingItems[index].itemObj;
    }

    public int GetChargingItemsCnt()
    {
        return m_ChargingItems.Length;
    }

    #endregion

    #region MANAGE_ELECTRIC_FUNC

    // [NEW_FUNC] =========>>>>
    public override void ChangeState()
    {
        if (Assembly != null && Assembly.IsRunning
            && isWorking())
        {
            m_IsRunning = true;
            FindElectrics();
        }
        else
        {
            DetachAllElectrics();
            m_IsRunning = false;
        }

        // Global Notice
        GlobalEvent.NoticePowerPlantStateChanged();

    }

    public override void DestroySelf()
    {
        base.DestroySelf();

        // Global Notice
        GlobalEvent.NoticePowerPlantStateChanged();
    }

    public void AttachElectric(CSElectric cs)
    {
        if (m_Electrics.Exists(item0 => item0 == cs)) return;

        if (m_IsRunning && InRange(cs.Position)
            && m_RestPower >= cs.m_Power)
        {
            m_RestPower -= cs.m_Power;
            m_Electrics.Add(cs);
            cs.m_PowerPlant = this;
            cs.ChangeState();
        }
    }

    public void DetachElectric(CSElectric cs)
    {
        m_Electrics.Remove(cs);
        cs.m_PowerPlant = null;
        cs.ChangeState();
    }

    protected void FindElectrics()
    {
        foreach (KeyValuePair<CSConst.ObjectType, List<CSCommon>> kvp in Assembly.m_BelongObjectsMap)
        {
            if (CSAssembly.IsPowerPlant(kvp.Key))
                continue;

            foreach (CSCommon tempCsc in kvp.Value)
            {
                CSElectric cse = tempCsc as CSElectric;
                if (cse == null)
                    break;

                if (!cse.IsRunning)
                    AttachElectric(cse);
            }
        }
    }

    protected void DetachAllElectrics()
    {
        CSElectric[] electrics = m_Electrics.ToArray();
        foreach (CSElectric cse in electrics)
        {
            DetachElectric(cse);

            // Find other powerplant can supply this electric
            foreach (CSCommon power in cse.Assembly.AllPowerPlants)
            {
                if (power == this) continue;

                CSPowerPlant realPower = power as CSPowerPlant;
                realPower.AttachElectric(cse);
                if (cse.IsRunning)
                    break;
            }

            cse.ChangeState();
        }
    }

    #endregion

    #region OVERRIDE_FUNC

    protected virtual void ChargingItem(float deltaTime)
    {
        if (!GameConfig.IsMultiMode)
        {
            foreach (Energy energyItem in m_ChargingItems)
            {
                if (energyItem == null)
                    continue;
                energyItem.energy.Change(deltaTime * Info.m_ChargingRate*10000/Time.deltaTime);
				energyItem.energy.ChangePercent(deltaTime * Info.m_ChargingRate);
                // Normal Item Object
                //if (item.instanceId < CreationData.s_ObjectStartID)
                //{
                //    //float battery_power = item.GetProperty(ItemProperty.BatteryPower);
                //    //float battery_power_max = item.GetProperty(ItemProperty.BatteryPowerMax);

                //    //battery_power = Mathf.Min(battery_power_max, Mathf.Abs(battery_power + Info.m_ChargingRate * deltaTime));

                //    //item.SetProperty(ItemProperty.BatteryPower, battery_power);
                //}
                //// Creation Item Object
                //else
                //{
                //    CreationData cdata = CreationMgr.GetCreation(item.instanceId);
                //    if (cdata != null)
                //        cdata.m_Fuel = Mathf.Min(cdata.m_Attribute.m_MaxFuel, Mathf.Abs(cdata.m_Fuel + Info.m_ChargingRate * 10.0f * deltaTime));
                //}
            }
        }
        else
        {

        }


    }

    #endregion


    // GameTime
    //	private double m_StartTime;
    //	private bool m_First = true;

    public override void Update()
    {
        base.Update();

        if (!m_IsRunning)
            return;

        // Charge items
        //		if (!m_First)
        //		{
        //			float deltaTime = (float)(GameTime.Timer.Second - m_StartTime);
        //			ChargingItem(deltaTime);
        //
        //			m_StartTime = GameTime.Timer.Second;
        //		}
        //
        //		if (m_First && GameConfig.StartToCounter)
        //		{
        //			m_StartTime = GameTime.Timer.Second;
        //			m_First = false;
        //		}
        //		ChargingItem((float)CSMain.GameDeltaTime);
		if(Pathea.PeGameMgr.IsSingle)
        	ChargingItem(Mathf.Clamp(Time.deltaTime*(float)GameTime.Timer.ElapseSpeed, 0, 50));
    }
}
