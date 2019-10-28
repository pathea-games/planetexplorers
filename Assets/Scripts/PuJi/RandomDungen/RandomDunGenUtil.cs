using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea;

public struct IdWeight{
    public int id;
    public int weight;
    public IdWeight(int idP,int weightP){
        id = idP;
        weight = weightP;
    }
}

public class RandomDunGenUtil
{
    public static IdWeight GetIdWeightFromStr(string str){
        string[] strs = str.Split(',');
        int id = Convert.ToInt32(strs[0]);
        int weight = Convert.ToInt32(strs[1]);
        return new IdWeight(id,weight);
    }
	public static List<IdWeight> GetIdWeightList(string str){
		List<IdWeight> iwList = new List<IdWeight> ();
		if(str=="0")
			return iwList;
		string[] strs = str.Split(';');
		foreach(string st in strs)
		{
			IdWeight iw = GetIdWeightFromStr(st);
			iwList.Add(iw);
		}
		return iwList;
	}

    public static List<int> PickIdFromWeightList(System.Random rand, List<IdWeight> pool,int pickAmount) {
        //List<int> weightIndex = new List<int>();
        WeightPool wp = new WeightPool();
        foreach(IdWeight iw in pool){
            wp.Add(iw.weight, iw.id);
        }
        return wp.PickSomeId(rand,pickAmount);
    }

    public static List<IdWeight> GenIdWeight(List<int> weightList,List<int> idList = null) {
        List<IdWeight> idWeightList = new List<IdWeight>();
        if (idList == null)
        {
            for (int i = 0; i < weightList.Count; i++)
                idWeightList.Add(new IdWeight(i, weightList[i]));
        }
        else
        {
            for (int i = 0; i < weightList.Count; i++)
                idWeightList.Add(new IdWeight(idList[i], weightList[i]));
        }

        return idWeightList;
    }

	public static Vector3 GetPosOnGround(IntVector2 GenPos){
		int y = VFDataRTGen.GetPosHeight(GenPos,true);
		Vector3 pos = new Vector3(GenPos.x,y+4,GenPos.y);
		RaycastHit hit;
		if (Physics.Raycast(pos, Vector3.down, out hit, 512, 1 << Pathea.Layer.VFVoxelTerrain))
		{
			if(hit.point.y>0)
				pos.y = hit.point.y;
			else
				pos.y=y;
		}else{
			pos.y=y;
		}
		return pos;
	}

	public static bool GetAreaLowestPos(IntVector2 centerPos,int checkDistance,out Vector3 resultPos){
		resultPos = Vector3.zero;
		IntVector2 leftPos= centerPos+new IntVector2 (-checkDistance,0);
		IntVector2 rightPos= centerPos+new IntVector2 (checkDistance,0);
		IntVector2 frontPos= centerPos+new IntVector2 (0,checkDistance);
		IntVector2 backPos= centerPos+new IntVector2 (0,-checkDistance);
		Vector3 lPos = GetPosOnGround(leftPos);
		Vector3 rPos = GetPosOnGround(rightPos);
		Vector3 fPos = GetPosOnGround(frontPos);
		Vector3 bPos = GetPosOnGround(backPos);
		if(lPos.y<0||rPos.y<0||fPos.y<0||bPos.y<0)
			return false;
		float maxY = Mathf.Max(lPos.y,rPos.y,fPos.y,bPos.y);
		float minY = Mathf.Min(lPos.y,rPos.y,fPos.y,bPos.y);
		if(maxY>minY+4)
			return false;
		List<Vector3> posList = new List<Vector3>();
		posList.Add(lPos);
		posList.Add(rPos);
		posList.Add(fPos);
		posList.Add(bPos);
		Vector3 LowerstPos = posList.Find(delegate (Vector3 item) {
			foreach(Vector3 pos in posList){
				if(item.y>pos.y)
					return false;
			}
			return true;
		});
		resultPos = LowerstPos;
		return true;
	}

	public static int GetEntranceLevel(Vector3 genPos){
		System.Random rand = new System.Random ();
		float levelFactor = (VATownGenerator.Instance.GetLevelByRealPos(new IntVector2 (Mathf.RoundToInt(genPos.x),Mathf.RoundToInt(genPos.z)))+1)/(float)(1+TownGenData.AreaLevelMax);
		float posLevel = levelFactor*RandomDungeonDataBase.LEVEL_MAX;
		bool isPosBased = rand.NextDouble()<0.9f;
		if(isPosBased)
		{
			return Mathf.Clamp(Mathf.RoundToInt(posLevel+(float)(2*rand.NextDouble()-1)),1,RandomDungeonDataBase.LEVEL_MAX);
		}else{
			return Mathf.Clamp(Mathf.FloorToInt((float)rand.NextDouble()*RandomDungeonDataBase.LEVEL_MAX)+1,1,RandomDungeonDataBase.LEVEL_MAX);
		}
	}

	public static bool IsDungeonPosY(float posY){
		return posY<-100;
	}

	public static bool IsInDungeon(PeEntity entity){
		return IsDungeonPosY(entity.position.y);
	}
	public static bool IsInIronDungeon(){
		if(RandomDungenMgrData.dungeonBaseData==null){
			Debug.LogError("IsInIronDungeon:RandomDungenMgrData.dungeonBaseData==null");
			return false;
		}
		return RandomDungenMgrData.dungeonBaseData.IsIron;
	}

	public static DungeonType GetDungeonType(){
		if(RandomDungenMgrData.dungeonBaseData==null){
			Debug.LogError("GetDungeonType:RandomDungenMgrData.dungeonBaseData==null");
			return DungeonType.Iron;
		}
		return RandomDungenMgrData.dungeonBaseData.Type;
	}
}

