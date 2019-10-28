using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public enum MapMaskType
{
	TransPoint = 1,
	Custom,
	Npc,
	Vehicle,
    Mark
}

public class MapIconData
{
	public int 			mId;
	public string		mIconName;
	
	public MapMaskType 	mMaskType;
	
	public static List<MapIconData> s_tblIconInfo;
	
	public static List<MapIconData> s_tblCustomInfo;
	
	public static void LoadDate()
	{
	    s_tblIconInfo = new List<MapIconData>();
		s_tblCustomInfo = new List<MapIconData>();
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Icon");
        while (reader.Read())
        {
            MapIconData fIcon = new MapIconData();
            fIcon.mId = Convert.ToInt32(reader.GetString(0));
            fIcon.mIconName = reader.GetString(1);
            fIcon.mMaskType = (MapMaskType)Convert.ToInt32(reader.GetString(2));
			if(fIcon.mMaskType == MapMaskType.Custom)
				s_tblCustomInfo.Add(fIcon);
            s_tblIconInfo.Add(fIcon);
        }
	}
}

public class MapMaskData
{
	internal int 		mId;
	internal int		mIconId;
	internal Vector3	mPosition;
	internal string		mDescription;
	internal float		mRadius;
    internal bool       mIsCamp;

	
	public static List<MapMaskData> s_tblMaskData;
    //public static List<MapMaskData> m_MapCampList = new List<MapMaskData>();
	public static void UnveilAll()
	{
		foreach(MapMaskData data in s_tblMaskData)
		{
			data.mRadius *= 100;
		}
	}
	
	public static void LoadDate()
	{
	    s_tblMaskData = new List<MapMaskData>();
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("MapIcon");
        while (reader.Read())
        {
            MapMaskData fMaskData = new MapMaskData();
            fMaskData.mId = Convert.ToInt32(reader.GetString(0));
            fMaskData.mDescription = PELocalization.GetString(Convert.ToInt32(reader.GetString(1)));
			string[] posStr = reader.GetString(2).Split(',');
            fMaskData.mPosition = new Vector3(Convert.ToSingle(posStr[0]),Convert.ToSingle(posStr[1]),Convert.ToSingle(posStr[2]));
            fMaskData.mIconId = Convert.ToInt32(reader.GetString(3));
			fMaskData.mRadius = Convert.ToSingle(reader.GetString(4));
            int itmp = Convert.ToInt32(reader.GetString(5));
            if (itmp == 1)
            {
                fMaskData.mIsCamp = true;
                //m_MapCampList.Add(fMaskData);
            }
            else
                fMaskData.mIsCamp = false;


            s_tblMaskData.Add(fMaskData);
        }
	}
}

public class CampPatrolData
{
	public int 		mId;
	public string	mDescription;
	public Vector3	mPosition;
	public float	mRadius;
	public List<Vector3> mPatrolList = new List<Vector3>();
	public List<string> mPatrolNpc = new List<string>();
	public int mPatrolNum;
    public int mPatrolTimeMin;
    public int mPatrolTimeMax;
    //public List<int> mEquipIDList = new List<int>();
    public List<float> mEatTimeMin = new List<float>();
    public List<float> mEatTimeMax = new List<float>();
    public List<float> mTalkTimeMin = new List<float>();
    public List<float> mTalkTimeMax = new List<float>();
    public int mSleepTime;
    public int mWakeupTime;
    public List<int> m_PreLimit = new List<int>();
    public List<int> m_TalkList = new List<int>();

	public static List<CampPatrolData> m_MapCampList;

    public static CampPatrolData GetCampData(int id)
    {
        for (int i = 0; i < m_MapCampList.Count; i++)
        {
            CampPatrolData cpData = m_MapCampList[i];
            if (cpData.mId == id)
                return cpData;
        }

        return null;
    }

	public static void LoadDate()
	{
		m_MapCampList = new List<CampPatrolData>();
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("CampPatrol");
		while (reader.Read())
		{
			CampPatrolData fMaskData = new CampPatrolData();
			fMaskData.mId = Convert.ToInt32(reader.GetString(0));
			fMaskData.mDescription = reader.GetString(1);
			string[] posStr = reader.GetString(2).Split(',');
			fMaskData.mPosition = new Vector3(Convert.ToSingle(posStr[0]),Convert.ToSingle(posStr[1]),Convert.ToSingle(posStr[2]));
			fMaskData.mRadius = Convert.ToSingle(reader.GetString(3));
			string strTmp = reader.GetString(4);
			string[] tmpList = strTmp.Split(';');
			for(int i=0; i<tmpList.Length; i++)
			{
				posStr = tmpList[i].Split(',');
				if(posStr.Length != 3)
					continue;
				
				fMaskData.mPatrolList.Add(new Vector3(Convert.ToSingle(posStr[0]),Convert.ToSingle(posStr[1]),Convert.ToSingle(posStr[2])));
			}
			
			strTmp = reader.GetString(5);
			tmpList = strTmp.Split(',');
			for(int i=0; i<tmpList.Length; i++)
			{
				if(tmpList[i] == "0")
					continue;
				
				fMaskData.mPatrolNpc.Add(tmpList[i]);
			}
			
			fMaskData.mPatrolNum = Convert.ToInt32(reader.GetString(6));

            //strTmp = reader.GetString(7);
            //tmpList = strTmp.Split(',');
            //for (int i = 0; i < tmpList.Length; i++)
            //{
            //    if (tmpList[i] == "0")
            //        continue;

            //    fMaskData.mEquipIDList.Add(Convert.ToInt32(tmpList[i]));
            //}

            strTmp = reader.GetString(8);
            tmpList = strTmp.Split('_');
            if(tmpList.Length == 2)
            {
                fMaskData.mPatrolTimeMin = Convert.ToInt32(tmpList[0]) * 3600;
                fMaskData.mPatrolTimeMax = Convert.ToInt32(tmpList[1]) * 3600;
            }

            strTmp = reader.GetString(9);
            tmpList = strTmp.Split('_');
            if (tmpList.Length == 2)
            {
                fMaskData.mSleepTime = Convert.ToInt32(tmpList[0]);
                fMaskData.mWakeupTime = Convert.ToInt32(tmpList[1]);
            }

            strTmp = reader.GetString(10);
            tmpList = strTmp.Split(',');
            string[] tmpList1;
            for (int i = 0; i < tmpList.Length; i++)
            {
                if (tmpList[i] == "0")
                    continue;

                tmpList1 = tmpList[i].Split('_');
                if (tmpList1.Length != 2)
                    continue;

                fMaskData.mTalkTimeMin.Add(Convert.ToSingle(tmpList1[0]));
                fMaskData.mTalkTimeMax.Add(Convert.ToSingle(tmpList1[1]));
            }

            strTmp = reader.GetString(11);
            tmpList = strTmp.Split(',');
            for (int i = 0; i < tmpList.Length; i++)
            {
                if (tmpList[i] == "0")
                    continue;

                tmpList1 = tmpList[i].Split('_');
                if (tmpList1.Length != 2)
                    continue;

                fMaskData.mEatTimeMin.Add(Convert.ToSingle(tmpList1[0]));
                fMaskData.mEatTimeMax.Add(Convert.ToSingle(tmpList1[1]));
            }

            string[] tmpList2;
            int tmpid;
            //id,id_talkid,talkid;id,id_talkid,talkid
            strTmp = reader.GetString(12);
            tmpList = strTmp.Split(';');
            for (int i = 0; i < tmpList.Length; i++)
            {
                if (tmpList[i] == "0")
                    continue;

                tmpList1 = tmpList[i].Split('_');
                if (tmpList1.Length != 2)
                    continue;

                tmpList2 = tmpList1[0].Split(',');
                for (int j = 0; j < tmpList2.Length; j++)
                    fMaskData.m_PreLimit.Add(Convert.ToInt32(tmpList2[j]));

                tmpList2 = tmpList1[1].Split(',');
                for (int j = 0; j < tmpList2.Length; j++)
                {
                    tmpid = Convert.ToInt32(tmpList2[j]);
                    fMaskData.m_TalkList.Add(tmpid);
                }
            }

			m_MapCampList.Add(fMaskData);
		}

	}
}