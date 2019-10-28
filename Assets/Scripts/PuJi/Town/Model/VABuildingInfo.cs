using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public enum VABuildingType
{
    Block,
    Prefeb
}
public class VABuildingInfo
{

    public VArtifactUnit vau;
    //the param used in the scene
    public BuildingID buildingId;
    public Vector3 pos;
    public float rotation = 0;
    public int id;
    public Vector2 size;
    public IntVector2 center;
    public bool isRendered = false;
    public int cellSizeX;
    public int cellSizeZ;
    public Vector3 endPos;
    public VABuildingType buildingType;
    public Vector3 frontDoorPos;

    public List<CreatItemInfo> itemInfoList;
    public List<Vector3> npcPos;
    public Dictionary<int, int> npcIdNum;
    public List<Vector3> nativePos;
    public Dictionary<int, int> nativeIdNum;

    //the param to compute the block
    public Vector3 root;//the point which is the axis of rotation
    public Vector3 rootSize;

	//native tower
    public int pathID;
	public int campID;
	public int damageID;
    public VABuildingInfo() { }

    public VABuildingInfo(Vector3 pos, float rot, int id, BuildingID buildingId, VABuildingType type, VArtifactUnit vau, Vector2 size)
    {
        this.pos = pos;
        this.id = id;
        this.rotation = rot;
        buildingType = type;

        center = new IntVector2();
        center.x = Mathf.FloorToInt(this.pos.x);
        center.y = Mathf.FloorToInt(this.pos.z);

        root = pos;
        this.size = size;
//        int length = Mathf.CeilToInt(size.x);
        int width = Mathf.CeilToInt(size.y);
        int distance = width / 2 + 3;
        //--to do:get front door pos
        frontDoorPos = root + new Vector3(distance * Mathf.Sin(rot * Mathf.PI / 180), 0, distance * Mathf.Cos(rot * Mathf.PI / 180));

        this.buildingId = buildingId;
        this.vau = vau;
    }


    public bool isBulidingArea(IntVector2 posXZ)
    {
        bool flag = false;
        if (posXZ.x >= pos.x && posXZ.y >= pos.z && posXZ.x <= endPos.x && posXZ.y <= endPos.z)
        {
            flag = true;
        }
        return flag;
    }

    public void setHeight(float top)
    {
        pos.y = top;
        endPos.y = top;
        root.y = top;
    }
    public float getHeight()
    {
        return pos.y;
    }

    //internal static object DeserializeInfo(uLink.BitStream stream, params object[] codecOptions)
    //{
    //    IntVector2 pos = stream.Read<IntVector2>();
    //    int id = stream.Read<int>();
    //    int rot = stream.Read<int>();
    //    int cellSizeX = stream.Read<int>();
    //    int cellSizeZ = stream.Read<int>();
    //    BuildingID no = stream.Read<BuildingID>();

    //    BuildingInfo buildingInfo = new BuildingInfo(pos, id, rot, cellSizeX, cellSizeZ, no);

    //    return buildingInfo;
    //}

    //internal static void SerializeInfo(uLink.BitStream stream, object value, params object[] codecOptions)
    //{
    //    BuildingInfo buildingInfo = value as BuildingInfo;
    //    stream.Write<IntVector2>(new IntVector2(Mathf.RoundToInt(buildingInfo.pos.x), Mathf.RoundToInt(buildingInfo.pos.z)));
    //    stream.Write<int>(buildingInfo.id);
    //    stream.Write<int>(buildingInfo.rotation);
    //    stream.Write<int>(buildingInfo.cellSizeX);
    //    stream.Write<int>(buildingInfo.cellSizeZ);
    //    //stream.Write<uint>(buildingInfo.buildingNo);
    //}


    public void OnSpawned(GameObject obj)
    {
        //native_barrack nb = obj.GetComponent<native_barrack>();
        //nb.SetTownId(vau.vat.townId);
    }

}
