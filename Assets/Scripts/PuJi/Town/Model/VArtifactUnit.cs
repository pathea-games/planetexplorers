using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VArtifactTownXML;
using TownData;

public enum VArtifactType
{
    NpcTown,
    NativeCamp
}

public struct VAUnitId
{
    public int townId;
    public int unitIndex;
}
public class VArtifactUnit
{
    public int unitIndex;
    public int isoId;
    public string isoName;
    public ulong isoGuId;
    public VArtifactTown vat;
    public Vector3 worldPos;
    public float rot;
    public Dictionary<IntVector3, VFVoxel> townVoxel = new Dictionary<IntVector3, VFVoxel>();
    public Dictionary<Vector3, BuildingID> buildingPosID = new Dictionary<Vector3, BuildingID>();
    public Dictionary<Vector3, VATownNpcInfo> npcPosInfo = new Dictionary<Vector3, VATownNpcInfo>();
    public Dictionary<Vector3, NativePointInfo> nativePointInfo = new Dictionary<Vector3, NativePointInfo>();

    public IntVector2 isoStartPos;
    public IntVector2 isoEndPos;
    public IntVector2 PosStart;
    public IntVector2 PosEnd;
	public IntVector2 PosCenter;
	public IntVector2 PosEntrance{
		get{return new IntVector2 (PosCenter.x,PosStart.y-5);}
	}
    public int level;
	public int SmallRadius{
		get{
			return Mathf.Max(Mathf.Abs(PosEnd.x-PosStart.x),Mathf.Abs(PosEnd.y-PosStart.y))/2-10;
		}
	}
	public float GetCenterDistance(IntVector2 pointPos){
		return pointPos.Distance(PosCenter);
	}

	public bool NeedCut(int x, int z){
		return new IntVector2(x,z).Distance(PosCenter)<SmallRadius;
	}
    public bool isRandomed = false;
    public bool isAddedToRender = false;
    public bool isDoodadNpcRendered = false;
    public VArtifactType type = VArtifactType.NpcTown;

    //xmlData
    public List<BuildingIdNum> buildingIdNum;
    public List<NpcIdNum> npcIdNum;

    //DatabaseData
    public List<BuildingCell> buildingCell;
    public List<Vector3> npcPos;
    public IntVector3 vaSize;
    public Vector3 towerPos;

    public void Clear()
    {
        townVoxel.Clear();
    }
	public void SetHeight(float y){
		worldPos.y = y;
	}

    public bool IsInTown(int x, int z)
    {
        if (x >= PosStart.x && z >= PosStart.y
            &&
            x <= PosEnd.x && z <= PosEnd.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
