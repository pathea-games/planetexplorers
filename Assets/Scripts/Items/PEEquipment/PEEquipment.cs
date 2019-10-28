using UnityEngine;
using Mono.Data.SqliteClient;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using ItemAsset;

public interface ICloneModelHelper
{
	void ResetView();
}

public class EquipSetData
{
	public float attack;
	public float defence;
	public float maxHp;
	public float hpRecovery;
	public float maxStamina;
	public float staminaRecovery;
	public float maxHunger;
	public float hungerDownRate;
	public float digPower;
	public float chopPower;
	public float maxComfort;
	public float comfortSpendingRate;

	public string desStr;
	public int[] buffIDs;
	
	public static Dictionary<int, EquipSetData> g_EquipSetDatas;
	public static void LoadData()
	{
		g_EquipSetDatas = new Dictionary<int, EquipSetData>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("SingleSet");
		while (reader.Read())
		{
			int id = PETools.Db.GetInt(reader, "id");
			EquipSetData data = new EquipSetData();
			data.attack = PETools.Db.GetFloat(reader, "Attack");
			data.defence = PETools.Db.GetFloat(reader, "Defence");
			data.maxHp = PETools.Db.GetFloat(reader, "MaxHp");
			data.hpRecovery = PETools.Db.GetFloat(reader, "HpRecovery");
			data.maxStamina = PETools.Db.GetFloat(reader, "MaxStamina");
			data.staminaRecovery = PETools.Db.GetFloat(reader, "StaminaRecovery");
			data.maxHunger = PETools.Db.GetFloat(reader, "MaxHunger");
			data.hungerDownRate = PETools.Db.GetFloat(reader, "HungerDownRate");
			data.digPower = PETools.Db.GetFloat(reader, "DigPower");
			data.chopPower = PETools.Db.GetFloat(reader, "ChopPower");
			data.maxComfort = PETools.Db.GetFloat(reader, "MaxComfort");
			data.comfortSpendingRate = PETools.Db.GetFloat(reader, "ComfortSpendingRate");
			data.buffIDs = PETools.Db.GetIntArray(reader, "SkBuffId");
			data.ProductDes();
			g_EquipSetDatas[id] = data;
		}
	}

	public static EquipSetData GetData(ItemObject itemObj)
	{
		if(null == itemObj) return null;
		if(g_EquipSetDatas.ContainsKey(itemObj.protoId))
			return g_EquipSetDatas[itemObj.protoId];
		return null;
	}

	public static void GetSuitSetEffect(List<ItemObject> equipList, ref List<int> buffList)
	{		
		if(null == buffList) return;
		
		if(null == equipList) return;
			
		for(int itemIDIndex = 0; itemIDIndex < equipList.Count; ++itemIDIndex)
		{
			if(null == equipList[itemIDIndex]) continue;
			if(g_EquipSetDatas.ContainsKey(equipList[itemIDIndex].protoId) && null != g_EquipSetDatas[equipList[itemIDIndex].protoId].buffIDs)
				buffList.AddRange(g_EquipSetDatas[equipList[itemIDIndex].protoId].buffIDs);
		}
	}

    void ProductDes()
    {
        desStr = "";
        string attrValue = "";
        if (0 != attack)
        {
            attrValue = (attack * 100) + "%";
            if (attack > 0) attrValue = "+" + attrValue;
            desStr += string.Format(PELocalization.GetString(8000814), attrValue);
            desStr = string.Format(attack > 0? AttributeInfo.GreenColFormat:AttributeInfo.RedColFomat, desStr)+"\n";
        }
        if (0 != defence)
        {
            attrValue = (defence * 100) + "%";
            if (defence > 0) attrValue = "+" + attrValue;
            desStr += string.Format(PELocalization.GetString(8000815), attrValue);
            desStr = string.Format(defence > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != maxHp)
        {
            attrValue = maxHp > 0 ? "+" + maxHp.ToString() : maxHp.ToString();
            desStr += string.Format(PELocalization.GetString(8000816), attrValue);
            desStr = string.Format(maxHp > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != hpRecovery)
        {
            attrValue = hpRecovery > 0 ? "+" + hpRecovery.ToString() : hpRecovery.ToString();
            desStr += string.Format(PELocalization.GetString(8000817), attrValue);
            desStr = string.Format(hpRecovery > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != maxStamina)
        {
            attrValue = maxStamina > 0 ? "+" + maxStamina.ToString() : maxStamina.ToString();
            desStr += string.Format(PELocalization.GetString(8000818), attrValue);
            desStr = string.Format(maxStamina > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != staminaRecovery)
        {
            attrValue = (staminaRecovery * 100) + "%";
            if (staminaRecovery > 0) attrValue = "+" + attrValue;
            desStr += string.Format(PELocalization.GetString(8000819), attrValue);
            desStr = string.Format(staminaRecovery > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != maxHunger)
        {
            attrValue = maxHunger > 0 ? "+" + maxHunger.ToString() : maxHunger.ToString();
            desStr += string.Format(PELocalization.GetString(8000820), attrValue);
            desStr = string.Format(maxHunger > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != hungerDownRate)
        {
            attrValue = (hungerDownRate * 100) + "%";
            if (hungerDownRate > 0) attrValue = "+" + attrValue;
            desStr += string.Format(PELocalization.GetString(8000821), attrValue);
            //lz-2016.11.08 这个下降是增益bug
            desStr = string.Format(hungerDownRate < 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != digPower)
        {
            attrValue = (digPower * 100) + "%";
            if (digPower > 0) attrValue = "+" + attrValue;
            desStr += string.Format(PELocalization.GetString(8000822), attrValue);
            desStr = string.Format(digPower > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != chopPower)
        {
            attrValue = (chopPower * 100) + "%";
            if (chopPower > 0) attrValue = "+" + attrValue;
            desStr += string.Format(PELocalization.GetString(8000823), attrValue);
            desStr = string.Format(chopPower > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != maxComfort)
        {
            attrValue = maxComfort > 0 ? "+" + maxComfort.ToString() : maxComfort.ToString();
            desStr += string.Format(PELocalization.GetString(8000824), attrValue);
            desStr = string.Format(maxComfort > 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
        if (0 != comfortSpendingRate)
        {
            attrValue = (comfortSpendingRate * 100) + "%";
            if (comfortSpendingRate > 0) attrValue = "+" + attrValue;
            desStr += string.Format(PELocalization.GetString(8000825), attrValue);
            //lz-2016.11.08 这个下降是增益bug
            desStr = string.Format(comfortSpendingRate < 0 ? AttributeInfo.GreenColFormat : AttributeInfo.RedColFomat, desStr) + "\n";
        }
    }
}

public class SuitSetData
{
	public string suitSetName;
	public List<int> itemProtoList;
	public List<string> itemNames;
	public int[][] setBuffs;
	public int[] tips;
	public static SuitSetData[] g_SuitSetDatas;

	public struct MatchData
	{
		public string name;
		public List<string> itemNames;
		public List<bool> 	activeIndex;
		public int[] 		tips;
		public int 			activeTipsIndex;
        public List<int> itemProtoList;
    }

	public static void LoadData()
	{
		List<SuitSetData> dataList = new List<SuitSetData>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("SuitSet");
		int num = (reader.FieldCount - 3) / 2;
		while (reader.Read())
		{
			SuitSetData data = new SuitSetData();
			data.suitSetName = PELocalization.GetString(PETools.Db.GetInt(reader, "NameID"));
			data.itemProtoList = new List<int>();
			data.itemProtoList.AddRange(PETools.Db.GetIntArray(reader, "IdList"));
			data.itemNames = GetEquipName(data.itemProtoList);
			data.setBuffs = new int[num][];
			data.tips = new int[num];

			for(int i = 0; i < num; i++)
			{
				data.setBuffs[i] = PETools.Db.GetIntArray(reader, "set" + (i + 2));
				data.tips[i] = PETools.Db.GetInt(reader, "Tips" + (i + 2));
			}
			dataList.Add(data);
		}
		g_SuitSetDatas = dataList.ToArray();
	}

	static List<string> GetEquipName(List<int> itemProtoIDs)
	{
		List<string> retList = new List<string>();
		if(null != itemProtoIDs)
		{
			for(int i = 0; i < itemProtoIDs.Count; ++i)
			{
				ItemProto itemData = ItemProto.Mgr.Instance.Get(itemProtoIDs[i]);
				retList.Add(null != itemData ? itemData.GetName() : "");
			}
		}
		return retList;
	}

	public static SuitSetData GetData(int protoID)
	{
		if(null == g_SuitSetDatas) return null;

		for(int i = 0; i < g_SuitSetDatas.Length; ++i)
		{
			SuitSetData data = g_SuitSetDatas[i];
			
			if(null == data.itemProtoList) continue;

			if(data.itemProtoList.Contains(protoID))
				return data;
		}

		return null;
	}

	public static void GetSuitSetEffect(List<ItemObject> equipList, ref List<int> buffList, ref List<MatchData> matchList)
	{
		if(null == buffList) return;
		
		if(null == equipList) return;
		
		if(null == matchList) return;
		
		if(null == g_SuitSetDatas) return;

		List<bool> activeIndex = null;
		
		int hitCount = 0;
		
		for(int i = 0; i < g_SuitSetDatas.Length; ++i)
		{
			SuitSetData data = g_SuitSetDatas[i];
			
			if(null == data.itemProtoList) continue;

			if(null == activeIndex)
				activeIndex = new List<bool>();
			else
				activeIndex.Clear();
			
			hitCount = 0;

			
			for(int protoIDIndex = 0; protoIDIndex < data.itemProtoList.Count; ++protoIDIndex)
			{
				activeIndex.Add(false);
				for(int j = 0; j < equipList.Count; ++j)
				{
					if(null == equipList[j]) 
						continue;
					if(data.itemProtoList[protoIDIndex] ==  equipList[j].protoId)
					{
						++hitCount;
						activeIndex[protoIDIndex] = true;
						break;
					}
				}
			}

			if(0 < hitCount)
			{
				MatchData matchData = new MatchData();
                matchData.itemProtoList = data.itemProtoList;
                matchData.name = data.suitSetName;
				matchData.itemNames = data.itemNames;
				matchData.activeIndex = activeIndex;
				matchData.tips = data.tips;
				matchData.activeTipsIndex = 1 < hitCount ? hitCount - 2 : -1;
                matchList.Add(matchData);
				activeIndex = null;
				for(int j = 0; j <= matchData.activeTipsIndex; ++j)
					if(null != data.setBuffs[j])
						buffList.AddRange(data.setBuffs[j]);
			}
		}
	}
}


public class PEEquipment : MonoBehaviour, ICloneModelHelper
{
	public string 		m_EquipAnim = "";
	public ItemObject 	m_ItemObj;
	protected PeEntity	m_Entity;
	protected BiologyViewCmpt m_View;
	public EquipType	equipType{get{return m_ItemObj.protoData.equipType;}}	
	public bool			showOnVehicle = true;

	protected Renderer[] m_Renderers;
	protected bool[] m_RendersEnable;

	private bool[] m_HideMask = new bool[4]; // 0 Vehicle. 1 FirstPerson. 2 UnderWater 3 Ragdoll
	private bool m_HideState;

	private WhiteCat.CreationController m_CreationController;

	public virtual void ResetView()
	{
		for(int i = 0; i < m_HideMask.Length; i++)
			m_HideMask[i] = false;
		UpdateHideState();
	}
	
	public virtual void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		m_Entity = entity;
		m_ItemObj = itemObj;
		m_View = m_Entity.biologyViewCmpt;
		m_Renderers = GetComponentsInChildren<Renderer>(true);
		m_RendersEnable = new bool[m_Renderers.Length];
		for(int i = 0; i < m_RendersEnable.Length; ++i)
			m_RendersEnable[i] = m_Renderers[i].enabled;
		m_CreationController = GetComponent<WhiteCat.CreationController>();
	}

	public virtual void RemoveEquipment()
	{
		m_View.DetachObject(gameObject);
		GameObject.Destroy(gameObject);
	}

	public virtual bool CanTakeOff() { return true; }

	public virtual void SetActiveState(bool active) { }

	public void HideEquipmentByVehicle(bool hide)
	{
		if(showOnVehicle)
			return;
		m_HideMask[0] = hide;
		UpdateHideState();
	}

	public void HideEquipmentByFirstPerson(bool hide)
	{
		m_HideMask[1] = hide;
		UpdateHideState();
	}

	public void HidEquipmentByUnderWater(bool hide)
	{
		m_HideMask[2] = hide;
		UpdateHideState();
	}

	public void HidEquipmentByRagdoll(bool hide)
	{
		m_HideMask[3] = hide;
		UpdateHideState();
	}

	protected virtual void UpdateHideState()
	{
		if(null == m_Renderers || null == m_HideMask)
			return;
		bool maskstate = false;
		for(int i = 0; i < m_HideMask.Length; i++)
		{
			if(m_HideMask[i])
			{
				maskstate = true;
				break;
			}
		}

		if(maskstate == m_HideState)
			return;

		m_HideState = maskstate;

		if(null != m_CreationController)
			m_CreationController.visible = !m_HideState;

		for(int i  = 0; i < m_Renderers.Length; i++)
			if(m_Renderers[i] != null && m_Renderers[i].enabled != !m_HideState)
				m_Renderers[i].enabled = !m_HideState && m_RendersEnable[i];
	}
}
