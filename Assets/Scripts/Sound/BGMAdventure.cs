using UnityEngine;
using System.Collections;
using Pathea;
using System;

public class BGMAdventure : BGManager 
{
    protected override int GetCurrentBgMusicID()
    {
		Vector3 camPos = PETools.PEUtil.MainCamTransform.position;
        if(AiUtil.CheckPositionInCave(camPos, 128.0f, AiUtil.groundedLayer))
            return 836;

		int x = (int)camPos.x;
		int z = (int)camPos.z;
		if(x<=Int32.MinValue){
			Debug.LogError("x value too small!");
			x=Int32.MinValue+1;
		}
		if(z<=Int32.MinValue){
			Debug.LogError("z value too small!");
			z=Int32.MinValue+1;
		}

		if (VFDataRTGen.IsSea(x, z))
            return AISpawnDataStory.GetBackGroundMusic(new Color(255.0f / 255.0f, 100.0f / 255.0f, 150.0f / 255.0f, 255 / 255.0f));
        else
        {
			RandomMapType mapType = VFDataRTGen.GetXZMapType(x,z);

            switch (mapType)
            {
                case RandomMapType.GrassLand:   return AISpawnDataStory.GetBackGroundMusic(new Color(30.0f / 255.0f, 50.0f / 255.0f, 50.0f / 255.0f, 255 / 255.0f));
                case RandomMapType.Forest:      return AISpawnDataStory.GetBackGroundMusic(new Color(70.0f / 255.0f, 70.0f / 255.0f, 70.0f / 255.0f, 255 / 255.0f));
                case RandomMapType.Desert:      return AISpawnDataStory.GetBackGroundMusic(new Color(140.0f / 255.0f, 100.0f / 255.0f, 50.0f / 255.0f, 255 / 255.0f));
                case RandomMapType.Redstone:    return AISpawnDataStory.GetBackGroundMusic(new Color(170.0f / 255.0f, 70.0f / 255.0f, 50.0f / 255.0f, 255 / 255.0f));
                case RandomMapType.Rainforest:  return AISpawnDataStory.GetBackGroundMusic(new Color(90.0f / 255.0f, 90.0f / 255.0f, 90.0f / 255.0f, 255 / 255.0f));
                case RandomMapType.Mountain:    return AISpawnDataStory.GetBackGroundMusic(new Color(170.0f / 255.0f, 70.0f / 255.0f, 150.0f / 255.0f, 255 / 255.0f));
                case RandomMapType.Swamp:       return AISpawnDataStory.GetBackGroundMusic(new Color(100.0f / 255.0f, 50.0f / 255.0f, 50.0f / 255.0f, 255 / 255.0f));
                case RandomMapType.Crater:      return AISpawnDataStory.GetBackGroundMusic(new Color(180.0f / 255.0f, 180.0f / 255.0f, 180.0f / 255.0f, 255 / 255.0f));
                default: return 0;
            }
        }
    }
}
