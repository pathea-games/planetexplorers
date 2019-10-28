using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SPMission : SPAutomatic
{
    float mRadius = 0.0f;

    public static SPMission InstantiateMission(Vector3 position, float radius, int id, float delayTime = 0.0f, bool isPlay = true)
    {
        GameObject obj = new GameObject("SPMission");
        SPMission spMission = obj.AddComponent<SPMission>() as SPMission;
        obj.transform.position = position;

        spMission.mRadius = radius;

        spMission.ID = id;
        spMission.Delay = delayTime;

        if (isPlay)
        {
            spMission.SpawnAutomatic();
        }

        return spMission;
    }

    protected override SPPoint Spawn(AISpawnData spData)
    {
        base.Spawn(spData);

        Vector3 pos;
        Quaternion rot;
        if (GetPositionAndRotation(out pos, out rot))
        {
            SPPointMovable movable = SPPoint.InstantiateSPPoint<SPPointMovable>(pos,
                                                                                rot,
                                                                                IntVector4.Zero,
                                                                                pointParent,
                                                                                spData.isPath ? 0 : spData.spID,
                                                                                spData.isPath ? spData.spID : 0,
                                                                                true,
                                                                                true,
                                                                                false,
                                                                                false,
                                                                                true,
                                                                                null,
                                                                                OnSpawned,
                                                                                this) as SPPointMovable;

            movable.target = transform;

            return movable;
        }

        return null;
    }

    protected override void OnSpawnComplete()
    {
        base.OnSpawnComplete();

        Delete();
    }

    bool GetPositionAndRotation(out Vector3 pos, out Quaternion rot)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;

        pos = AiUtil.GetRandomPosition(transform.position, 0.0f, mRadius, 10.0f, AiUtil.groundedLayer, 5);

        if (pos != Vector3.zero)
            return true;

		pos = AiUtil.GetRandomPosition(transform.position, 0.0f, mRadius);
		
		if (pos != Vector3.zero)
			return true;

        return false;
    }
}
