using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public partial class VFDataRTGen
{

    public static void OptimiseMaxHeight(ref int maxHeight,float voxelX, float voxelZ,float flatParam,float fTerType, int nTerType, float fTerNoise,float bottomHeight,
        float[] fTerNoiseHeight,float[] fTerDensityClamp,float PlainThickness,int minHeight =5)
    {
		//int optimiseCount =0;
        byte volumeThreshold = 64;
        //float bottomHeight;
        //if(IsHillTerrain(nTerType)){
        //    bottomHeight = PlainMax-20;
        //}else{
        //    bottomHeight = SeaDepth + 1;
        //}
        float tryHeightPlus = maxHeight-bottomHeight;
        while(tryHeightPlus>minHeight){
            int vy = Mathf.Clamp(Mathf.RoundToInt(bottomHeight + tryHeightPlus / 2) - 1, 0, fTerNoiseHeight.Count()-1);
            byte volume = 0;
            byte type = 0;
            GenTileVoxelOnly(voxelX,fTerNoiseHeight[vy],voxelZ,flatParam,
                fTerDensityClamp[vy], fTerType, nTerType, fTerNoise, ref volume, ref type, PlainThickness);
            if (volume < volumeThreshold)
            {
                
            }else{
                bottomHeight += tryHeightPlus/2;
            }
            tryHeightPlus /= 2;
			//optimiseCount++;
        }
		//Debug.Log(optimiseCount);
        maxHeight = Mathf.Clamp(Mathf.RoundToInt(bottomHeight + tryHeightPlus), 1, s_noiseHeight - 1);
    }

    private static float GetFlatParamFromFactor(float factor,int x,int y,RandomMapType firstType=RandomMapType.GrassLand)
    {
		float flatFactor = factor*factor*factor;
		float flatParam;
		if(flatFactor<0)
			flatParam =flatMin + (flatMid - flatMin) * (1+flatFactor);
		else
			flatParam =flatMid + (flatMax- flatMid) * flatFactor;
//		float flatParam =flatMid;

//		if(firstType==RandomMapType.Desert)
//			flatParam*=0.5f;
//		else if(firstType==RandomMapType.Crater)
//			flatParam*=1.2f;
//		else if(firstType == RandomMapType.Mountain)
//			flatParam*=1.5f;
		if(flatParam>TownConnectionFlatMin)
		{
			float factorValue = GetTownConnectionFactor(x,y,TownConnectionFlatDistance);
			if(factorValue<1)
			flatParam = TownConnectionFlatMin;
		}
			
        return flatParam;
    }


    private static float GetThicknessParamFromFactor(float factor)
    {
        //float absFactor = Mathf.Abs(factor);
        //float absThreshold = 0.75f;
        //if (absFactor > absThreshold)
        //    return 0;
        //float p1 = (absFactor - absThreshold / 2) / (absThreshold / 2);
        //float param = 1 - Mathf.Abs(Mathf.Pow(p1, 3));

        //float param = (Mathf.Pow(factor, 3)+1)*0.5f;
		float param = (factor*factor*factor+1)*0.5f;
        return param;
    }

    private static float GetHASParamFromFactor(float factor)
    {
		factor = (factor+1)*0.5f;
		if(factor>=0.9f)
			return 1;
		float value1 = factor/0.9f*Mathf.PI-Mathf.PI/2;
		float sinValue = Mathf.Sin(value1);
//		float value2 = Mathf.Pow(Mathf.Sin(value1),3);
		float value2 = sinValue*sinValue*sinValue;
		if(sinValue>0)
			value2=value2*sinValue*sinValue;
		return value2;

//        factor = Mathf.Pow(Mathf.Abs(factor), 6);
//        return factor;
    }

	private static float GetSlideBarValue(float value,float mid){//0~100
		// mid/4   mid/2   mid   2mid   4mid
		//p0       p1      p2    p3    p4 
		float p0= mid/4;
		float p1 = mid/2;
		float p2 = mid;
		float p3 = mid*2;
		float p4 = mid*4;

		if(value<=25)
			return (value-1)/24*(p1-p0)+p0;
		if(value<=50)
			return (value-25)/25*(p2-p1)+p1;
		if(value<=75)
			return (value-50)/25*(p3-p2)+p2;
		if(value<=100)
			return (value-75)/25*(p4-p3)+p3;
		return value;
	}

	private static float GetSlideBarValue(float value,float min,float max){//1~100
		// min    ?    ?   ?    max
		float coef = Mathf.Pow(max/min,0.25f);
		float p0 = min;
		float p1 = min*coef;
		float p2 = p1*coef;
		float p3 = p2*coef;
		float p4= max;

		if(value<=25)
			return (value-1)/24*(p1-p0)+p0;
		if(value<=50)
			return (value-25)/25*(p2-p1)+p1;
		if(value<=75)
			return (value-50)/25*(p3-p2)+p2;
		if(value<=100)
			return (value-75)/25*(p4-p3)+p3;
		return value;
	}

	private static float GetSlideBarValue(float value,float mid,float min,float max){//1~100
		// min   x*min   mid   x*mid   4mid
		float coef1 = Mathf.Sqrt(mid/min);
		float coef2 = Mathf.Sqrt(max/mid);

		float p0 = min;
		float p1 = min*coef1;
		float p2 = mid;
		float p3 = p2*coef2;
		float p4= max;

		if(value<=25)
			return (value-1)/24*(p1-p0)+p0;
		if(value<=50)
			return (value-25)/25*(p2-p1)+p1;
		if(value<=75)
			return (value-50)/25*(p3-p2)+p2;
		if(value<=100)
			return (value-75)/25*(p4-p3)+p3;
		return value;
	}
}
