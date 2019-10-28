using UnityEngine;
using System.Collections;


namespace GraphMapping
{
	//[ExecuteInEditMode]
	public class EditMapping : MonoBehaviour
	{
		public Texture2D mBiomeTex;
		public Texture2D mHeightTex;
		public Texture2D mAiSpawnTex;
		public bool mSaveData = false;

		public bool mTest =false;
		public Vector2 testPos = new Vector2(2000,2000);
		public Vector2 testWorldSize = new Vector2(18432,18432);

		bool SaveData()
		{
			PeMappingMgr.Instance.mBiomeMap.LoadTexData(mBiomeTex);
			PeMappingMgr.Instance.mHeightMap.LoadTexData(mHeightTex);
			PeMappingMgr.Instance.mAiSpawnMap.LoadTexData(mAiSpawnTex);
			return PeMappingMgr.Instance.SaveFile("D:/PeGraphMapping/");
		}
		void Awake()
		{
			PeMappingMgr.Instance.Init(testWorldSize);
		}

		void Start()
		{
//			int lo4,hi4;
//
//			hi4 = 4;
//			Debug.Log(hi4 + "---------------------------------hi4");
//			lo4 = 3;
//			Debug.Log(lo4 + "---------------------------------lo4");
//			byte b = (byte)(lo4 + (hi4<< 4));
//			Debug.Log(b + "---------------------------------");
//
//
//			hi4 = (b & 0xf0) >> 4;
//			lo4 = b & 0x0f;
//			Debug.Log(hi4 + "---------------------------------hi4");
//			Debug.Log(lo4 + "---------------------------------lo4");
		}


		void Update()
		{
			if (mSaveData)
			{
				bool ok = SaveData();
				string info = ok ? "ReLoad texture suceess!" : "ReLoad texture failed!";
				Debug.Log (info);
				mSaveData = false;
			}

			else if(mTest)
			{
				int  id  = PeMappingMgr.Instance.mAiSpawnMap.GetAiSpawnMapId(testPos,testWorldSize);
				Debug.Log(id);
				mTest = false ;
			}
		}
	}
}