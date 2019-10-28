//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;
//using NativeCampXML;
//using RandomTownXML;

//namespace TownData
//{
//    public enum TownType
//    {
//        NpcTown,
//        NativeCamp
//    }

//    public class TownInfo
//    {
//        int id;
//        public int Id
//        {
//            get { return id; }
//            set { id = value; }
//        }

//        bool isRandomed = false;
//        public bool IsRandomed
//        {
//            get { return isRandomed; }
//            set { isRandomed = value; }
//        }

//        bool isExplored = false;
//        public bool IsExplored
//        {
//            get { return isExplored; }
//            set { isExplored = value; }
//        }

//        bool isAdded = false;
//        public bool IsAdded
//        {
//            get { return isAdded; }
//            set { isAdded = value; }
//        }

//        int height = -1;
//        public int Height
//        {
//            get { return height; }
//            set { height = value; }
//        }


//        IntVector2 posStart;
//        public IntVector2 PosStart
//        {
//            get { return posStart; }
//            set { posStart = value; }
//        }

//        IntVector2 posEnd;
//        public IntVector2 PosEnd
//        {
//            get { return posEnd; }
//            set { posEnd = value; }
//        }

//        IntVector2 posCenter;
//        public IntVector2 PosCenter
//        {
//            get { return posCenter; }
//            set { posCenter = value; }
//        }

//        int cellNumX;
//        public int CellNumX
//        {
//            get { return cellNumX; }
//            set { cellNumX = value; }
//        }

//        int cellNumZ;
//        public int CellNumZ
//        {
//            get { return cellNumZ; }
//            set { cellNumZ = value; }
//        }

//        int cellSizeX;
//        public int CellSizeX
//        {
//            get { return cellSizeX; }
//            set { cellSizeX = value; }
//        }

//        int cellSizeZ;
//        public int CellSizeZ
//        {
//            get { return cellSizeZ; }
//            set { cellSizeZ = value; }
//        }

//        int sizeX;
//        public int SizeX
//        {
//            get { return sizeX; }
//            set { sizeX = value; }
//        }

//        int sizeZ;
//        public int SizeZ
//        {
//            get { return sizeZ; }
//            set { sizeZ = value; }
//        }

//        int tid;//template id;
//        public int Tid
//        {
//            get { return tid; }
//            set { tid = value; }
//        }

//        int cid;
//        public int Cid
//        {
//            get { return cid; }
//            set { cid = value; }
//        }

//        TownType townType;
//        public TownType Type
//        {
//            get { return townType; }
//            set { townType = value; }
//        }

//        public int ladderSelectNum;
//        public List<Ladder> ladderInfo;
//        public List<Cell> cellInfo;
//        public List<BuildingNum> buildingNum;
//        public List<NpcIdNum> npcIdNum;
//        public List<NativeIdNum> nativeIdNum;
//        public NativeTower nativeTower;
//        public bool nativeBuildingRendered = false;

//        List<BuildingID> buildingIdList = new List<BuildingID>();
//        public List<BuildingID> BuildingIdList
//        {
//            get { return buildingIdList; }
//            set { buildingIdList = value; }
//        }

//        public Dictionary<IntVector2, int> townPosId = new Dictionary<IntVector2, int>();

//        List<IntVector2> townPosList = new List<IntVector2>();
//        public List<IntVector2> TownPosList
//        {
//            get { return townPosList; }
//            set { townPosList = value; }
//        }




//        List<IntVector2> buildingCellList = new List<IntVector2>();
//        public List<IntVector2> BuildingCellList
//        {
//            get { return buildingCellList; }
//            set { buildingCellList = value; }
//        }

//        List<IntVector2> streetCellList = new List<IntVector2>();
//        public List<IntVector2> StreetCellList
//        {
//            get { return streetCellList; }
//            set { streetCellList = value; }
//        }

//        //List<AiNpcObject> townNpcList = new List<AiNpcObject>();

//        public int level;
//        public float radius;

//        public TownInfo() { }

//        public TownInfo(TownInfo ti)
//        {
//            this.posStart = ti.PosStart;
//            this.cellNumX = ti.cellNumX;
//            this.cellNumZ = ti.cellNumZ;
//            this.cellSizeX = ti.cellSizeX;
//            this.cellSizeZ = ti.cellSizeZ;
//            this.sizeX = ti.sizeX;
//            this.sizeZ = ti.sizeZ;
//            this.tid = ti.tid;

//            posEnd = new IntVector2(posStart.x + sizeX, posStart.y + sizeZ);
//            posCenter = new IntVector2(posStart.x + sizeX / 2, posStart.y + sizeZ / 2);
//            radius = TrianglesSide(sizeX / 2, sizeZ / 2);
//        }

//        public TownInfo(TownType type, IntVector2 pos, int cellNumX, int cellNumZ, int cellSizeX, int cellSizeZ, int id)
//        {
//            townType = type;
//            this.posStart = pos;
//            this.cellNumX = cellNumX;
//            this.cellNumZ = cellNumZ;
//            this.cellSizeX = cellSizeX;
//            this.cellSizeZ = cellSizeZ;
//            this.sizeX = cellNumX * cellSizeX;
//            this.sizeZ = cellNumZ * cellSizeZ;

//            posEnd = new IntVector2(posStart.x + sizeX, posStart.y + sizeZ);
//            posCenter = new IntVector2(posStart.x + sizeX / 2, posStart.y + sizeZ / 2);
//            radius = TrianglesSide(sizeX / 2, sizeZ / 2);

//            if (type == TownType.NpcTown)
//            {
//                this.tid = id;
//            }
//            else
//            {
//                this.cid = id;
//            }
//        }

//        public TownInfo(Town t, IntVector2 pos)
//        {
//            townType = TownType.NpcTown;
//            tid = t.tid;
//            level = t.level;
//            this.posStart = pos;
//            this.cellNumX = t.cellNumX;
//            this.cellNumZ = t.cellNumZ;
//            this.cellSizeX = t.cellSizeX;
//            this.cellSizeZ = t.cellSizeZ;
//            this.sizeX = cellNumX * cellSizeX;
//            this.sizeZ = cellNumZ * cellSizeZ;

//            posEnd = new IntVector2(posStart.x + sizeX, posStart.y + sizeZ);
//            posCenter = new IntVector2(posStart.x + sizeX / 2, posStart.y + sizeZ / 2);
//            radius = TrianglesSide(sizeX / 2, sizeZ / 2);

//            cellInfo = t.cell.ToList();
//            buildingNum = t.buildingNum.ToList();
//            npcIdNum = t.npcIdNum.ToList();

//            if (t.ladderArray != null && t.ladderArray.ladder != null)
//            {
//                ladderInfo = t.ladderArray.ladder.ToList();
//                ladderSelectNum = t.ladderArray.selectNum;
//                if (ladderSelectNum == 0)
//                {
//                    ladderSelectNum = ladderInfo.Count;
//                }
//            }
//        }

//        public TownInfo(NativeCamp nc, IntVector2 pos)
//        {
//            townType = TownType.NativeCamp;
//            cid = nc.cid;
//            level = nc.level;
//            this.posStart = pos;
//            this.cellNumX = nc.cellNumX;
//            this.cellNumZ = nc.cellNumZ;
//            this.cellSizeX = nc.cellSizeX;
//            this.cellSizeZ = nc.cellSizeZ;
//            this.sizeX = cellNumX * cellSizeX;
//            this.sizeZ = cellNumZ * cellSizeZ;

//            posEnd = new IntVector2(posStart.x + sizeX, posStart.y + sizeZ);
//            posCenter = new IntVector2(posStart.x + sizeX / 2, posStart.y + sizeZ / 2);
//            radius = TrianglesSide(sizeX / 2, sizeZ / 2);

//            nativeTower = nc.nativeTower;
//            cellInfo = nc.cell.ToList();
//            buildingNum = nc.buildingNum.ToList();
//            nativeIdNum = nc.nativeIdNum.ToList();

//            if (nc.ladderArray != null && nc.ladderArray.ladder != null)
//            {
//                ladderInfo = nc.ladderArray.ladder.ToList();
//                ladderSelectNum = nc.ladderArray.selectNum;
//                if (ladderSelectNum == 0)
//                {
//                    ladderSelectNum = ladderInfo.Count;
//                }
//            }
//        }

//        //public TownInfo(IntVector2 pos, int cellNumX, int cellNumZ, int cellSizeX, int cellSizeZ, int tid,uint[] buildingIdList,IntVector2[] townNpcPosList)
//        //{
//        //    this.posStart = pos;
//        //    this.cellNumX = cellNumX;
//        //    this.cellNumZ = cellNumZ;
//        //    this.cellSizeX = cellSizeX;
//        //    this.cellSizeZ = cellSizeZ;
//        //    this.sizeX = cellNumX * cellSizeX;
//        //    this.sizeZ = cellNumZ * cellSizeZ;
//        //    posEnd = new IntVector2(pos.x + sizeX, pos.y + sizeZ);
//        //    posCenter = new IntVector2(pos.x + sizeX / 2, pos.y + sizeZ / 2);
//        //    this.tid = tid;
//        //    radius = TrianglesSide(sizeX / 2, sizeZ / 2);
//        //    foreach (uint bid in buildingIdList)
//        //    {
//        //        this.BuildingIdList.Add(bid);
//        //    }
//        //    foreach (IntVector2 townNpcPos in townNpcPosList)
//        //    {
//        //        this.TownPosList.Add(townNpcPos);
//        //    }
//        //}

//        public void AddStreetCell()
//        {
//            for (int i = 0; i < cellNumX; i++)
//            {
//                for (int j = 0; j < cellNumZ; j++)
//                {
//                    IntVector2 cellIndex = new IntVector2(i, j);
//                    if (!buildingCellList.Contains(cellIndex))
//                    {
//                        streetCellList.Add(cellIndex);
//                    }
//                }
//            }
//        }

//        //public void AddTownNpc(AiNpcObject npc)
//        //{
//        //    if (!townNpcList.Contains(npc))
//        //        townNpcList.Add(npc);
//        //}

//        //public void RemoveTownNpc(AiNpcObject npc)
//        //{
//        //    townNpcList.Remove(npc);
//        //}

//        //public IEnumerable<AiNpcObject> GetAttackNpc()
//        //{
//        //    return townNpcList.Where(iter => iter.IsController && !iter.IsFollower);
//        //}

//        public void AddBuildingCell(int x, int z)
//        {
//            buildingCellList.Add(new IntVector2(x, z));
//        }



//        // 0,1, 2,3
//        //-x,x,-z,z
//        public int IsOnBoundary(IntVector2 posXZ)
//        {
//            //-x
//            if (posXZ.x == posStart.x && posXZ.y >= posStart.y && posXZ.y <= posEnd.y)
//            {
//                return 0;
//            }

//            //x
//            if (posXZ.x == posEnd.x && posXZ.y >= posStart.y && posXZ.y <= posEnd.y)
//            {
//                return 1;
//            }

//            //-z
//            if (posXZ.y == posStart.y && posXZ.x >= posStart.x && posXZ.y <= posEnd.x)
//            {
//                return 2;
//            }


//            //z
//            if (posXZ.y == posEnd.y && posXZ.x >= posStart.x && posXZ.y <= posEnd.x)
//            {
//                return 3;
//            }


//            return -1;
//        }

//        //internal static object DeserializeInfo(uLink.BitStream stream, params object[] codecOptions)
//        //{
//        //    int id = stream.Read<int>();
//        //    IntVector2 pos = stream.Read<IntVector2>();
//        //    int cellNumX = stream.Read<int>();
//        //    int cellNumZ = stream.Read<int>();
//        //    int cellSizeX = stream.Read<int>();
//        //    int cellSizeZ = stream.Read<int>();
//        //    int tid = stream.Read<int>();
//        //    uint[] buildingIdList = stream.Read<uint[]>();
//        //    IntVector2[] townNpcInfo = stream.Read<IntVector2[]>();
//        //    TownInfo townInfo = new TownInfo(pos, cellNumX, cellNumZ, cellSizeX, cellSizeZ, tid,buildingIdList, townNpcInfo);
//        //    townInfo.Id = id;
//        //    return townInfo;
//        //}


//        //internal static void SerializeInfo(uLink.BitStream stream, object value, params object[] codecOptions)
//        //{
//        //    TownInfo townInfo = value as TownInfo;
//        //    stream.Write<int>(townInfo.id);
//        //    stream.Write<IntVector2>(townInfo.posStart);
//        //    stream.Write<int>(townInfo.cellNumX);
//        //    stream.Write<int>(townInfo.cellNumZ);
//        //    stream.Write<int>(townInfo.cellSizeX);
//        //    stream.Write<int>(townInfo.cellSizeZ);
//        //    stream.Write<int>(townInfo.tid);
//        //    stream.Write<uint[]>(townInfo.buildingIdList.ToArray());
//        //    stream.Write<IntVector2[]>(townInfo.TownPosList.ToArray());
//        //}

//        public float TrianglesSide(float x, float y)
//        {
//            return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
//        }

//    }
//}
