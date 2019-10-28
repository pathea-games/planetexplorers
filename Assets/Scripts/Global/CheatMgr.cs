using UnityEngine;
using Mono.Data.SqliteClient;
using System.Collections;
using System.Collections.Generic;
using Pathea;

public class CheatData
{
	public string cheatCode;
	public string successNotice;
	public int addType;
	public int itemID;
	public string isoName;

	static CheatData[] g_Datas;

	public static void LoadData()
	{
		List<CheatData> dataList = new List<CheatData>();
		
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("CheatData");
		while (reader.Read())
		{
			CheatData data = new CheatData();
			data.cheatCode = PETools.Db.GetString(reader, "Code").ToLower();
			data.successNotice = PELocalization.GetString(PETools.Db.GetInt(reader, "SuccessedNotice"));
			data.addType = PETools.Db.GetInt(reader, "AddType");
			data.itemID = PETools.Db.GetInt(reader, "ItemID");
			data.isoName = PETools.Db.GetString(reader, "ISOName");
			dataList.Add(data);
		}
		g_Datas = dataList.ToArray();
	}

	public static CheatData GetData(string code)
	{
		string lowerCode = code.ToLower();
		for(int i = 0; i < g_Datas.Length; ++i)
			if(g_Datas[i].cheatCode == lowerCode)
				return g_Datas[i];
		return null;
	}
}

public class CheatMgr : MonoBehaviour 
{
	void Awake()
	{
		UIMessageCtrl messageCtrl = GetComponent<UIMessageCtrl>();
		if(null != messageCtrl)
			messageCtrl.SeedMsgEvent += CheckCheat;
	}

	void CheckCheat(string msg)
	{
		CheckAddCheatItem(msg);
//			CheckMoveEntityByName(msg);
	}

	void CheckAddCheatItem(string msg)
	{
		if(null == MainPlayer.Instance.entity) return;

		CheatData data = CheatData.GetData(msg);
		if(null != data)
		{
			if(1 == data.addType)
			{
				if(MainPlayer.Instance.entity.packageCmpt.Add (data.itemID, 1) && "0" != data.successNotice)
					PeTipMsg.Register(data.successNotice, PeTipMsg.EMsgLevel.HighLightRed);
			}
			else if(2 == data.addType)
			{
				if(0 == VCEditor.MakeCreation("Isos/Mission/" + data.isoName) && "0" != data.successNotice)						
					PeTipMsg.Register(data.successNotice, PeTipMsg.EMsgLevel.HighLightRed);
			}
		}
	}

	void CheckMoveNPCByName(string msg)
	{
		string[] subStr = msg.Split(',');
		if(subStr.Length > 1 && subStr[1].Contains("come here"))
		{
			PeEntity entity = EntityMgr.Instance.Get(subStr[0]);
			if(null != entity)
				NpcMgr.CallBackNpcToMainPlayer(entity);
		}
	}
}
