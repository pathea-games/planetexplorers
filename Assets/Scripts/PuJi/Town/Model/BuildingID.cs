using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BuildingID
{
    public int townId;
    public int buildingNo;

    public BuildingID()
    {
    }

    public BuildingID(int townId, int buildingNo)
    {
        this.townId = townId;
        this.buildingNo = buildingNo;
    }

    public override bool Equals(object obj)
    {
        if (null == obj)
            return false;
        BuildingID vec = (BuildingID)obj;
        //if(vec != null)
        {
            return townId == vec.townId && buildingNo == vec.buildingNo;
        }
        //return false;
    }

    internal static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
    {
        int townId = stream.Read<int>();
        int buildingNo = stream.Read<int>();
        BuildingID buildingID = new BuildingID(townId, buildingNo);
        return buildingID;
    }

    internal static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
    {
        BuildingID buildingID = value as BuildingID;
        stream.Write<int>(buildingID.townId);
        stream.Write<int>(buildingID.buildingNo);
    }

    public override int GetHashCode()	// In Dictionary, x,y must be unmodifiable to keep hash code constant
    {
        return townId + (buildingNo << 16);
    }
    public override string ToString()
    {
        return string.Format("[{0}-{1}]", townId, buildingNo);
    }
}
