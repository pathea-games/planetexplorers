using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public partial class VFDataRTGen 
{
    private static bool IsHillTerrain(int nTerType)
    {
        switch (nTerType)
		{
            case 4://top of mountain
            case 3://mountain
                return true;
        }
        return false;
    }

    private static bool IsMountainTop(int nTerType)
    {
		if (nTerType == 4)
        {
            return true;
        }
        return false;
    }

    private static bool IsWaterTerrain(int nTerType)
    {
        if (nTerType == 0)
        {
            return true;
        }
        return false;
    }

    private static bool IsSeaSideTerrain(int nTerType)
    {
        if (nTerType == 1)
        {
            return true;
        }
        return false;
    }
	private static bool IsPlainTerrain(int nTerType){
		if(nTerType==2)
			return true;
		return false;
	}
}
