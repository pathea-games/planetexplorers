using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BGMStory : BGManager 
{
	const float compareValue = 2.0f;
    protected override int GetCurrentBgMusicID()
    {
		Vector3 camPos = PETools.PEUtil.MainCamTransform.position;
        if(AiUtil.CheckPositionInCave(camPos, 128.0f, AiUtil.groundedLayer))
            return 836;

        Vector2 pos = new Vector2(camPos.x, camPos.z);
		return AISpawnDataStory.GetBgMusicID(PeMappingMgr.Instance.GetAiSpawnMapId(pos));
    }
}
