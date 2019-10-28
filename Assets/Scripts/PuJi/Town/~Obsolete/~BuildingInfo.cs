//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using UnityEngine;


////data for randomMap
//namespace TownData {
//    public enum BuildingType
//    {
//        Block,
//        Prefeb
//    }

//    public class BuildingInfo
//    {
        
//        //the param used in the scene
//        public BuildingID buildingId;
//        public Vector3 pos;
//        public int rotation=0;
//        public int id;
//        public Vector3 size;
//        public IntVector2 center;
//        public bool isRendered=false;
//        public int cellSizeX;
//        public int cellSizeZ;
//        public Vector3 endPos;
//        public BuildingType buildingType;
//        public Vector3 frontDoorPos;

//        public List<CreatItemInfo> itemInfoList;
//        public List<Vector3> npcPos;
//        public Dictionary<int,int> npcIdNum;
//        public List<Vector3> nativePos;
//        public Dictionary<int, int> nativeIdNum;

//        //the param to compute the block
//        public Vector3 root;//the point which is the axis of rotation
//        public Vector3 rootSize;

//        public VATownInfo townInfo;
//        public int pathID;
//        public BuildingInfo(){}

//        public BuildingInfo(IntVector2 pos, int id, int rot,int cellSizeX,int cellSizeZ)
//        {
//            this.pos.x = pos.x;
//            this.pos.y = -1;
//            this.pos.z = pos.y;
//            this.id = id;
//            this.rotation = rot;
//            this.cellSizeX = cellSizeX;
//            this.cellSizeZ = cellSizeZ;
//            if (rot == 0)
//            {
//                root = this.pos;

//            }else if (rot==1)
//            {
//                root =new Vector3(this.pos.x,this.pos.y,this.pos.z+cellSizeZ);
                
//            }else if (rot==2)
//            {
//                root = new Vector3(this.pos.x+cellSizeX,this.pos.y,this.pos.z+cellSizeZ);
//            }else if (rot==3)
//            {
//                root = new Vector3(this.pos.x+cellSizeX, this.pos.y, this.pos.z);
//            }

//            center = new IntVector2();
//            center.x = Mathf.FloorToInt(this.pos.x + cellSizeX / 2);
//            center.y = Mathf.FloorToInt(this.pos.z + cellSizeZ / 2);
//        }

//        public BuildingInfo(IntVector2 pos, int id, int rot, int cellSizeX, int cellSizeZ, BuildingID buildingId,BuildingType type)
//        {
//            this.pos.x = pos.x;
//            this.pos.y = -1;
//            this.pos.z = pos.y;
//            this.id = id;
//            this.rotation = rot;
//            this.cellSizeX = cellSizeX;
//            this.cellSizeZ = cellSizeZ;
//            buildingType = type;

//            center = new IntVector2();
//            center.x = Mathf.FloorToInt(this.pos.x + cellSizeX / 2);
//            center.y = Mathf.FloorToInt(this.pos.z + cellSizeZ / 2);

//            if (type == BuildingType.Block)
//            {
//                if (rot == 0)
//                {
//                    root = this.pos;
//                }
//                else if (rot == 1)
//                {
//                    root = new Vector3(this.pos.x, this.pos.y, this.pos.z + cellSizeZ);
//                }
//                else if (rot == 2)
//                {
//                    root = new Vector3(this.pos.x + cellSizeX, this.pos.y, this.pos.z + cellSizeZ);
//                }
//                else if (rot == 3)
//                {
//                    root = new Vector3(this.pos.x + cellSizeX, this.pos.y, this.pos.z);
//                }
//            }
//            else {
//                root = new Vector3(center.x,-1,center.y);
//                rotation = rot * 90+180;

//            }

//            switch(rot)
//            {
//                case 0:
//                    frontDoorPos = new Vector3(pos.x + cellSizeX*2 / 5.0f, -1, pos.y-1);
//                    break;
//                case 1:
//                    frontDoorPos = new Vector3(pos.x-1, -1, pos.y + cellSizeZ*2 / 5.0f);
//                    break;
//                case 2:
//                    frontDoorPos = new Vector3(pos.x + cellSizeX*2 / 5.0f, -1, pos.y + cellSizeZ+1);
//                    break;
//                case 3:
//                    frontDoorPos = new Vector3(pos.x + cellSizeX+1, -1, pos.y + cellSizeZ*2 / 5.0f);
//                    break;
//                default: Debug.LogError("rot error! " +"id="+id+" buildingId="+buildingId+" rot="+rot);
//                    break;
//            }


//            this.buildingId = buildingId;
//        }

//        public void setEndPos()
//        {
//            endPos = new Vector3(pos.x+cellSizeX,pos.y,pos.z+cellSizeZ);
//        }

//        public bool isBulidingArea(IntVector2 posXZ)
//        {
//            bool flag = false;
//            if (posXZ.x >= pos.x && posXZ.y >= pos.z && posXZ.x <= endPos.x && posXZ.y<=endPos.z)
//            {
//                flag = true;
//            }
//            return flag;
//        }

//        public void setHeight(float top)
//        {
//            pos.y = top;
//            endPos.y = top;
//            root.y = top;
//        }
//        public float getHeight()
//        {
//            return pos.y;
//        }

//        //internal static object DeserializeInfo(uLink.BitStream stream, params object[] codecOptions)
//        //{
//        //    IntVector2 pos = stream.Read<IntVector2>();
//        //    int id = stream.Read<int>();
//        //    int rot = stream.Read<int>();
//        //    int cellSizeX = stream.Read<int>();
//        //    int cellSizeZ = stream.Read<int>();
//        //    BuildingID no = stream.Read<BuildingID>();

//        //    BuildingInfo buildingInfo = new BuildingInfo(pos, id, rot, cellSizeX, cellSizeZ, no);

//        //    return buildingInfo;
//        //}

//        //internal static void SerializeInfo(uLink.BitStream stream, object value, params object[] codecOptions)
//        //{
//        //    BuildingInfo buildingInfo = value as BuildingInfo;
//        //    stream.Write<IntVector2>(new IntVector2(Mathf.RoundToInt(buildingInfo.pos.x), Mathf.RoundToInt(buildingInfo.pos.z)));
//        //    stream.Write<int>(buildingInfo.id);
//        //    stream.Write<int>(buildingInfo.rotation);
//        //    stream.Write<int>(buildingInfo.cellSizeX);
//        //    stream.Write<int>(buildingInfo.cellSizeZ);
//        //    //stream.Write<uint>(buildingInfo.buildingNo);
//        //}


//        public void OnSpawned(GameObject obj)
//        {
//            //native_barrack nb = obj.GetComponent<native_barrack>();
//            //nb.SetTownId(townInfo.Id);
//        }
//    }

    
//}
