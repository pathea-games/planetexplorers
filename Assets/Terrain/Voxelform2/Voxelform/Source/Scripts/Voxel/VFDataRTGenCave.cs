using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public partial class VFDataRTGen 
{

    private void GenCave(float caveHeight, float caveThickness, byte[] yVoxels) 
    {
		byte surfaceVolumeThreshold1 = 225;//225;
		byte surfaceVolumeThreshold2 = 165;//165;
        //testHeight
		if(caveHeight<0)
			caveHeight=0;
        int maxHeight1 = Mathf.FloorToInt(caveHeight);
		for (int testHeight = Mathf.FloorToInt(caveHeight), vy2 = testHeight*VFVoxel.c_VTSize; 
		     testHeight < Mathf.CeilToInt(caveHeight+caveThickness); 
		     testHeight++,vy2+=VFVoxel.c_VTSize)
        {
			if(vy2<yVoxels.Length&&yVoxels[vy2]>surfaceVolumeThreshold1){
				maxHeight1++;
			}else{
				maxHeight1=testHeight;
				break;
			}
        }

        int maxHeight2 = Mathf.CeilToInt(caveHeight + caveThickness);
		for (int testHeight = maxHeight2, vy2 = testHeight*VFVoxel.c_VTSize; 
		     testHeight > Mathf.FloorToInt(caveHeight); 
		     testHeight--,vy2-=VFVoxel.c_VTSize)
        {
            if (vy2 < yVoxels.Length && yVoxels[vy2] > surfaceVolumeThreshold2)
            {
                maxHeight2 = testHeight;
                break;
            }
            maxHeight2--;
        }
//        maxHeight2-=7;

		int voxelTop = yVoxels.Length/2-1;
		for(int testHeight = voxelTop,vy2 = testHeight*VFVoxel.c_VTSize;
		    testHeight>0;
		    testHeight--,vy2-=VFVoxel.c_VTSize)
		{
			voxelTop = testHeight;
			if(yVoxels[vy2]>surfaceVolumeThreshold2)
				break;
		}
		int maxHeight3 = voxelTop-17;


		int maxHeight = Mathf.Min(maxHeight1, maxHeight2,maxHeight3);
		for (int vy = Mathf.CeilToInt(caveHeight), vy2 = vy*VFVoxel.c_VTSize; vy < maxHeight; vy++,vy2+=VFVoxel.c_VTSize)
        {

            //byte volume = yxVoxels[vy][vx2];
            //byte type = yxVoxels[vy][vx2 + 1];
            if (vy>caveHeight+1)
                yVoxels[vy2] = 0;

            if (vy == Mathf.CeilToInt(caveHeight))
            {
                yVoxels[vy2] = (byte)Mathf.RoundToInt(255 * (caveHeight - Mathf.FloorToInt(caveHeight)));
            }
            if (yVoxels[vy2] < 127.5f)
            {
                yVoxels[vy2 + 1] = BLOCK_AIR;
            }
        }
    }
}
