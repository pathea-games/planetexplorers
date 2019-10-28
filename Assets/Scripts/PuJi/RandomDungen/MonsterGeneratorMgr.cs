using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
public class MonsterGeneratorMgr:MonoBehaviour
{
    public List<GameObject> allGenerators = new List<GameObject>();
    public List<int> weightList = new List<int>();
    public int pickMin;
    public int pickMax;
    void Awake()
    {
		foreach(GameObject go in allGenerators){
			if(go!=null)
				go.SetActive(true);
		}
    }
//    void Start()
//    {
//
//    }

//	public static void GenMonsters(MonsterGeneratorMgr genMgr){
//		if (genMgr.allGenerators.Count > 0)
//		{
//			if (genMgr.pickMin > genMgr.allGenerators.Count)
//				genMgr.pickMin = genMgr.allGenerators.Count;
//			if (genMgr.pickMax > genMgr.allGenerators.Count)
//				genMgr.pickMax = genMgr.allGenerators.Count;
//			System.Random rand = new System.Random((int)(System.DateTime.UtcNow.Ticks % Int32.MaxValue));
//			int pickAmount = genMgr.pickMin + rand.Next(genMgr.pickMax - genMgr.pickMin);
//			List<IdWeight> idWeightList = RandomDunGenUtil.GenIdWeight(genMgr.weightList);
//			
//			List<int> pickedId = RandomDunGenUtil.PickIdFromWeightList(rand,idWeightList, pickAmount);
//			for (int i = 0; i < genMgr.allGenerators.Count; i++)
//			{
//				if (pickedId.Contains(i))
//					genMgr.allGenerators[i].SetActive(true);
//				else
//					genMgr.allGenerators[i].SetActive(false);
//			}
//		}
//	}
}