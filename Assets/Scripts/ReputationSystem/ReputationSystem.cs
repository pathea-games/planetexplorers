using UnityEngine;
using System;
using System.IO;
using Pathea;
using System.Collections.Generic;

public class ReputationSystem : ArchivableSingleton<ReputationSystem>
{
	public enum TargetType
	{
		Puja = 5,
		Paja = 6,
	}

	public enum ReputationLevel
	{
		Fear = 0,
		Hatred,
		Animosity,
		Cold,
		Neutral,
		Cordial,
		Amity,
		Respectful,
		Reverence,
		MAX,
	}

	class ReputationCamp
	{
		public int reputationValue = ReputationSystem.DefaultReputationValue;
		public bool	belligerency = true;
		public ReputationLevel level = ReputationLevel.Cold;
		public int exValue = 0;

		public void ResetState()
		{
			level = ReputationSystem.ConvntValueToLevel ((exValue > 0)?exValue:reputationValue);
			belligerency = level < ReputationLevel.Neutral;
		}
	}

	class ReputationData
	{
		public 	bool active;
		Dictionary<int, ReputationCamp> m_ReputationCamps = new Dictionary<int, ReputationCamp>();

		public List<int> GetReputationCampIDs()
		{
			List<int> retList = new List<int>();
			foreach(int key in m_ReputationCamps.Keys)
				retList.Add(key);
			return retList;
		}

		public ReputationCamp GetReputationCamp(int playerID)
		{
			ReputationCamp retCamp = null;
			if(m_ReputationCamps.ContainsKey(playerID))
				retCamp = m_ReputationCamps[playerID];
			else
			{
				retCamp = new ReputationCamp();
				m_ReputationCamps[playerID] = retCamp;
			}
			return retCamp;
		}

		public void Import(BinaryReader _in, int version)
		{
			m_ReputationCamps.Clear();
			active = _in.ReadBoolean();
			if(version < 7)
			{
				for(int i = 0; i < 2; i++)
				{
					ReputationCamp camp = new ReputationCamp();
					m_ReputationCamps[i + 5] = camp;
					camp.reputationValue = _in.ReadInt32();
					if(version < 6)
						camp.belligerency = _in.ReadBoolean();
					else
						camp.exValue = _in.ReadInt32();
					camp.ResetState();
				}
			}
			else
			{
				int count = _in.ReadInt32();
				for(int i = 0; i < count; ++i)
				{
					int playerID = _in.ReadInt32();
					m_ReputationCamps[playerID] = new ReputationCamp();
					m_ReputationCamps[playerID].reputationValue = _in.ReadInt32();
					m_ReputationCamps[playerID].exValue = _in.ReadInt32();
					m_ReputationCamps[playerID].ResetState();
				}
			}
		}

		public void Export(BinaryWriter w)
		{
			w.Write(active);
			w.Write(m_ReputationCamps.Count);
			foreach(int key in m_ReputationCamps.Keys)
			{
				w.Write(key);
				w.Write(m_ReputationCamps[key].reputationValue);
				w.Write(m_ReputationCamps[key].exValue);
			}
		}
	}

	const int CURRENT_VERSION = 7;
	
	static readonly int[] ReputationLevelValue = new int[]{999, 36999, 57999, 63999, 66999, 72999, 84999, 105999, 106998};
	static readonly int[] ReputationLevelValueEX = new int[]{999, 36000, 21000, 6000, 3000, 6000, 12000, 21000, 999};

	public const float ChangeValueProportion = -1.2f;

	const int DefaultbelligerencyValue = 60999;

    const int DefaultReputationValue = 60999;

	const ReputationLevel DefaultReputationLevel = ReputationLevel.Hatred;
	
	public event Action<int,int> onReputationChange;

	Dictionary<int, ReputationData> m_ReputationDatas = new Dictionary<int, ReputationData>();
	
	List<int> m_ForceIDList = new List<int>();

	public static bool IsReputationTarget(int playerID) { return 5 == playerID || 6 == playerID || 20 <= playerID; }
	
	public void AddPlayerID(int playerID, bool netMsg = false)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		//Default 1
		if (forceID == -1 && !PeGameMgr.IsMulti)
			forceID = 1;
		AddForceID (forceID);

		//Net SendMsg
//		if(PeGameMgr.IsMulti && !netMsg)
//		{
//
//		}
	}

	public void AddForceID(int forceID)
	{
		if(!m_ForceIDList.Contains(forceID))
			m_ForceIDList.Add(forceID);
	}

    public bool HasReputation(int p1, int p2)
    {
		return GetActiveState(p1) && IsReputationTarget(p2);
	}

    bool HasReputation(int playerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		return m_ForceIDList.Contains(forceID);
	}

	public bool GetActiveState(int playerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return false;
		return GetReputationData(forceID).active;
	}

	public void ActiveReputation(int playerID)
	{
        int forceID = ForceSetting.Instance.GetForceID(playerID);
        if (!m_ForceIDList.Contains(forceID))
            return;
        if (PeGameMgr.IsMulti)
        {
            NetworkManager.SyncServer(EPacketType.PT_Reputation_SetActive, forceID);
        }
        else
        {           
            ReputationData data = GetReputationData(forceID);
            data.active = true;
        }
    }

	public void SetEXValue(int playerID, int targetPlayerID, int exValue)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if (!m_ForceIDList.Contains (forceID))
			return;
        if(PeGameMgr.IsMulti)
        {
			NetworkManager.SyncServer(EPacketType.PT_Reputation_SetExValue, forceID, targetPlayerID, exValue);
        }
		else
			SetEXValueByForce (forceID, targetPlayerID, exValue);
	}
	public void SetReputationValue(int playerID, int targetPlayerID, int value)
    {
        int forceID = ForceSetting.Instance.GetForceID(playerID);
        if (!m_ForceIDList.Contains(forceID))
            return;
        if(PeGameMgr.IsMulti)
        {
			NetworkManager.SyncServer(EPacketType.PT_Reputation_SetValue, forceID, targetPlayerID, value);
        }
        else
			SetReputationValueByForce(forceID, targetPlayerID, value);
    }
	public void CancelEXValue(int playerID, int targetPlayerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if (!m_ForceIDList.Contains (forceID))
			return;
        if (PeGameMgr.IsMulti)
        {
            NetworkManager.SyncServer(EPacketType.PT_Reputation_SetExValue, forceID, targetPlayerID, 0);
        }
        else
            SetEXValueByForce (forceID, targetPlayerID, 0);
	}
	
	/// <summary>
	/// Gets the reputation value.
	/// </summary>
	/// <returns>The abs reputation value.</returns>
	public int GetReputationValue(int playerID, int targetPlayerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return DefaultReputationValue;
		return GetReputationValueByForce(forceID, targetPlayerID);
	}

	public int GetExValue(int playerID, int targetPlayerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return DefaultReputationValue;
		ReputationData data = GetReputationData(forceID);
		return data.GetReputationCamp(targetPlayerID).exValue;
	}

	public int GetShowReputationValue(int playerID, int targetPlayerID)
	{
		int value = GetReputationValue(playerID, targetPlayerID);
		ReputationLevel level = ReputationSystem.ConvntValueToLevel(value);
		if(level == ReputationLevel.Fear)
			return value;
		int preValue = ReputationLevelValue[(int)level - 1];
		return value - preValue;
	}
	
	/// <summary>
	/// Gets the level threshold.
	/// </summary>
	/// <returns>The abs level threshold.</returns>
	public int GetShowLevelThreshold(int playerID, int targetPlayerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return DefaultReputationValue;
		int value = GetReputationValue(playerID, targetPlayerID);
		ReputationLevel level = ReputationSystem.ConvntValueToLevel(value);
		return GetLevelThreshold(level);
	}

    /// <summary>
    /// Gets the reputation level.
    /// </summary>
	public ReputationLevel GetReputationLevel(int playerID, int targetPlayerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return DefaultReputationLevel;
		return GetReputationLevelByForce(forceID, targetPlayerID);
	}

	public ReputationLevel GetShowLevel(int playerID, int targetPlayerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return DefaultReputationLevel;
		int value = GetReputationValue(playerID, targetPlayerID);
		return ReputationSystem.ConvntValueToLevel(value);
	}
	
	/// <summary>
	/// Gets the belligerency.
	/// </summary>
	public bool GetBelligerency(int playerID, int targetPlayerID)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return false;
		return GetBelligerencyByForce(forceID, playerID);
	}
	
	/// <summary>
	/// Modifies the reputation value.
	/// </summary>
	public void ChangeReputationValue(int playerID, int targetPlayerID, int addValue, bool changeOther = false)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return;
		ChangeReputationValueByForce(forceID, targetPlayerID, addValue, changeOther);
	}
	
	public bool TryChangeBelligerencyState(int playerID, int targetPlayerID, bool state)
	{
		int forceID = ForceSetting.Instance.GetForceID (playerID);
		if(!m_ForceIDList.Contains(forceID))
			return false;
		return TryChangeBelligerencyStateByForce(forceID, targetPlayerID, state); 
	}
	
	#region save load
	public void Import(byte[] data)
	{
		m_ReputationDatas.Clear();
		MemoryStream ms = new MemoryStream(data);
		BinaryReader _in = new BinaryReader(ms);

		int readVersion = _in.ReadInt32();
		int count = _in.ReadInt32();
		for(int i = 0; i < count; i++)
		{
			int forceID = _in.ReadInt32();
			AddForceID(forceID);
			ReputationData rData = GetReputationData(forceID);
			rData.Import(_in, readVersion);
		}
		_in.Close();
		ms.Close();
	}

	public void Export(BinaryWriter w)
	{
		w.Write((int)CURRENT_VERSION);
		w.Write(m_ReputationDatas.Count);
		foreach(int forceID in m_ReputationDatas.Keys)
		{
			w.Write(forceID);
			ReputationData data = GetReputationData(forceID);
			data.Export(w);
		}
	}

    protected override bool GetYird()
    {
        return false;
    }

	protected override void WriteData (BinaryWriter bw)
	{
		Export(bw);
	}

	protected override void SetData (byte[] data)
	{
		Import(data);
	}
	#endregion

	public static ReputationLevel ConvntValueToLevel(int value)
	{
		for(int i = 0; i < ReputationLevelValue.Length; i++)
			if(value < ReputationLevelValue[i])
				return (ReputationLevel)i;
		return ReputationLevel.Reverence;
	}

	public static int GetLevelThreshold(ReputationLevel level)
	{
		return ReputationLevelValueEX[(int)level];
	}

	ReputationData GetReputationData(int forceID)
	{
		if(!m_ReputationDatas.ContainsKey(forceID))
			m_ReputationDatas[forceID] = new ReputationData();
		return m_ReputationDatas[forceID];
	}

    /// <summary>
    /// Gets the reputation value.
    /// </summary>
    /// <returns>The abs reputation value.</returns>
    int GetReputationValueByForce(int forceID, int targetPlayerID)
	{
		ReputationData data = GetReputationData(forceID);
		return data.GetReputationCamp(targetPlayerID).reputationValue;
	}
	
	/// <summary>
	/// Gets the level threshold.
	/// </summary>
	/// <returns>The abs level threshold.</returns>
	int GetLevelThresholdByForce(int forceID, int targetPlayerID)
	{
		ReputationData data = GetReputationData(forceID);
		return GetLevelThreshold(data.GetReputationCamp(targetPlayerID).level);
	}

	/// <summary>
	/// Gets the reputation level.
	/// </summary>
	ReputationLevel GetReputationLevelByForce(int forceID, int targetPlayerID)
	{
		ReputationData data = GetReputationData(forceID);
		return data.GetReputationCamp(targetPlayerID).level;
	}

	/// <summary>
	/// Gets the belligerency.
	/// </summary>
	bool GetBelligerencyByForce(int forceID, int targetPlayerID)
	{
		ReputationData data = GetReputationData(forceID);
		return data.GetReputationCamp(targetPlayerID).belligerency;
	}

	/// <summary>
	/// Modifies the reputation value.
	/// </summary>
	void ChangeReputationValueByForce(int forceID, int targetPlayerID, int addValue, bool changeOther = false)
	{
		ReputationData data = GetReputationData(forceID);
		ReputationCamp camp = data.GetReputationCamp(targetPlayerID);
		SetReputationValueByForce(forceID, targetPlayerID, addValue + camp.reputationValue);
		if(data.active && changeOther && PeGameMgr.IsStory)
		{
			int otherChangeValue = (int)(addValue * ((addValue > 0) ? ChangeValueProportion : (1f / ChangeValueProportion)));

			List<int> playerIDList = data.GetReputationCampIDs();
			for(int i = 0; i < playerIDList.Count; i++)
			{
				if(playerIDList[i] != targetPlayerID)
				{
					ReputationCamp otherCamp = data.GetReputationCamp(playerIDList[i]);
					SetReputationValueByForce(forceID, playerIDList[i], otherChangeValue + otherCamp.reputationValue);
				}
			}
		}
	}

	bool TryChangeBelligerencyStateByForce(int forceID, int targetPlayerID, bool state)
	{
		ReputationData data = GetReputationData(forceID);
		if(data.GetReputationCamp(targetPlayerID).belligerency == state)
			return false;
		if(!state)
			return false;
		SetReputationValueByForce(forceID, targetPlayerID, DefaultbelligerencyValue);
		return true; 
	}

	void SetEXValueByForce(int forceID, int targetPlayerID, int value)
	{
		ReputationData data = GetReputationData(forceID);
		ReputationCamp camp = data.GetReputationCamp(targetPlayerID);
		camp.exValue = Mathf.Clamp(value, 0, ReputationLevelValue[ReputationLevelValue.Length - 1]);
		camp.ResetState();
	}

    void SetReputationValueByForce(int forceID, int targetPlayerID, int value)
    {
        ReputationData data = GetReputationData(forceID);
		ReputationCamp camp = data.GetReputationCamp(targetPlayerID);
		camp.reputationValue = Mathf.Clamp(value, 0, ReputationLevelValue[ReputationLevelValue.Length - 1]);
		camp.ResetState();
		if(null != onReputationChange)
			onReputationChange(forceID, targetPlayerID);
    }
    public void Active(int forceID)
    {
        if (!m_ForceIDList.Contains(forceID))
            return;
        ReputationData data = GetReputationData(forceID);
        data.active = true;
    }
    #region net

    public static void RPC_S2C_SyncValue(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        byte[] data = stream.Read<byte[]>();
        Instance.Import(data);
    }
    public static void RPC_S2C_SetValue(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int forceID = stream.Read<int>();
        int targetPlayerID = stream.Read<int>();
        int value = stream.Read<int>();
        Instance.SetReputationValueByForce(forceID, targetPlayerID, value);
    }
    public static void RPC_S2C_SetExValue(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int forceID = stream.Read<int>();
        int targetPlayerID = stream.Read<int>();
        int value = stream.Read<int>();
        Instance.SetEXValueByForce(forceID, targetPlayerID, value);
    }
    public static void RPC_S2C_SetActive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int forceID = stream.Read<int>();
        Instance.Active(forceID);
    }
    #endregion
}
