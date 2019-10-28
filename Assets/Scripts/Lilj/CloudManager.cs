using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Data.SqliteClient;

public class Cloud3D
{
	public int 		mCloudType;
	public Color	mBaseColor;
	public Vector3 	mPosition;
	public string	mPerfabName;
}

public class CloudManager : MonoBehaviour
{
	public static Dictionary<int,Cloud3D> s_tblCloudList = new Dictionary<int, Cloud3D>();
	
	public Light			mSun;
	
	List<int> mUnInstantiateList = new List<int>();
	
	List<CloudController> mClouds = new List<CloudController>();
	
    //Player	mPlayer;
	
	//float	mLastCheckTime = 0;
	
	const float CheckDt = 5f;
	
	const float	ShowCloudDis = 2000f;
	
	void Awake()
	{
		mUnInstantiateList.Clear();
		foreach(int key in s_tblCloudList.Keys)
			mUnInstantiateList.Add(key);
		gameObject.SetActive(false);
	}
	
	void Update ()
	{
        //if(mPlayer == null)
        //{
        //    mPlayer = PlayerFactory.mMainPlayer;
        //    return;
        //}
		
        //if(Time.time - mLastCheckTime > CheckDt)
        //{
        //    mLastCheckTime = Time.time;
        //    Vector3 playerPos = mPlayer.transform.position;
        //    foreach(int key in mUnInstantiateList)
        //    {
        //        if(Vector3.Distance(playerPos,s_tblCloudList[key].mPosition) < ShowCloudDis)//CheckAddState
        //        {
        //            CreateCloud(s_tblCloudList[key]);
        //            mUnInstantiateList.Remove(key);
        //            break;
        //        }
        //    }
        //    foreach(CloudController cc in mClouds)//CheckActiveState
        //        cc.gameObject.SetActive(Vector3.Distance(playerPos,cc.transform.position) < ShowCloudDis);
        //}
	}
	
	void CreateCloud(Cloud3D cloud)
	{
		UnityEngine.Object obj = Resources.Load("Prefab/Cloud/" + cloud.mPerfabName);
		GameObject gobj = Instantiate(obj) as GameObject;
		CloudController AddItem = gobj.GetComponent<CloudController>();//;Instantiate(Resources.Load("Prefab/Cloud/" + cloud.mPerfabName)) as CloudController;
		AddItem.transform.parent = transform;
		AddItem.InitCloud(mSun,cloud);
		mClouds.Add(AddItem);
	}
	
	public static void LoadData()
	{
		s_tblCloudList.Clear();
		
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Cloud3D");
		
        while (reader.Read())
        {
            Cloud3D cloud = new Cloud3D();
            cloud.mCloudType = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Type")));
			string[] spiltStr = reader.GetString(reader.GetOrdinal("BaseColor")).Split(',');
            cloud.mBaseColor = new Color(Convert.ToSingle(spiltStr[0])/255f
				,Convert.ToSingle(spiltStr[1])/255f,Convert.ToSingle(spiltStr[2])/255f,Convert.ToSingle(spiltStr[3])/255f);
			spiltStr = reader.GetString(reader.GetOrdinal("Position")).Split(',');
            cloud.mPosition = new Vector3(Convert.ToSingle(spiltStr[0]),Convert.ToSingle(spiltStr[1]),Convert.ToSingle(spiltStr[2]));
            cloud.mPerfabName = reader.GetString(reader.GetOrdinal("PerfabName"));
            s_tblCloudList[Convert.ToInt32(reader.GetString(reader.GetOrdinal("Id")))] = cloud;
        }
	}
}
