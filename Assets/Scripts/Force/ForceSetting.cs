using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.IO;
using PETools;

public enum EPlayerType
{
	Neutral = 0,
	Human = 1,
	Computer = 2
}

public class ForceDesc
{
	public int ID;
	public Color32 Color;
	public string Name;
	public List<int> Allies = new List<int> ();
	public int JoinablePlayerCount = 0;
	public int JoinWorld = 0;
	public Vector3 JoinLocation = Vector3.zero;
	public bool PublicInventory = true;
	public bool ItemUseShare = true;
	public bool ItemShare = true;
	public bool InternalConflict = true;
	public bool AllyConflict = true;
	public bool EnemyConflict = true;


	public string JoinStr
	{
		get { return JoinWorld.ToString() + "|" + JoinLocationStr; }
		set
		{
			string[] ss = value.Split(new string[] {"|"}, System.StringSplitOptions.RemoveEmptyEntries);
			if (ss.Length > 1)
			{
				int.TryParse(ss[0], out JoinWorld);
				JoinLocationStr = ss[1];
			}
			else if (ss.Length == 1)
			{
				JoinWorld = 0;
				JoinLocationStr = ss[0];
			}
			else
			{
				JoinWorld = 0;
				JoinLocation = Vector3.zero;
			}
		}
	}
	
	public string JoinLocationStr
	{
		get { return JoinLocation.x.ToString("0.##") 
			+ "," + JoinLocation.y.ToString("0.##")
				+ "," + JoinLocation.z.ToString("0.##"); }
		set
		{
			try
			{
				string[] ss = value.Split(new string[] {","}, System.StringSplitOptions.RemoveEmptyEntries);
				if (ss.Length > 0)
					float.TryParse(ss[0], out JoinLocation.x);
				if (ss.Length > 1)
					float.TryParse(ss[1], out JoinLocation.y);
				if (ss.Length > 2)
					float.TryParse(ss[2], out JoinLocation.z);
			}
			catch { JoinLocation = Vector3.zero; }
		}
	}
}

public class PlayerDesc
{
	public int ID;
	public int Force;
    public string Name;
	public EPlayerType Type;
    public int StartWorld;
    public Vector3 StartLocation;

	public string StartStr
	{
		get { return StartWorld.ToString() + "|" + StartLocationStr; }
		set
		{
			string[] ss = value.Split(new string[] {"|"}, System.StringSplitOptions.RemoveEmptyEntries);
			if (ss.Length > 1)
			{
				int.TryParse(ss[0], out StartWorld);
				StartLocationStr = ss[1];
			}
			else if (ss.Length == 1)
			{
				StartWorld = 0;
				StartLocationStr = ss[0];
			}
			else
			{
				StartWorld = 0;
				StartLocation = Vector3.zero;
			}
		}
	}
	
	public string StartLocationStr
	{
		get { return StartLocation.x.ToString("0.##") 
			+ "," + StartLocation.y.ToString("0.##")
				+ "," + StartLocation.z.ToString("0.##"); }
		set
		{
			try
			{
				string[] ss = value.Split(new string[] {","}, System.StringSplitOptions.RemoveEmptyEntries);
				if (ss.Length > 0)
					float.TryParse(ss[0], out StartLocation.x);
				if (ss.Length > 1)
					float.TryParse(ss[1], out StartLocation.y);
				if (ss.Length > 2)
					float.TryParse(ss[2], out StartLocation.z);
			}
			catch { StartLocation = Vector3.zero; }
		}
	}
}

public class ForceSetting : Singleton<ForceSetting>
{
    //public Dictionary<int, ForceDesc> m_Forces;
    //public Dictionary<int, PlayerDesc> m_Players;

    public List<ForceDesc> m_Forces;
    public List<PlayerDesc> m_Players;

	public ForceSetting ()
	{
		m_Forces = new List<ForceDesc>();
		m_Players = new List<PlayerDesc>();
    }

    public int GetForceIndex(int id)
    {
        int n = m_Forces.Count;
        for (int i = 0; i < n; i++)
        {
            if (m_Forces[i].ID == id)
                return i;
        }

        return -1;
    }

	public void AddForceDesc (ForceDesc desc)
	{
        if (desc != null)
        {
            int index = GetForceIndex(desc.ID);
            if (index == -1)
                m_Forces.Add(desc);
            else
            {
                m_Forces[index] = desc;
				if (LogFilter.logDebug) Debug.LogWarningFormat("replace force id:{0}", desc.ID);
            }

			if(Pathea.PeGameMgr.IsMulti)
	            ReputationSystem.Instance.AddForceID(desc.ID);
        }
	}

    public int GetPlayerIndex(int id)
    {
		int n = m_Players.Count;
		for (int i = 0; i < n; i++) {
			if(m_Players[i].ID == id){
				return i;
			}
		}
		return -1;
    }

	public void AddPlayerDesc (PlayerDesc desc)
	{
        if (desc != null)
        {
            int index = GetPlayerIndex(desc.ID);
            if (index == -1)
                m_Players.Add(desc);
            else
            {
                m_Players[index] = desc;
                if (LogFilter.logDebug) Debug.LogWarningFormat("replace player id:{0}", desc.ID);
            }

			if (Pathea.PeGameMgr.IsMulti)
				ReputationSystem.Instance.AddForceID(desc.Force);
        }
	}

	public bool AllyPlayer (int srcPlayerID, int dstPlayerID)
	{
        int srcIndex = GetPlayerIndex(srcPlayerID);
        int dstIndex = GetPlayerIndex(dstPlayerID);
		if (srcIndex < 0 || dstIndex < 0)
		    return false;
        else
			return AllyForce (m_Players [srcIndex].Force, m_Players [dstIndex].Force);
	}

	public bool AllyForce (int srcForceID, int dstForceID)
	{
        int index = GetForceIndex(srcForceID);

		if (index < 0) 
		    return false;
        else
			return m_Forces [index].Allies.Contains (dstForceID);
	}

	public bool Ally (ForceDesc f1, ForceDesc f2)
	{
		if (f1 != null && f2 != null)
			return f1.Allies.Contains (f2.ID);
		else
			return false;
	}

	public int GetForceID (int playerID)
	{
        int index = GetPlayerIndex(playerID);
        if (index == -1)
            return -1;
        else
            return m_Players[index].Force;
	}

    public Color32 GetForceColor(int forceId)
    {
        int index = GetForceIndex(forceId);
        if(index >= 0)
            return m_Forces[index].Color;
        else
            return new Color32();
    }

	public EPlayerType GetForceType (int playerID)
	{
        int index = GetPlayerIndex(playerID);
        if (index == -1)
            return EPlayerType.Neutral;
        else
            return m_Players[index].Type;
	}

	public bool Conflict (int srcID, int dstID)
	{
        int spIdx = GetPlayerIndex(srcID);
        int dpIdx = GetPlayerIndex(dstID);
		if (spIdx >= 0 && dpIdx >= 0) {
            int sfIdx = GetForceIndex(m_Players[spIdx].Force);
            int dfIdx = GetForceIndex(m_Players[dpIdx].Force);

			if (sfIdx >= 0 && dfIdx >= 0) {
				if (AllyForce (m_Forces[sfIdx].ID, m_Forces[dfIdx].ID)) {
					if (m_Forces[sfIdx].ID == m_Forces[dfIdx].ID)
						return m_Forces[sfIdx].InternalConflict;
					else
						return m_Forces[sfIdx].AllyConflict;
				} else {
					return m_Forces[sfIdx].EnemyConflict;
				}
			} else {
				//Debug.LogError ("Can't find force id : " + m_Players[spIdx].Force + " & " + m_Players[dpIdx].Force);
			}
		} else {
			//Debug.LogError("Can't find player id : " + srcID + " & " + dstID);
		}

		return false;
	}

	public void Load(string s)
    {
        //if(Pathea.PeGameMgr.IsSingle)
        {
            m_Forces.Clear();
            m_Players.Clear();
        }       

        XmlDocument doc = new XmlDocument();
        StringReader reader = new StringReader(s);
        doc.Load(reader);

        XmlNode root = doc.SelectSingleNode("ForceSetting");

        //add force
        XmlNode forceNode = root.SelectSingleNode("Force");
        XmlNodeList forceList = ((XmlElement)forceNode).GetElementsByTagName("ForceDesc");

        foreach (XmlNode node in forceList)
        {
            XmlElement xe = node as XmlElement;

            ForceDesc force = new ForceDesc();
            force.ID = XmlUtil.GetAttributeInt32(xe, "ID");
            force.Name = XmlUtil.GetAttributeString(xe, "Name");
            force.Color = XmlUtil.GetNodeColor32(xe, "Color");
            force.Allies = XmlUtil.GetNodeInt32List(xe, "Ally");
            force.JoinablePlayerCount = XmlUtil.GetNodeInt32(xe, "JoinablePlayerCount");
			force.JoinStr =  XmlUtil.GetNodeString(xe, "JoinLocation");
            force.ItemUseShare = XmlUtil.GetNodeBool(xe, "ItemUseShare");
            force.ItemShare = XmlUtil.GetNodeBool(xe, "ItemShare");
            force.InternalConflict = XmlUtil.GetNodeBool(xe, "InternalConflict");
            force.AllyConflict = XmlUtil.GetNodeBool(xe, "AllyConflict");
            force.EnemyConflict = XmlUtil.GetNodeBool(xe, "EnemyConflict");

            force.Color.a = 255;

            AddForceDesc(force);
        }

        //add player
        XmlNode playerNode = root.SelectSingleNode("Player");
        XmlNodeList playerList = ((XmlElement)playerNode).GetElementsByTagName("PlayerDesc");
        foreach (XmlNode node in playerList)
        {
            XmlElement xe = node as XmlElement;

            PlayerDesc player = new PlayerDesc();
            player.ID = XmlUtil.GetAttributeInt32(xe, "ID");
            player.Name = XmlUtil.GetAttributeString(xe, "Name");
            player.Force = XmlUtil.GetAttributeInt32(xe, "Force");
			player.StartStr = XmlUtil.GetAttributeString(xe, "Start");

            player.Type = (EPlayerType)System.Enum.Parse(typeof(EPlayerType), XmlUtil.GetAttributeString(xe, "Type"));

            AddPlayerDesc(player);
        }

        reader.Close();
    }

	public void Load (TextAsset text)
	{
        Load(text.text);
	}

	public void OnLevelWasLoaded (int level)
	{
    }

    #region Network
	private bool HasForce (int id)
	{
		return Instance.GetForceIndex(id) != -1;
	}

    public List<PlayerDesc> HumanPlayer = new List<PlayerDesc>();
    public List<ForceDesc> HumanForce = new List<ForceDesc>();
    private static int m_TeamNum;
    private static int m_NumPerTeam;

    public void InitRoomForces(int teamNum, int numPerTeam)
    {
        m_TeamNum = teamNum;
        m_NumPerTeam = numPerTeam;

        HumanForce.Clear();
        HumanPlayer.Clear();

        if (Pathea.PeGameMgr.IsCustom)
        {
			m_Forces.Clear();
			m_Players.Clear();

			m_Forces.AddRange(Pathea.CustomGameData.Mgr.Instance.curGameData.mForceDescs);
			m_Players.AddRange(Pathea.CustomGameData.Mgr.Instance.curGameData.mPlayerDescs);

			foreach (PlayerDesc pd in m_Players)
			{
				if (pd.Type == EPlayerType.Human)
				{
					int fdIndex = GetForceIndex(pd.Force);
					if (-1 == fdIndex)
						continue;

					if (!HumanPlayer.Contains(pd))
						HumanPlayer.Add(pd);

					if (!HumanForce.Contains(m_Forces[fdIndex]))
						HumanForce.Add(m_Forces[fdIndex]);
				}
			}

			foreach (ForceDesc fd in m_Forces)
			{
				if (fd.JoinablePlayerCount != 0)
				{
					if (!HumanForce.Contains(fd))
						HumanForce.Add(fd);
				}
			}
		}
        else
        {
            if (Pathea.PeGameMgr.IsCooperation)
            {
                ForceDesc desc = new ForceDesc();

                desc.Allies = new List<int>();
                desc.ID = 1;

                desc.Allies.Add(desc.ID);
                desc.AllyConflict = true;
                desc.EnemyConflict = true;
                desc.InternalConflict = false;
                desc.ItemShare = true;
                desc.ItemUseShare = true;
                desc.JoinablePlayerCount = m_NumPerTeam;
                desc.Name = "Cooperation";
                desc.PublicInventory = true;
                desc.Color = Color.green;

                HumanForce.Add(desc);
            }
            else if (Pathea.PeGameMgr.IsSurvive)
            {
                int forceId = -1;
                ForceDesc desc = new ForceDesc();
                desc.ID = forceId;
                desc.Allies = new List<int>();
                desc.Allies.Add(forceId);
                desc.AllyConflict = false;
                desc.EnemyConflict = true;
                desc.InternalConflict = false;
                desc.ItemShare = false;
                desc.ItemUseShare = false;
                desc.JoinablePlayerCount = m_NumPerTeam;
                desc.Name = "Survive";
                desc.PublicInventory = false;
                desc.Color = Color.red;

                HumanForce.Add(desc);
            }
            else if (Pathea.PeGameMgr.IsVS)
            {
                int minTeamId = GroupNetwork.minTeamID;
                for (int i = 0; i < teamNum; i++)
                {
                    ++minTeamId;

                    ForceDesc desc = new ForceDesc();
                    desc.ID = minTeamId;
                    desc.Allies = new List<int>();
                    desc.Allies.Add(minTeamId);
                    desc.AllyConflict = false;
                    desc.EnemyConflict = true;
                    desc.InternalConflict = false;
                    desc.ItemShare = true;
                    desc.ItemUseShare = true;
                    desc.JoinablePlayerCount = m_NumPerTeam;
                    desc.Name = "Team" + minTeamId;
                    desc.PublicInventory = true;
                    desc.Color = Color.red;

                    HumanForce.Add(desc);
                }
            }
        }
    }

    public void InitGameForces(int curSurviveTeamId)
    {
		if (Pathea.PeGameMgr.IsCooperation)
		{
			int minTeamId = 1;
			AddPlayer(minTeamId, minTeamId, EPlayerType.Human, "Team" + minTeamId);
			AddForce(minTeamId, m_NumPerTeam, Pathea.PeGameMgr.EGameType.Cooperation);
		}
		else if (Pathea.PeGameMgr.IsVS)
		{
			int minTeamId = GroupNetwork.minTeamID;
			for (int i = 0; i < m_TeamNum; i++)
			{
				++minTeamId;
				AddPlayer(minTeamId, minTeamId, EPlayerType.Human, "Team" + minTeamId);
				AddForce(minTeamId, m_NumPerTeam, Pathea.PeGameMgr.EGameType.VS);
			}
		}
		else if (Pathea.PeGameMgr.IsCustom)
		{
			foreach (ForceDesc fd in HumanForce)
				AddForceDesc(fd);

			foreach (PlayerDesc pd in HumanPlayer)
				AddPlayerDesc(pd);
		}
		else if (Pathea.PeGameMgr.IsSurvive)
		{
			int i = GroupNetwork.minTeamID + 1;
			for (; i <= curSurviveTeamId; i++)
			{
				AddForce(i, Pathea.PeGameMgr.EGameType.Survive);
				AddPlayer(i, i, EPlayerType.Human, "Team" + i);
			}
		}
	}

	public static bool AddAllyPlayer(int srcPlayerID, int dstPlayerID)
	{
		if (srcPlayerID == dstPlayerID || null == Instance)
			return false;

		int srcIndex = Instance.m_Players.FindIndex(iter => iter.ID == srcPlayerID);
		int dstIndex = Instance.m_Players.FindIndex(iter => iter.ID == dstPlayerID);

		if (-1 != srcIndex && -1 != dstIndex)
			return AddAllyForce(Instance.m_Players[srcIndex].Force, Instance.m_Players[dstIndex].Force);

		return false;
	}

	public static bool AddAllyForce(int srcFroce, int dstForce)
	{
		if (srcFroce == dstForce || null == Instance)
			return false;

		int srcIndex = Instance.m_Forces.FindIndex(iter => iter.ID == srcFroce);
		int dstIndex = Instance.m_Forces.FindIndex(iter => iter.ID == dstForce);
		if (-1 != srcIndex && -1 != dstIndex && srcIndex != dstIndex)
		{
			if (!Instance.m_Forces[srcIndex].Allies.Contains(dstForce))
				Instance.m_Forces[srcIndex].Allies.Add(dstForce);

			if (!Instance.m_Forces[dstIndex].Allies.Contains(srcFroce))
				Instance.m_Forces[dstIndex].Allies.Add(srcFroce);

			return true;
		}

		return false;
	}

	public static bool RemoveAllyPlayer(int srcPlayerID, int dstPlayerID)
	{
		if (srcPlayerID == dstPlayerID || null == Instance)
			return false;

		int srcIndex = Instance.m_Players.FindIndex(iter => iter.ID == srcPlayerID);
		int dstIndex = Instance.m_Players.FindIndex(iter => iter.ID == dstPlayerID);

		if (-1 != srcIndex && -1 != dstIndex)
			return RemoveAllyForce(Instance.m_Players[srcIndex].Force, Instance.m_Players[dstIndex].Force);

		return false;
	}

	public static bool RemoveAllyForce(int srcFroce, int dstForce)
	{
		if (srcFroce == dstForce || null == Instance)
			return false;

		int srcIndex = Instance.m_Forces.FindIndex(iter => iter.ID == srcFroce);
		int dstIndex = Instance.m_Forces.FindIndex(iter => iter.ID == dstForce);
		if (-1 != srcIndex && -1 != dstIndex && srcIndex != dstIndex)
		{
			if (Instance.m_Forces[srcIndex].Allies.Contains(dstForce))
				Instance.m_Forces[srcIndex].Allies.Remove(dstForce);

			if (Instance.m_Forces[dstIndex].Allies.Contains(srcFroce))
				Instance.m_Forces[dstIndex].Allies.Remove(srcFroce);

			return true;
		}

		return false;
	}

	public static ForceDesc AddForce(int id, Pathea.PeGameMgr.EGameType type)
    {
        return AddForce(id, m_NumPerTeam, type);
    }

    public static ForceDesc AddForce(int id, int maxNum, Pathea.PeGameMgr.EGameType type)
	{
		if (Pathea.PeGameMgr.IsMultiCoop)
			id = 1;

		if (!Instance.HasForce(id))
		{
			ForceDesc desc = new ForceDesc();
			desc.ID = id;
			desc.Allies = new List<int>();
			desc.Allies.Add(id);
			desc.EnemyConflict = true;
			desc.InternalConflict = false;
			desc.JoinablePlayerCount = maxNum;
			desc.Name = id.ToString();

			switch (type)
			{
				case Pathea.PeGameMgr.EGameType.Cooperation:

					desc.AllyConflict = true;
					desc.ItemShare = true;
					desc.ItemUseShare = true;
					desc.PublicInventory = true;
					desc.Color = Color.green;
					break;

				case Pathea.PeGameMgr.EGameType.Survive:

					desc.AllyConflict = false;
					desc.ItemShare = false;
					desc.ItemUseShare = false;
					desc.PublicInventory = false;
					desc.Color = Color.red;
					break;

				case Pathea.PeGameMgr.EGameType.VS:

					desc.AllyConflict = false;
					desc.ItemShare = true;
					desc.ItemUseShare = true;
					desc.PublicInventory = true;
					desc.Color = Color.red;
					break;
			}

			Instance.AddForceDesc(desc);

            return desc;
		}

        int index = Instance.GetForceIndex(id);
        if (-1 != index)
            return Instance.m_Forces[index];
        else
            return null;
	}

	private bool HasPlayer (int id)
	{
		return Instance.GetPlayerIndex(id) != -1;
	}

	public static PlayerDesc AddPlayer(int id, int force, EPlayerType type, string name)
	{
        if (Pathea.PeGameMgr.IsMultiCoop)
            force = 1;

        int index = Instance.GetPlayerIndex(id);
		if (-1 == index)
		{
			PlayerDesc desc = new PlayerDesc();
			desc.ID = id;
			desc.Force = force;
			desc.Type = type;
			desc.Name = name;
			Instance.AddPlayerDesc(desc);
			return desc;
		}
        else
        {
            Instance.m_Players[index].Force = force;
			return Instance.m_Players[index];
        }
	}

    public static bool GetScenarioPos(int scenarioId, out Vector3 pos)
    {
        int index = Instance.m_Players.FindIndex(iter => iter.ID == scenarioId);
        if (-1 != index)
        {
            pos = Instance.m_Players[index].StartLocation;
            return true;
        }
        else
        {
            pos = Vector3.zero;
            return false;
        }
    }

    public static bool GetForcePos(int forceId, out Vector3 pos)
    {
        int index = Instance.m_Forces.FindIndex(iter => iter.ID == forceId);
        if (-1 != index)
        {
            pos = Instance.m_Forces[index].JoinLocation;
            return true;
        }
        else
        {
            pos = Vector3.zero;
            return false;
        }
    }

	public static PlayerDesc GetPlayerDesc(int id)
	{
		return Instance.m_Players.Find(iter => iter.ID == id);
	}

	public static ForceDesc GetForceDesc(int id)
	{
		return Instance.m_Forces.Find(iter => iter.ID == id);
	}
	#endregion
}


