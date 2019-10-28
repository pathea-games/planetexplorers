using UnityEngine;
using Mono.Data.SqliteClient;
using System.Collections;
using System.Collections.Generic;

public static class PELocalization
{
	static int gIdxId = -1;
	static int gIdxLangCur;
	static int gIdxLangEng;
	static List<string> gLangs;
	static Dictionary<string, int> gEnIdMap;
	static Dictionary<int, string> gCurStrMap = null;

	public static void LoadData(string strDbPath = null)
	{
		if (gIdxId >= 0) 	return;

		gIdxId = -1;
		gIdxLangCur = 0;
		gIdxLangEng = 0;
		gLangs = new List<string> ();
		gEnIdMap = new Dictionary<string, int> ();
		gCurStrMap = new Dictionary<int, string> ();

		SqliteAccessCS i18nDb = strDbPath != null ? new SqliteAccessCS(strDbPath) : LocalDatabase.Instance;
		SqliteDataReader reader = i18nDb.ReadFullTable("Translation");
		int n = reader.FieldCount;
		for(int i = 0; i < n; i++){
			string colName = reader.GetName(i).ToLower();
			if(colName == "id"){
				gIdxId = i;
			} else {
				gLangs.Add(colName);
				if(colName == SystemSettingData.Instance.mLanguage){	gIdxLangCur = i;	}
				if(colName == "english"){								gIdxLangEng = i;	}
			}
		}

        while (reader.Read()){
			int id = reader.GetInt32(gIdxId);
			gCurStrMap[id] = reader.GetString(gIdxLangCur);
			if(gIdxLangCur != gIdxLangEng){
				gEnIdMap[reader.GetString(gIdxLangEng).ToLower()] = id;
			}
		}
		reader.Close();

		if (strDbPath != null) {
			i18nDb.CloseDB();
		}
	}
	
	public static string GetString(int strId)
	{
		string retStr;
		if (gCurStrMap.TryGetValue (strId, out retStr))
			return retStr;
		
		//Debug.LogError("Localization ID: " + strId + " don't exist.");
		return string.Empty;
	}
	
	public static string ToLocalizationString(this string origin)
	{
		if(gIdxLangCur != gIdxLangEng)
		{
			//string lowerStr = origin.ToLower();
			int id;
			if(gEnIdMap.TryGetValue(origin.ToLower(), out id))
				return gCurStrMap[id];
		}
		return origin;
	}
}

