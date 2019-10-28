using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#region model
public class RandomMapTypePoint{
	public RandomMapType type;
	public List<IntVector2> posList=new List<IntVector2> ();

	public RandomMapTypePoint(RandomMapType type){
		this.type = type;
	}

	public RandomMapTypePoint(RandomMapType type,IntVector2 pos){
		this.type = type;
		posList.Add(pos);
	}

	public void AddPos(IntVector2 pos){
		posList.Add(pos);
	}

	public float GetDistance(IntVector2 pos){
		float distance = float.MaxValue;
		foreach(IntVector2 p in posList){
			float tmpDist = p.Distance(pos);
			if(tmpDist<distance)
				distance = tmpDist;
		}
		return distance;
	}
}
public struct RandomMapTypeDist:IComparable
{
	public RandomMapType type;
	public float distance;

	public RandomMapTypeDist(RandomMapType type,float dist){
		this.type = type;
		distance = dist;
	}

	public int CompareTo(object obj) {
		int result;
		try
		{
			RandomMapTypeDist info = (RandomMapTypeDist)obj;
			if(this.distance<info.distance)
				result = -1;
			else if(this.distance>info.distance)
				result = 1;
			else
			{
				if(this.type<info.type)
					result = -1;
				else
					result = 1;
			}
			return result;
		}
		catch (Exception ex) { throw new Exception(ex.Message); }
	}
}

#endregion

public class TerrainUtil
{
    public static float VolumeToHeight(byte volumeUp,byte volumeDown)
    {
        if (volumeUp > 127.5f && volumeDown > 127.5f)
        {
            return -1;
        }
        if (volumeUp < 127.5f && volumeDown < 127.5f)
        {
            return -1;
        }
        if (volumeUp == 128)
        {
            return 1;
        }
        if (volumeDown == 128)
        {
            return 0;
        }

        return ((volumeDown * 1.0f - 128) / (128 - volumeUp)) / (1 + (volumeDown * 1.0f - 128) / (128 - volumeUp));
    }

    public static byte HeightToVolume(float height)
    {
        byte volumeDown = 255;
        if(height ==1){
            return 128;
        }
        return (byte)(128 - (volumeDown - 128) * (1 - height) / height);
    }
}
