using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VArtifactTownXML;
using VANativeCampXML;
using TownData;

public class VATSaveData{
	public int townId;

	public int ms_id;
	public double lastHour;
	public double nextHour;
}

public class VArtifactTown
{
    public int templateId;
    public int townId;
	public int allyId;
	public int AllyColorId{
		get{return VATownGenerator.Instance.GetAllyColor(allyId);}
	}
	public int genAllyId;
	public int AllyId{
		set{
			allyId = value;
			genAllyId= value;
		}
		get{return allyId;}
	}
	public bool IsPlayerTown{
		get{return allyId ==TownGenData.PlayerAlly;}
	}
	public bool isMainTown= false;
	public int townNameId;
    public IntVector2 PosGen;
	public IntVector2 PosStart;
	public int areaId;
    public int level;
    public IntVector2 PosEnd;
	public IntVector2 PosCenter;
	public IntVector2 PosEntrance{
		get{return VAUnits[0].PosEntrance;}
	}
	public IntVector2 PosEntranceLeft{
		get{return VAUnits[0].PosEntrance+new IntVector2(-radius,0);}
	}
	public IntVector2 PosEntranceRight{
		get{return VAUnits[0].PosEntrance+new IntVector2(radius,0);}
	}

    public List<IntVector2> occupiedTile;
    public List<VArtifactUnit> VAUnits = new List<VArtifactUnit>();
    public int buildingNo = 0;
    public bool isRandomed = false;
    public VArtifactType type = VArtifactType.NpcTown;
    public NativeType nativeType = NativeType.Paja;
    //Monster Siege
    public int ms_id;
    public double lastHour;
    public double nextHour;
    public float lastCheckTime;

	//task
	public bool isEmpty = false;

    public VArtifactType Type
    {
        get { return type; }
        set { type = value; }
    }
    public List<VATownNpcInfo> npcList
    {
        get { return VAUnits[0].npcPosInfo.Values.ToList(); }
    }
//	public List<VABuildingInfo> buildingList
//	{
//		get{return VAUnits[0].building}
//	}

	public void SetMsId(int ms_id){
		this.ms_id = ms_id;
		VArtifactTownManager.Instance.SetSaveData(townId,ms_id);
	}

    public void RandomSiege(float minHour, float maxHour)
    {
        lastHour = GameTime.Timer.Hour;
        nextHour = UnityEngine.Random.Range(minHour, maxHour);
		VArtifactTownManager.Instance.SetSaveData(townId,lastHour,nextHour);
    }

    //List<AiNpcObject> townNpcList = new List<AiNpcObject>();
    //public void AddTownNpc(AiNpcObject npc)
    //{
    //    if (!townNpcList.Contains(npc))
    //        townNpcList.Add(npc);
    //}

    //public void RemoveTownNpc(AiNpcObject npc)
    //{
    //    townNpcList.Remove(npc);
    //}

    //public IEnumerable<AiNpcObject> GetAttackNpc()
    //{
    //    return townNpcList.Where(iter => iter.IsController && !iter.IsFollower);
    //}

    public NativeTower nativeTower;
    public int height = 1;
    public int Height
    {
        get { return height; }
    }

	public Vector3 TransPos { 
		get{return new Vector3 (PosCenter.x,height+2,PosCenter.y);}
	}
    public int radius;
	public int SmallRadius{
		get{
			return Mathf.Max(Mathf.Abs(PosEnd.x-PosStart.x),Mathf.Abs(PosEnd.y-PosStart.y))/2-8;
		}
	}
	public int MiddleRadius{
		get{
			return Mathf.Max(Mathf.Abs(PosEnd.x-PosStart.x),Mathf.Abs(PosEnd.y-PosStart.y))/2;
		}
	}
	public bool InYArea(float y){
		return y>=VAUnits[0].worldPos.y&&y<height+50;
	}
    public VArtifactTown()
    {

    }
    public VArtifactTown(VATown vat, IntVector2 posGen)
    {
        type = VArtifactType.NpcTown;
        templateId = vat.tid;
        level = vat.level;
        PosGen = posGen;
    }

    public VArtifactTown(NativeCamp nc, IntVector2 posGen)
    {
        type = VArtifactType.NativeCamp;
        templateId = nc.cid;
        level = nc.level;
        nativeTower = nc.nativeTower;
        PosGen = posGen;
        nativeType = (NativeType)nc.nativeType;
    }

    public bool IsExplored { get; set; }


	public static VArtifactTown GetStandTown(Vector3 pos){
		float distance = float.MaxValue;
		VArtifactTown vaTown = null;
		foreach(VArtifactTown vat in VArtifactTownManager.Instance.townPosInfo.Values){
			if(vat.type==VArtifactType.NpcTown){
				float d = (pos-vat.TransPos).magnitude;
				if(d<distance){
					distance = d;
					vaTown = vat;
				}
			}
		}
		if(vaTown!=null&&distance<vaTown.MiddleRadius)
			return vaTown;
		else
			return null;
	}
}



