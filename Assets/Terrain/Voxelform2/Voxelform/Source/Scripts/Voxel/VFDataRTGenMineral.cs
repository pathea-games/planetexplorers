using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoxelPaintXML;


public partial class VFDataRTGen 
{
	//产量:煤>铁=铜>铝=银>金>石油>锌=锂>硫=钻>钛
	//价值:硫  煤  铜 铁   锌  钛  银 石油 锂  金   钻  铝  升序排列
	const int coalIndex = 0;
	const int ironIndex = 1;
	const int copperIndex = 2;
	const int aluminumIndex = 3;
	const int silverIndex = 4;
	const int goldIndex = 5;
	const int oilIndex = 6;
	const int zincIndex = 7;
	const int lithiumIndex = 8;
	const int sulfurIndex = 9;
	const int diamondIndex = 10;
	const int titaniumIndex = 11;
	static float mineGenChanceFactor =1.5f;
    private void GeneralMineral(byte[] yVoxels, float voxelX, float[] voxelYs, float voxelZ,float mineChanceFactor,float ftertype,RandomMapType maptype)
	{ 
		MineChanceArrayCLS[] regionMinePercentDesc = regionArray[(int)maptype-1].MineChanceArrayValues;
		float regionMineChance = regionArray[(int)maptype-1].mineChance;
        int nChance = regionMinePercentDesc.Length;
		int maxY = yVoxels.Length>>VFVoxel.c_shift;
        //start from coal
        for (int j = 0; j < nChance; j++)
        {
			float SecondChance = regionMineChance * mineGenChanceFactor;
            
            //int i = j;
            int i = j;
            int noiseIndex = i + MineStartNoiseIndex;
            //float mineChance = (float)myNoise[noiseIndex].Noise(voxelX * minePerturbanceFrequency0, voxelY * minePerturbanceFrequency0, voxelZ * minePerturbanceFrequency0);//��Ĵ�ŷֲ���״
            float mineChance = (float)myMineNoise[noiseIndex].Noise(voxelX * minePerturbanceFrequency0, voxelZ * minePerturbanceFrequency0);//��Ĵ�ŷֲ���״
			float chance;
			if(mineChance<0)
				chance = -mineChance;
			else
				chance = mineChance;

            if (chance < SecondChance)
            {
                float metalReduce = 1.0f;
            
//                if (metalReduceSwitch)
//                {
//                    if (j >= 2 && Mathf.Abs(voxelX / s_detailScale) < metalReduceArea && Mathf.Abs(voxelZ / s_detailScale) < metalReduceArea)
//                    {
//                        metalReduce = 1 - ((metalReduceArea - Mathf.Abs(voxelX / s_detailScale)) * (metalReduceArea - Mathf.Abs(voxelZ / s_detailScale))) / (metalReduceArea * metalReduceArea);
//                    }
//                }
                float mineChance2 = (float)myMineNoise[noiseIndex + nChance].Noise(voxelX * minePerturbanceFrequency1, voxelZ * minePerturbanceFrequency1);//
				float chance2 = mineChance2 * 100;
				if(mineChance2<0)
					chance2 *=-1;
                //chance2 = chance2 * chance;
				float genPercent = regionMinePercentDesc[i].perc;
//				if(maptype==RandomMapType.Desert && i==oilIndex){
//					genPercent +=5;
//				}
//				if((maptype == RandomMapType.Forest||maptype==RandomMapType.Rainforest)&& i ==coalIndex){
//					genPercent +=5;
//				}
				float newChance = genPercent * metalReduce;
                //float newChance = 1.2f*terMinePercentDesc[nTerType][i].perc * agglomeration;

                if (chance2 < newChance)
                {
                    float heightFactor = (float)myMineNoise[HEIGHT_INDEX].Noise(voxelX * mineHeightFrequency, voxelZ * mineHeightFrequency);
                    float thicknessFactor = (float)myMineNoise[THICKNESS_INDEX].Noise(voxelX * mineThicknessFrequency, voxelZ * mineThicknessFrequency);
                    float quantityFactor = (float)myMineNoise[QUANTITY_INDEX].Noise(voxelX * mineQuantityFrequency, voxelZ * mineQuantityFrequency);
                    float chanceFactor = (SecondChance - chance) / SecondChance * (newChance - chance2) / newChance * mineChanceFactor;

					int midVy =Mathf.RoundToInt((MineStartHeightList[i]*PlainMax(PlainThickness)/(s_seaDepth+40))+heightFactor*HeightOffsetMax+HeightOffsetTer*ftertype*ftertype);
                    float thickness = MineThickness+ThicknessOffsetMax*thicknessFactor;
                    float quantity = 1 / ((quantityFactor + 1) * 0.3f);//(0,1)->(1,NaN)
                    float chanceDecrease;
                    if (chanceFactor > 0.3f)
                    {
                        chanceDecrease = 1;
                    }
                    else
                    {
						//chanceDecrease= Mathf.Pow(chanceFactor/0.3f,0.5f);
						chanceDecrease= Mathf.Sqrt(chanceFactor/0.3f);
                    }
                    thickness *= chanceDecrease;
                    int startVy = Mathf.Clamp(Mathf.RoundToInt(midVy - thickness / 2), 0, 511);
					int endVy = startVy + (int)(thickness/2) + 1;
					if(endVy > maxY)	endVy = maxY;

                    float vyValue = startVy;
					for (int vy = startVy; vy < endVy;)
                    {
						int vy2 = vy*VFVoxel.c_VTSize;
                        if (yVoxels[vy2] > 127.5f)
                        { 
                            yVoxels[vy2+1] = (byte)(regionMinePercentDesc[i].type);
                        }
                        vyValue += quantity;
                        vy = Mathf.RoundToInt(vyValue);
                    }
                    break;
                }
            }
        }
    }
    
}
