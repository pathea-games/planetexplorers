using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
public class DunItemGeneratorMgr : MonoBehaviour
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

//	public static void GenItem(DunItemGeneratorMgr digm){
//		if (digm.allGenerators.Count > 0)
//		{
//			if (digm.pickMin > digm.allGenerators.Count)
//				digm.pickMin = digm.allGenerators.Count;
//			if (digm.pickMax > digm.allGenerators.Count)
//				digm.pickMax = digm.allGenerators.Count;
//			System.Random rand = new System.Random((int)(System.DateTime.UtcNow.Ticks % Int32.MaxValue));
//			int pickAmount = digm.pickMin + rand.Next(digm.pickMax - digm.pickMin);
//			List<IdWeight> idWeightList = RandomDunGenUtil.GenIdWeight(digm.weightList);
//			
//			List<int> pickedId = RandomDunGenUtil.PickIdFromWeightList(rand, idWeightList, pickAmount);
//			for (int i = 0; i < digm.allGenerators.Count; i++)
//			{
//				if (pickedId.Contains(i))
//					digm.allGenerators[i].SetActive(true);
//				else
//					digm.allGenerators[i].SetActive(false);
//			}
//		}
//	}

}