using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea.Maths;
using System.IO;
using System.Collections;
using Mono.Data.SqliteClient;
using VANativeCampXML;
using VArtifactTownXML;
using TownData;
using Pathea;

public class Town_artifacts
{
    public int ID;
    public string isoName;
    public IntVector3 vaSize;
    public List<BuildingCell> buildingCell;
    public List<Vector3> npcPos;
    public Vector3 towerPos;
}
public struct AllyName{
	public int id;
	public int raceId;
	public int nameId;
}

public class BuildingCell
{
    public Vector3 cellPos;
    public float cellRot;
}

public class WeightPool
{
    public int maxValue = 0;
    List<int> weightTree = new List<int>();
    List<int> idTree = new List<int>();
    public int count = 0;

    public void Add(int weight, int id)
    {
        maxValue += weight;
        weightTree.Add(maxValue);
        idTree.Add(id);
        count++;
    }

    public void Clear()
    {
        maxValue = 0;
        weightTree = new List<int>();
        idTree = new List<int>();
        count = 0;
    }

    public int GetRandID(System.Random randSeed)
    {
        if (maxValue == 0)
        {
            return -1;
        }
        int value = randSeed.Next(maxValue);
        for (int i = 0; i < count; i++)
        {
            if (value < weightTree[i])
            {
                return idTree[i];
            }
        }
        return -1;
    }
    public List<int> PickSomeId(System.Random randSeed, int pickAmount) {
        List<int> pickedId = new List<int>();
        List<int> tempWeightTree = new List<int> (weightTree);
        List<int> tempIdTree = new List<int> (idTree);
        int tempMax = maxValue;
        int tempCount =count;
        for (int i = 0; i < pickAmount; i++)
        {
            int value = randSeed.Next(tempMax);
            for(int j=0;j<tempCount;j++){
                if (value < tempWeightTree[j])
                {
                    pickedId.Add(tempIdTree[j]);
                    int pickedWeight;
                    if(j==0)
                        pickedWeight = tempWeightTree[0];
                    else
                        pickedWeight = tempWeightTree[j]-tempWeightTree[j-1];
                    for(int m = j+1;m<tempCount;m++){
                        tempWeightTree[m] -= pickedWeight;   
                    }
                    tempWeightTree.RemoveAt(j);
                    tempIdTree.RemoveAt(j);
                    tempMax -= pickedWeight;
                    tempCount--;
                    break;
                }
            }
        }
        return pickedId;
    }

}

public struct DynamicNativePoint
{
    public Vector3 point;
    public int id;
    public int type;//0 group,1 single
}

public class VArtifactUtil
{
    public static String ISOPath = "";

    public static String GetISONameFullPath(string filenameWithoutExtension)
    {
        return ISOPath + "/" + filenameWithoutExtension + ".art";
    }

    public static Dictionary<int, Town_artifacts> townArtifactsData = new Dictionary<int, Town_artifacts>();//load from database
	public static Dictionary<int,int> townNameData = new Dictionary<int, int>();
	public static Dictionary<int,AllyName> allyNameData = new Dictionary<int, AllyName>();

    public static Dictionary<string, ulong> isoNameId = new Dictionary<string, ulong>();
    public static Dictionary<ulong, VArtifactData> isos = new Dictionary<ulong, VArtifactData>();
    public static Dictionary<int, ulong> townIdIso = new Dictionary<int, ulong>();
    public static Dictionary<Vector3, int> loadedPos = new Dictionary<Vector3, int>();
    public static Dictionary<IntVector3, VFVoxel> artTown = new Dictionary<IntVector3, VFVoxel>();
    public static int townCount = 0;
    //public VArtifactCursor.OnOutputVoxel OutputTownVoxel;

    public static int spawnRadius0 = 0;
    public static int spawnRadius = 0;



    public static string[] triplaner = new string[]{"4,3,68,24,17,23,23,23",
			"8,6,66,21,22,22,22,66",
			"14,15,12,13,12,12,13,15",
			"26,27,65,25,65,25,65,65",
			"9,5,22,22,17,23,22,24",
			"8,17,22,66,66,24,23,66",
		//"32,31,22,22,66,66,23,23",
		//"7,7,66,66,22,22,23,23",
			"2,5,18,24,17,18,24,35",
			"20,59,67,67,20,67,67,67"};
    public static int triplanerIndex_grassLand = 0;
    public static int triplanerIndex_forest = 1;
    public static int triplanerIndex_dessert = 2;
    public static int triplanerIndex_redStone = 3;
    public static int triplanerIndex_rainforest = 4;
	public static int triplanerIndex_hill = 5;
	public static int triplanerIndex_swamp = 6;
	public static int triplanerIndex_crater = 7;
	
	
	public static void Clear()
    {
        isoNameId.Clear();
        isos.Clear();
        townIdIso.Clear();
        loadedPos.Clear();
        artTown.Clear();
        townCount = 0;
    }


    public static void OutputTownVoxel(int x, int y, int z, VCVoxel voxel, VArtifactUnit town)
    {
        IntVector3 pos = new IntVector3(x, y, z);
        VFVoxel vfvoxel = new VFVoxel();
        vfvoxel.Type = voxel.Type;
        vfvoxel.Volume = voxel.Volume;
        if (town.townVoxel.ContainsKey(pos))
        {
            VFVoxel originVoxel = town.townVoxel[pos];
            vfvoxel.Volume = (byte)Mathf.Clamp(vfvoxel.Volume + originVoxel.Volume, 0, 255);
        }
        town.townVoxel[pos] = vfvoxel;
        //Debug.LogError("addarttown:"+pos);
    }
    //Vector3 xdir = Vector3.right;
    //Vector3 ydir = Vector3.up;
    //Vector3 zdir = Vector3.forward;


    //q.eulerAngles = new Vector3 (0,50,0);
    //xdir = (q * xdir).normalized;
    //ydir = (q * ydir).normalized;
    //zdir = (q * zdir).normalized;

    //Matrix4x4 mat = new Matrix4x4 ();
    //mat.SetTRS(p_artifact, q, Vector3.one);
    //t.position = mat.MultiplyPoint(p0);
    //Transform t;
    //t.rotation = q;

    //public Vector3 Size { get { return ISO.m_HeadInfo.si; } }
    //public static Vector3 Origin
    //{
    //    get { return worldPos; }
    //}
    //public static Vector3 XDir
    //{
    //    get { return (q * Vector3.right).normalized; }
    //}
    //public static Vector3 YDir
    //{
    //    get { return (q * Vector3.up).normalized; }
    //}
    //public static Vector3 ZDir
    //{
    //    get { return (q * Vector3.forward).normalized; }
    //}

    //public static Vector3 XDir
    //{
    //    get { return Vector3.right; }
    //}
    //public static Vector3 YDir
    //{
    //    get { return Vector3.up; }
    //}
    //public static Vector3 ZDir
    //{
    //    get { return Vector3.forward; }
    //}

    public VArtifactUtil()
    {
    }

    public static bool LoadIso(string path)
    {
        VArtifactData ISO;
        long tick = System.DateTime.Now.Ticks;
        try
        {
            ISO = new VArtifactData();
            string fullpath = path;
            string filename = Path.GetFileNameWithoutExtension(fullpath);
            //TextAsset ta = Resources.Load(fullpath,typeof(TextAsset)) as TextAsset;
            using (FileStream fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read))
            {
                byte[] iso_buffer = new byte[(int)(fs.Length)];
                //byte[] iso_buffer = ta.bytes;
                ulong guid = CRC64.Compute(iso_buffer);
                fs.Read(iso_buffer, 0, (int)(fs.Length));
                fs.Close();
                if (ISO.Import(iso_buffer, new VAOption(false)))
                {
                    isos[guid] = ISO;
                    isoNameId[filename] = guid;
                    Debug.Log("loadIso Time: " + (System.DateTime.Now.Ticks - tick));
                    return true;
                }
                else
                {
                    ISO = null;
                    return false;
                }
            }
        }
        catch (Exception e)
        {
			Debug.LogError("Failed to load file "+path);
			GameLog.HandleIOException(e, GameLog.EIOFileType.InstallFiles);
            ISO = null;
            return false;
        }
    }

    public static VArtifactData GetIsoData(string isoName, out ulong guid)
    {
        guid = GetGuidFromIsoName(isoName);
        if (guid == 0)
        {
            Debug.LogError("isoName not exist!");
            return null;
        }
        if (!isos.ContainsKey(guid))
        {
            LoadIso(GetISONameFullPath(isoName));
        }
        VArtifactData isoData = isos[guid];
        return isoData;
    }

    public static void ClearAllISO()
    {
        isos.Clear();
    }

    public static void ClearISO(string filename)
    {
        isos.Remove(isoNameId[filename]);
    }

    public static void ClearISO(ulong isoGuId)
    {
        isos.Remove(isoGuId);
    }
    //public static void OutputVoxels(Vector3 worldPos, ulong guid, VArtifactTown newTown)
    //{
    //    ISO = isos[guid];
    //    OutputVoxels(worldPos,newTown);
    //}

    //public static void OutputVoxels(Vector3 worldPos, int townId, VArtifactTown newTown)
    //{
    //    ulong guid = townIdIso[townId];
    //    ISO = isos[guid];
    //    OutputVoxels(worldPos,newTown);
    //}


    public static void OutputVoxels(Vector3 worldPos, VArtifactUnit newTown, float rotation = 0)
    {
        if (!isos.ContainsKey(newTown.isoGuId))
            LoadIso(GetISONameFullPath(newTown.isoName));
        if (!isos.ContainsKey(newTown.isoGuId))
        {
            Debug.LogError("isoGuId error: " + newTown.isoGuId + "isoName: " + newTown.isoName);
            return;
        }
        VArtifactData isoData = isos[newTown.isoGuId];
        //long tick = System.DateTime.Now.Ticks;
        Quaternion q = new Quaternion();
        q.eulerAngles = new Vector3(0, rotation, 0);
        Vector3 XDir = (q * Vector3.right).normalized;
        Vector3 YDir = (q * Vector3.up).normalized;
        Vector3 ZDir = (q * Vector3.forward).normalized;
        //Vector3 XDir = Vector3.right;
        //Vector3 YDir = Vector3.up;
        //Vector3 ZDir = Vector3.forward;
        Vector3 ofs = new Vector3(isoData.m_HeadInfo.xSize, 0, isoData.m_HeadInfo.zSize) * (-0.5f);
        Vector3 new_pos = worldPos + q * ofs;

        foreach (KeyValuePair<int, VCVoxel> kvp in isoData.m_Voxels)
        {
            Vector3 lpos = new Vector3(kvp.Key & 0x3ff, kvp.Key >> 20, (kvp.Key >> 10) & 0x3ff);
            Vector3 wpos = new_pos
                + lpos.x * XDir
                + lpos.y * YDir
                + lpos.z * ZDir;

            INTVECTOR3 wpos_floor = new INTVECTOR3(Mathf.FloorToInt(wpos.x), Mathf.FloorToInt(wpos.y), Mathf.FloorToInt(wpos.z));
            INTVECTOR3 wpos_ceil = new INTVECTOR3(Mathf.CeilToInt(wpos.x), Mathf.CeilToInt(wpos.y), Mathf.CeilToInt(wpos.z));

            if (wpos_floor == wpos_ceil)
            {
                OutputTownVoxel(wpos_floor.x, wpos_floor.y, wpos_floor.z, kvp.Value, newTown);
            }
            else
            {
                for (int x = wpos_floor.x; x <= wpos_ceil.x; ++x)
                {
                    for (int y = wpos_floor.y; y <= wpos_ceil.y; ++y)
                    {
                        for (int z = wpos_floor.z; z <= wpos_ceil.z; ++z)
                        {
                            float deltax = 1 - Mathf.Abs(wpos.x - x);
                            float deltay = 1 - Mathf.Abs(wpos.y - y);
                            float deltaz = 1 - Mathf.Abs(wpos.z - z);
                            float u = deltax * deltay * deltaz;
                            if (u < 0.5f)
                                u = u / (0.5f + u);
                            else
                                u = 0.5f / (1.5f - u);
                            VCVoxel voxel = kvp.Value;
                            voxel.Volume = (byte)Mathf.CeilToInt(voxel.Volume * u);
                            if (voxel.Volume > 1)
                                OutputTownVoxel(x, y, z, voxel, newTown);
                        }
                    }
                }
            }
        }
        // Debug.LogError("Output Time: " + (System.DateTime.Now.Ticks - tick) + " townCount:" + (++townCount));
    }

    public static List<IntVector2> OccupiedTile(List<VArtifactUnit> artifactList)
    {
        List<IntVector2> OccupiedTileList = new List<IntVector2>();
        for (int i = 0; i < artifactList.Count; i++)
        {
            List<IntVector2> aList = LinkedChunkIndex(artifactList[i]);
            for (int j = 0; j < aList.Count; j++)
            {
                if (!OccupiedTileList.Contains(aList[j]))
                {
                    OccupiedTileList.Add(aList[j]);
                }
            }
        }
        return OccupiedTileList;
    }
    public static List<IntVector2> LinkedChunkIndex(VArtifactUnit townInfo)
    {
        IntVector2 startPos = townInfo.PosStart;
        IntVector2 endPos = townInfo.PosEnd;

        List<IntVector2> startIndexList = Link1PointToChunk(startPos);
        List<IntVector2> endIndexList = Link1PointToChunk(endPos);

        IntVector2 startIndex = GetMinChunkIndex(startIndexList);
        IntVector2 endIndex = GetMaxChunkIndex(endIndexList);

        List<IntVector2> chunkIndexList = GetChunkIndexListFromStartEnd(startIndex, endIndex);

        return chunkIndexList;
    }

    public static List<IntVector2> GetChunkIndexListFromStartEnd(IntVector2 startIndex, IntVector2 endIndex)
    {
        List<IntVector2> ChunkIndexList = new List<IntVector2>();
        for (int x = startIndex.x; x <= endIndex.x; x++)
        {
            for (int z = startIndex.y; z <= endIndex.y; z++)
            {
                IntVector2 index = new IntVector2(x, z);
                if (!ChunkIndexList.Contains(index))
                    ChunkIndexList.Add(index);
            }
        }
        return ChunkIndexList;
    }

    public static IntVector2 GetMinChunkIndex(List<IntVector2> startIndexList)
    {
        if (startIndexList == null || startIndexList.Count <= 0)
            return null;
        IntVector2 minIndex = new IntVector2();
        minIndex = startIndexList[0];
        for (int i = 0; i < startIndexList.Count; i++)
        {
            if (startIndexList[i].x <= minIndex.x && startIndexList[i].y <= minIndex.y)
                minIndex = startIndexList[i];
        }

        return minIndex;
    }

    public static IntVector2 GetMaxChunkIndex(List<IntVector2> endIndexList)
    {
        if (endIndexList == null || endIndexList.Count <= 0)
            return null;
        IntVector2 maxIndex = new IntVector2();
        maxIndex = endIndexList[0];
        for (int i = 0; i < endIndexList.Count; i++)
        {
            if (endIndexList[i].x >= maxIndex.x && endIndexList[i].y >= maxIndex.y)
                maxIndex = endIndexList[i];
        }

        return maxIndex;
    }


    public static List<IntVector2> Link1PointToChunk(IntVector2 pos)
    {
        List<IntVector2> indexList = new List<IntVector2>();
        int i = pos.x;
        int j = pos.y;
        int x = i >> VoxelTerrainConstants._shift;
        int z = j >> VoxelTerrainConstants._shift;
        IntVector2 chunkIndexXZ = new IntVector2(x, z);
        indexList.Add(chunkIndexXZ);

        int x2 = x;
        if ((i + VoxelTerrainConstants._numVoxelsPrefix) % VoxelTerrainConstants._numVoxelsPerAxis == 0)
        {
            x2 = (i + VoxelTerrainConstants._numVoxelsPrefix) >> VoxelTerrainConstants._shift;
        }
        else if (i % VoxelTerrainConstants._numVoxelsPerAxis < VoxelTerrainConstants._numVoxelsPostfix)
        {
            x2 = (i - 2) >> VoxelTerrainConstants._shift;
        }

        int z2 = z;
        if ((j + VoxelTerrainConstants._numVoxelsPrefix) % VoxelTerrainConstants._numVoxelsPerAxis == 0)
        {
            z2 = (j + VoxelTerrainConstants._numVoxelsPrefix) >> VoxelTerrainConstants._shift;
        }
        else if (j % VoxelTerrainConstants._numVoxelsPerAxis < VoxelTerrainConstants._numVoxelsPostfix)
        {
            z2 = (j - VoxelTerrainConstants._numVoxelsPostfix) >> VoxelTerrainConstants._shift;
        }


        if (x2 != x && z2 == z)
        {
            chunkIndexXZ = new IntVector2(x2, z);
            if (!indexList.Contains(chunkIndexXZ))
            {
                indexList.Add(chunkIndexXZ);
            }
        }
        else if (x2 == x && z2 != z)
        {
            chunkIndexXZ = new IntVector2(x, z2);
            if (!indexList.Contains(chunkIndexXZ))
            {
                indexList.Add(chunkIndexXZ);
            }
        }
        else if (x2 != x && z2 != z)
        {
            chunkIndexXZ = new IntVector2(x2, z);
            if (!indexList.Contains(chunkIndexXZ))
            {
                indexList.Add(chunkIndexXZ);
            }

            chunkIndexXZ = new IntVector2(x, z2);
            if (!indexList.Contains(chunkIndexXZ))
            {
                indexList.Add(chunkIndexXZ);
            }

            chunkIndexXZ = new IntVector2(x2, z2);
            if (!indexList.Contains(chunkIndexXZ))
            {
                indexList.Add(chunkIndexXZ);
            }
        }
        return indexList;
    }

    public static VArtifactData GetVartifactDataFromIsoName(string isoName)
    {
        if (isoNameId.ContainsKey(isoName))
        {
            ulong guid = isoNameId[isoName];
            if (isos.ContainsKey(guid))
            {
                return isos[guid];
            }
        }
        return null;
    }

    public static ulong GetGuidFromIsoName(string isoName)
    {
        if (isoNameId.ContainsKey(isoName))
        {
            return isoNameId[isoName];
        }
        return 0;
    }

    //to judge whether a posXZ is in a town
    public static float IsInTown(IntVector2 posXZ)
    {
        if (VArtifactTownManager.Instance == null)
        {
            return 0;
        }
        int chunkX = posXZ.x >> VoxelTerrainConstants._shift;
        int chunkZ = posXZ.y >> VoxelTerrainConstants._shift;
        IntVector2 chunkPos = new IntVector2(chunkX, chunkZ);

        //if (!VArtifactTownManager.Instance.TownChunk.ContainsKey(chunkPos))
        //{
        //    return null;
        //}
        //IntVector2 townCenter = VArtifactTownManager.Instance.TownChunk[chunkPos];

        if (!VArtifactTownManager.Instance.IsTownChunk(chunkPos))
        {
            return 0;
        }
        float townHeight = VArtifactTownManager.Instance.GetTownCenterByTileAndPos(chunkPos, posXZ);
        return townHeight;
    }

	public static bool IsInTown(IntVector2 posXZ,out IntVector2 townPosCenter){
		townPosCenter = new IntVector2(-9999999,-9999999);
		if (VArtifactTownManager.Instance == null)
		{
			return false;
		}

		if(VArtifactTownManager.Instance==null)
			return false;
		foreach(VArtifactTown vat in VArtifactTownManager.Instance.townPosInfo.Values){
			if(vat.isEmpty)
				continue;
			if(IntVector2.SqrMagnitude(posXZ-vat.PosCenter)<=vat.radius*vat.radius){
				townPosCenter = vat.PosCenter;
				return true;
			}
		}
		return false;
	}

    public static VArtifactTown GetPosTown(Vector3 pos)
    {
        IntVector2 posXZ = new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        int chunkX = posXZ.x >> VoxelTerrainConstants._shift;
        int chunkZ = posXZ.y >> VoxelTerrainConstants._shift;
        IntVector2 chunkPos = new IntVector2(chunkX, chunkZ);

        if (VArtifactTownManager.Instance==null||!VArtifactTownManager.Instance.IsTownChunk(chunkPos))
        {
            return null;
        }
        return VArtifactTownManager.Instance.GetTileTown(chunkPos);
    }
    public static bool CheckTownAvailable(VArtifactTown vaTowndata)
    {
		if (!IsContained(vaTowndata)){
	        for (int i = 0; i < vaTowndata.VAUnits.Count; i++)
	        {
	            VArtifactUnit vau = vaTowndata.VAUnits[i];
				vau.worldPos = new Vector3(vau.PosCenter.x, -1, vau.PosCenter.y);
	        }
			return true;
		}
        return false;
    }


    public static void GetArtifactUnit(VArtifactTown townData, ArtifactUnit[] artifactUnitArray, System.Random myRand)
    {
        int townPosXMin = townData.PosGen.x;
        int townPosZMin = townData.PosGen.y;
        int townPosXMax = townData.PosGen.x;
        int townPosZMax = townData.PosGen.y;

        int unitIndex = 0;
        for (int m = 0; m < artifactUnitArray.Count(); m++)
        {
            IntVector2 posXZ = VArtifactUtil.GetIntVector2FromStr(artifactUnitArray[m].pos);
            int unitID;
            if (artifactUnitArray[m].id.Equals("-1"))
            {
                List<int> idList = VArtifactUtil.townArtifactsData.Keys.ToList();
                unitID = idList[myRand.Next(idList.Count)];
            }
            else
            {
                unitID = VArtifactUtil.RandIntFromStr(artifactUnitArray[m].id, myRand);
            }
            //Debug.Log("unitID:" + unitID);
            Town_artifacts townDataFromDataBase = VArtifactUtil.townArtifactsData[unitID];
            string isoName = townDataFromDataBase.isoName;
            float rot;
            if (artifactUnitArray[m].rot.Equals("-1"))
            {
                //--to do: "type" is not used
                rot = (float)(myRand.NextDouble() * 360);
            }
            else
            {
                rot = VArtifactUtil.RandFloatFromStr(artifactUnitArray[m].rot, myRand);
            }

            //--to do: get the isoData  from id;
            ulong guid;
            //townData.isodataList.Add(posXZ,GetIsoData(isoName,out guid));
            VArtifactUnit vau = new VArtifactUnit();
            VArtifactData isoData = VArtifactUtil.GetIsoData(isoName, out guid);
            if (isoData == null)
            {
                Debug.LogError("unitID:" + unitID + " isoName not found! IsoName: " + isoName);
                continue;
            }
            vau.isoName = isoName;
            vau.unitIndex = unitIndex++;
            vau.isoId = unitID;
            vau.isoGuId = guid;
            vau.vat = townData;
            vau.rot = rot;
            vau.PosCenter = posXZ + townData.PosGen;
            int xIsoSize = isoData.m_HeadInfo.xSize;
            int zIsoSize = isoData.m_HeadInfo.zSize;
            vau.isoStartPos = new IntVector2(posXZ.x - xIsoSize / 2, posXZ.y - zIsoSize / 2) + townData.PosGen;
            vau.isoEndPos = new IntVector2(posXZ.x + xIsoSize / 2, posXZ.y + zIsoSize / 2) + townData.PosGen;
            int xSize = townDataFromDataBase.vaSize.x;
            int zSize = townDataFromDataBase.vaSize.y;
            vau.PosStart = new IntVector2(posXZ.x - xSize / 2, posXZ.y - zSize / 2) + townData.PosGen;
            vau.PosEnd = new IntVector2(posXZ.x + xSize / 2, posXZ.y + zSize / 2) + townData.PosGen;
            vau.level = townData.level;
            vau.type = townData.type;
            vau.buildingIdNum = artifactUnitArray[m].buildingIdNum.ToList();
            vau.npcIdNum = artifactUnitArray[m].npcIdNum.ToList();
            vau.buildingCell = townDataFromDataBase.buildingCell;
            vau.npcPos = townDataFromDataBase.npcPos;
            vau.vaSize = townDataFromDataBase.vaSize;
            vau.towerPos = townDataFromDataBase.towerPos;
            if (vau.PosStart.x < townPosXMin)
                townPosXMin = vau.PosStart.x;
            if (vau.PosStart.y < townPosZMin)
                townPosZMin = vau.PosStart.y;
            if (vau.PosEnd.x > townPosXMax)
                townPosXMax = vau.PosEnd.x;
            if (vau.PosEnd.y > townPosZMax)
                townPosZMax = vau.PosEnd.y;
            townData.VAUnits.Add(vau);
        }
        townData.PosStart = new IntVector2(townPosXMin, townPosZMin);
        townData.PosEnd = new IntVector2(townPosXMax, townPosZMax);
        townData.PosCenter = new IntVector2((townPosXMin + townPosXMax) / 2, (townPosZMin + townPosZMax) / 2);
//        townData.height = Mathf.CeilToInt(townData.VAUnits[0].worldPos.y + townData.VAUnits[0].vaSize.z);
//        townData.TransPos = new Vector3(townData.PosCenter.x, townData.height, townData.PosCenter.y);
        townData.radius = (int)Mathf.Sqrt(Mathf.Pow((townPosXMax - townPosXMin) / 2, 2) + Mathf.Pow((townPosZMax - townPosZMin) / 2, 2));
    }


    public static Vector3 GetPosAfterRotation(VArtifactUnit vau, Vector3 relativePos)
    {
        Quaternion q = new Quaternion();
        q.eulerAngles = new Vector3(0, vau.rot, 0);
        Vector3 ofs = relativePos - vau.worldPos;
        ofs.x += vau.isoStartPos.x;
        ofs.y += vau.worldPos.y;
        ofs.z += vau.isoStartPos.y;
        Vector3 rezultPos = vau.worldPos + q * ofs;
        return rezultPos;
    }

    public static DynamicNativePoint GetDynamicNativePoint(int townId)
    {
        VArtifactTown vat = VArtifactTownManager.Instance.GetTownByID(townId);
        DynamicNativePoint result;

        DynamicNative[] dns = vat.nativeTower.dynamicNatives;
        System.Random randSeed = new System.Random(System.DateTime.Now.Millisecond);
        DynamicNative dn = dns[randSeed.Next(dns.Count())];
        result.id = dn.did;
        result.type = dn.type;
        if (dn.type == 1)
        {
            result.point = GetDynamicNativeSinglePoint(vat, randSeed);
        }
        else
        {
            result.point = GetDynamicNativeGroupPoint(vat, randSeed);
        }

        return result;
    }
    public static Vector3 GetDynamicNativeSinglePoint(VArtifactTown vat, System.Random randSeed)
    {
        VArtifactUnit vau = vat.VAUnits[randSeed.Next(vat.VAUnits.Count)];
        Vector3 posBeforRot = vau.npcPos[randSeed.Next(vau.npcPos.Count)];
        return GetPosAfterRotation(vau, posBeforRot);
    }
    public static Vector3 GetDynamicNativeGroupPoint(VArtifactTown vat, System.Random randSeed)
    {
        return GetDynamicNativeSinglePoint(vat, randSeed);
    }

	public static DynamicNative[] GetAllDynamicNativePoint(int townId,out List<Vector3> posList){
		DynamicNative[] result =null;
		posList = new List<Vector3> ();
		if(VArtifactTownManager.Instance!=null){
			VArtifactTown vat = VArtifactTownManager.Instance.GetTownByID(townId);
			if(vat!=null){
				if(vat.nativeTower!=null){
					result = vat.nativeTower.dynamicNatives;
					if(vat.VAUnits!=null&&vat.VAUnits.Count>0){
						VArtifactUnit vau = vat.VAUnits[0];
						List<Vector3> posBeforRot= vau.npcPos;
						foreach(Vector3 p in posBeforRot){
							posList.Add(GetPosAfterRotation(vau, p));
						}
					}else{
						Debug.LogError("GetAllDynamicNativePoint: "+"vat.VAUnits==null||vat.VAUnits.Count=0");
					}
				}else{
					Debug.LogError("GetAllDynamicNativePoint: "+"vat.nativeTower==null");
				}
			}else{
				Debug.LogError("GetAllDynamicNativePoint: "+"vat==null");
			}
		}else{
			Debug.LogError("GetAllDynamicNativePoint: "+"ArtifactTownManager.Instance==null");
		}
		return result;
	}


    public static void LoadData()
    {
		LoadTownArtifact();
		LoadTownNameData();
		LoadAllyName();
    }

	public static void LoadTownArtifact(){
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("Town_artifacts");
		
		while (reader.Read())
		{
			Town_artifacts townArtifact = new Town_artifacts();
			
			townArtifact.ID = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Id")));
			townArtifact.isoName = reader.GetString(reader.GetOrdinal("Name"));
			townArtifact.vaSize = GetIntVector3FromStr(reader.GetString(reader.GetOrdinal("Size")));
			string b_positions = reader.GetString(reader.GetOrdinal("B_position"));
			string npc_borns = reader.GetString(reader.GetOrdinal("NPC_born"));
			string tower_pos = reader.GetString(reader.GetOrdinal("Tower"));
			
			townArtifact.buildingCell = new List<BuildingCell>();
			string[] buildingCellStr = b_positions.Split('_');
			for (int i = 0; i < buildingCellStr.Count(); i++)
			{
				BuildingCell bc = new BuildingCell();
				string[] posRotStr = buildingCellStr[i].Split(';');
				bc.cellPos = GetVector3FromStr(posRotStr[0]);
				bc.cellRot = float.Parse(posRotStr[1]);
				townArtifact.buildingCell.Add(bc);
			}
			
			townArtifact.npcPos = new List<Vector3>();
			string[] npcPosStr = npc_borns.Split('_');
			for (int i = 0; i < npcPosStr.Count(); i++)
			{
				Vector3 npcPos = GetVector3FromStr(npcPosStr[i]);
				townArtifact.npcPos.Add(npcPos);
			}
			
			townArtifact.towerPos = GetVector3FromStr(tower_pos);
			townArtifactsData.Add(townArtifact.ID, townArtifact);
		}
	}

	public static void LoadTownNameData(){
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("TownName");
		
		while (reader.Read())
		{
			int id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Id")));
			int nameId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TranslationId")));
			townNameData.Add(id,nameId);
		}
	}
	public static int GetTownNameId(int id){
		if(townNameData.ContainsKey(id))
			return townNameData[id];
		return -1;
	}

	public static void LoadAllyName(){
		SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("AdvCampName");
		
		while (reader.Read())
		{
			AllyName an = new AllyName ();
			an.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("ID")));
			an.raceId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("Race")));
			an.nameId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("TranslationID")));
			allyNameData.Add(an.id,an);
		}
	}


    public static bool IsContained(VArtifactTown townInfo)
    {
        for (int vaui = 0; vaui < townInfo.VAUnits.Count; vaui++)
        {
            List<IntVector2> chunkIndexList = LinkedChunkIndex(townInfo.VAUnits[vaui]);
            for (int i = 0; i < chunkIndexList.Count; i++)
            {
                IntVector2 chunkIndex = chunkIndexList[i];
                if (VArtifactTownManager.Instance.TileContainsTown(chunkIndex))
                {
                    return true;
                }
            }
        }
        return false;
    }


    public static Vector3 GetStartPos()
    {
        if(PeGameMgr.IsSingleAdventure)
            return VArtifactTownManager.Instance.playerStartPos;
        IntVector2 posXZ = GetSpawnPos();
        Vector3 pos = new Vector3(posXZ.x, VFDataRTGen.GetPosTop(posXZ), posXZ.y);
        return pos;
    }

    public static IntVector2 GetSpawnPos()
    {
        System.Random seed = new System.Random(System.DateTime.Now.Millisecond);
        if (PeGameMgr.IsMultiAdventure)
        {
            if (PeGameMgr.IsMultiCoop)
			{
				return new IntVector2(Mathf.RoundToInt(VArtifactTownManager.Instance.playerStartPos.x),Mathf.RoundToInt(VArtifactTownManager.Instance.playerStartPos.z));
//					List<VArtifactTown> townList = VATownGenerator.Instance.GetAllyTowns(TownGenData.PlayerAlly);
//					townList= townList.FindAll (it=>it.townId==0);
//                    if (townList.Count==0)
//                    {
//                        LogManager.Error("No town! ");
//						return VATownGenerator.Instance.GetInitPos(seed);
//                    }
//
//                    int count = seed.Next(townList.Count);
//                    return townList[count].PosCenter;

            }
            else if (PeGameMgr.IsMultiVS)
            {
					List<VArtifactTown> townList = VATownGenerator.Instance.GetAllyTowns(TownGenData.PlayerAlly);
					townList= townList.FindAll (it=>it.level<=1&&it.isMainTown);
					if (townList.Count==0)
					{
						LogManager.Error("No town! ");
						return VATownGenerator.Instance.GetInitPos(seed);
					}
                    int townCount = townList.Count;
                    List<int> indexGroup = new List<int>();
                    for (int i = 0; i < townCount; i++)
                    {
                        indexGroup.Add(i);
                    }
                    Shuffle(indexGroup, new System.Random(RandomMapConfig.RandSeed));
                    int townIndex = indexGroup[BaseNetwork.MainPlayer.TeamId % townCount];
                    VArtifactTown townInfo = townList[townIndex];
                    IntVector2 townCenter = townInfo.PosCenter;
                    IntVector2 spawnPos = new IntVector2(townCenter.x + seed.Next(-spawnRadius, spawnRadius), townCenter.y + seed.Next(-spawnRadius, spawnRadius));
                    return spawnPos;

            }
            else //GameConfig.IsMultiSurvive
            {
					List<VArtifactTown> townList = VATownGenerator.Instance.GetAllyTowns(TownGenData.PlayerAlly);
					townList= townList.FindAll (it=>it.level<=1&&it.isMainTown);
					if (townList.Count==0)
					{
						LogManager.Error("No town! ");
						return VATownGenerator.Instance.GetInitPos(seed);
					}
                    int count = seed.Next(townList.Count);
                    VArtifactTown townInfo = townList[count];
                    IntVector2 townCenter = townInfo.PosCenter;
                    IntVector2 spawnPos = new IntVector2(townCenter.x + seed.Next(-spawnRadius, spawnRadius), townCenter.y + seed.Next(-spawnRadius, spawnRadius));
                    return spawnPos;
            }
        }
        else
        {
			IntVector2 spawnPos = VATownGenerator.Instance.GetInitPos();

            return spawnPos;
        }
    }

    public static void SetNpcStandRot(PeEntity npc,float rot,bool isStand)
    {
        if (npc == null)
            return;
        NpcCmpt npcCmpt = npc.GetCmpt<NpcCmpt>();
        if (npcCmpt != null)
        {
            npcCmpt.Req_Rotation(Quaternion.Euler(0,rot,0));
			npcCmpt.StandRotate = rot;
//            if(isStand)
//                npcCmpt.Req_SetIdle("Idle");
        }
    }
    public static void GetPosRotFromPointRot(ref Vector3 pos, ref float rot, Vector3 refPos, float refRot)
    {
        Quaternion q = new Quaternion();
        q.eulerAngles = new Vector3(0, refRot, 0);
        pos = refPos + q * pos;
        rot = refRot + rot;
    }

    //public static bool IsInBuildingArea(Vector2 pos, List<VArtifactUnit> vauList)
    //{
    //    bool flag = false;
    //    foreach(VArtifactUnit vau in vauList)
    //    {
    //        foreach (BuildingID bid in vau.buildingPosID.Values)
    //        {
    //            Quaternion q = new Quaternion();
    //            q.eulerAngles = new Vector3(0, -vab.rotation, 0);
    //            Vector3 pointPos = new Vector3(pos.x, vab.root.y, pos.y);
    //            Vector3 relativePos = q * (pointPos - vab.root);
    //            if (relativePos.x > -vab.size.x / 2 && relativePos.x < vab.size.x / 2
    //                && relativePos.z > -vab.size.y && relativePos.z < vab.size.y / 2)
    //            {
    //                flag = true;
    //                break;
    //            }
    //        }
    //        if (flag)
    //            break;
    //    }
    //    return flag;
    //}

	#region Town
	public static void ShowAllTowns(){
		List<VArtifactTown> vatList = VArtifactTownManager.Instance.townPosInfo.Values.ToList();
		foreach(VArtifactTown vat in vatList){
			if(vat.Type==VArtifactType.NpcTown)
				RandomMapIconMgr.AddTownIcon(vat);
			else
				RandomMapIconMgr.AddNativeIcon(vat);
		}
	}
	public static void RemoveAllTowns(){
		RandomMapIconMgr.ClearAll();
	}
	public static bool HasTown(){
		if(VArtifactTownManager.Instance==null)
			return false;
		return VArtifactTownManager.Instance.townPosInfo.Values.Count>0;
	}

	public static float GetNearestTownDistance(int x,int z,out VArtifactTown vaTown){
		float distancePow = float.MaxValue;
		vaTown= null;
		if(VArtifactTownManager.Instance==null)
			return distancePow;
		foreach(VArtifactTown vat in VArtifactTownManager.Instance.townPosInfo.Values){
			if(vat.isEmpty)
				continue;
			float d = (vat.PosCenter.x-x)*(vat.PosCenter.x-x)+(vat.PosCenter.y-z)*(vat.PosCenter.y-z);
			if(d<distancePow){
				distancePow = d;
				vaTown = vat;
			}
		}
		return Mathf.Sqrt(distancePow);
	}

	public static bool IsInTownBallArea(Vector3 pos){
		if(VArtifactTownManager.Instance==null)
			return false;
		foreach(VArtifactTown vat in VArtifactTownManager.Instance.townPosInfo.Values){
			if(vat.isEmpty)
				continue;
			if((pos-vat.TransPos).sqrMagnitude<=vat.radius*vat.radius)
				return true;
		}
		return false;
	}

	//-1:none,0:puja,1:paja
	public static int IsInNativeCampArea(Vector3 pos){
		if(VArtifactTownManager.Instance==null){
			return -1;
		}
		IntVector2 posXZ = new IntVector2 (Mathf.RoundToInt(pos.x),Mathf.RoundToInt(pos.z));
		IntVector2 tileIndex = new IntVector2 (posXZ.x>>VoxelTerrainConstants._shift,posXZ.y>>VoxelTerrainConstants._shift);

		VArtifactTown vat =VArtifactTownManager.Instance.GetTileTown(tileIndex);
		if(vat==null)
			return -1;
		else if(vat.type==VArtifactType.NativeCamp){
			if(vat.PosCenter.Distance(posXZ)<vat.radius&&vat.InYArea(pos.y)){
				if(vat.nativeType==NativeType.Puja){
					return 0;
				}
				else{
					
					return 1;
				}
			}else
				return -1;
		}else
			return -1;
	}

	public static void ChangeTownAlliance(int townId){
		VArtifactTown vat = VArtifactTownManager.Instance.GetTownByID(townId);
		if(vat!=null)
			VATownGenerator.Instance.ChangeAlliance(vat);
		//--to do: refresh
	}

	public static void RestoreTownAlliance(int townId){
		VArtifactTown vat = VArtifactTownManager.Instance.GetTownByID(townId);
		if(vat!=null)
			VATownGenerator.Instance.RestoreAlliance(vat);
		//--to do: refresh
	}

	public static bool GetTownPos(int townId,out Vector3 pos){
		if(VArtifactTownManager.Instance==null){
			pos=Vector3.zero;
			return false;
		}
		else{
			VArtifactTown vat = VArtifactTownManager.Instance.GetTownByID(townId);
			if(vat==null)
			{
				pos = Vector3.zero;
				return false;
			}else{
				pos = vat.TransPos;
				return true;
			}
		}
	}

	public static bool GetTownName(int townId,out string name){
		if(VArtifactTownManager.Instance==null){
			name="";
			return false;
		}
		else{
			VArtifactTown vat = VArtifactTownManager.Instance.GetTownByID(townId);
			if(vat==null)
			{
				name = "";
				return false;
			}else{
				name = PELocalization.GetString(vat.townNameId);
				return true;
			}
		}
	}

	public static int GetFirstEnemyNpcColor(){
		if(VATownGenerator.Instance==null)
			return -1;
		return VATownGenerator.Instance.GetFirstEnemyNpcAllyColor();
	}
	public static int GetFirstEnemyNpcPlayerId(){
		if(VATownGenerator.Instance==null)
			return -1;
		return VATownGenerator.Instance.GetFirstEnemyNpcAllyPlayerId();
	}
	public static void RegistTownDestryedEvent(VArtifactTownManager.TownDestroyed eventListener){
		if(VArtifactTownManager.Instance!=null){
			VArtifactTownManager.Instance.TownDestroyedEvent-=eventListener;
			VArtifactTownManager.Instance.TownDestroyedEvent+=eventListener;
		}

	}
	public static void UnRegistTownDestryedEvent(VArtifactTownManager.TownDestroyed eventListener){
		if(VArtifactTownManager.Instance!=null){
			VArtifactTownManager.Instance.TownDestroyedEvent-=eventListener;
		}
	}
	#endregion

    #region tool
    public static void Shuffle(List<int> id, System.Random myRand)
    {
        int size = id.Count;
        List<int> temp = new List<int>();
        for (int i = 0; i < size; i++)
        {
            temp.Add(id[i]);
        }

        int index = 0;
        while (temp.Count > 0)
        {
            int i = myRand.Next(temp.Count);
            id[index] = temp[i];
            index++;
            temp.RemoveAt(i);
        }
    }

    public static List<int> RandomChoose(int num, int minValue, int maxValue, System.Random randomSeed)
    {
        List<int> pool = new List<int>();
        for (int i = minValue; i < maxValue + 1; i++)
        {
            pool.Add(i);
        }

        List<int> rezult = new List<int>();
        for (int i = 0; i < num; i++)
        {
            int index = randomSeed.Next(maxValue - minValue + 1 - i);
            rezult.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return rezult;
    }
    public static Vector3 GetVector3FromStr(string value)
    {
        if (value == "0")
        {
            return Vector3.zero;
        }
        string[] values = value.Split(',');
        float x = float.Parse(values[0]);
        float y = float.Parse(values[1]);
        float z = float.Parse(values[2]);
        return new Vector3(x, y, z);
    }
    public static IntVector2 GetIntVector2FromStr(string value)
    {
        string[] values = value.Split(',');
        int x = Convert.ToInt32(values[0]);
        int z = Convert.ToInt32(values[1]);
        return new IntVector2(x, z);
    }

    public static IntVector3 GetIntVector3FromStr(string value)
    {
        string[] values = value.Split(',');
        int x = Convert.ToInt32(values[0]);
        int y = Convert.ToInt32(values[1]);
        int z = Convert.ToInt32(values[2]);
        return new IntVector3(x, y, z);
    }
    public static int RandIntFromStr(string value, System.Random rand)
    {
        string[] values = value.Split(',');
        int result = rand.Next(values.Count());
        result = Convert.ToInt32(values[result]);
        return result;
    }
    public static float RandFloatFromStr(string value, System.Random rand)
    {
        string[] values = value.Split(',');
        int resultIndex = rand.Next(values.Count());
        float result = float.Parse(values[resultIndex]);
        return result;
    }

    public static IntVector2 GenCampByZone(IntVector2 center, int zoneNo, int distanceMin, int distanceMax, System.Random randomSeed)
    {
        IntVector2 result = new IntVector2(center.x, center.y);
        switch (zoneNo)
        {
            case 0:
                result.x += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
                result.y += randomSeed.Next(distanceMin, distanceMax);
                break;
            case 1:
                result.x += randomSeed.Next(distanceMin, distanceMax);
                result.y += randomSeed.Next(distanceMin, distanceMax);
                break;
            case 2:
                result.x += randomSeed.Next(distanceMin, distanceMax);
                result.y += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
                break;
            case 3:
                result.x += randomSeed.Next(distanceMin, distanceMax);
                result.y -= randomSeed.Next(distanceMin, distanceMax);
                break;
            case 4:
                result.x += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
                result.y -= randomSeed.Next(distanceMin, distanceMax);
                break;
            case 5:
                result.x -= randomSeed.Next(distanceMin, distanceMax);
                result.y -= randomSeed.Next(distanceMin, distanceMax);
                break;
            case 6:
                result.x -= randomSeed.Next(distanceMin, distanceMax);
                result.y += randomSeed.Next(-distanceMin / 2, distanceMin / 2);
                break;
            case 7:
                result.x -= randomSeed.Next(distanceMin, distanceMax);
                result.y += randomSeed.Next(distanceMin, distanceMax);
                break;
            default:
                result.x += randomSeed.Next(distanceMin, distanceMax);
                result.y += randomSeed.Next(distanceMin, distanceMax);
                break;
        }
        return result;
    }

	public static IntVector2 GetRandomPointFromPoint(IntVector2 centerPoint,float MaxRadius, System.Random rand){
		IntVector2 resultPoint = null;
		double dist = rand.NextDouble()*MaxRadius*4/5+MaxRadius/5;
		double angle = 2*Mathf.PI*rand.NextDouble();
		int xDist = Mathf.RoundToInt((float)(dist*Math.Cos(angle)));
		int zDist = Mathf.RoundToInt((float)(dist*Math.Sin(angle)));
		resultPoint = new IntVector2(centerPoint.x+xDist,centerPoint.y+zDist);
		return resultPoint;
	}
    #endregion
}