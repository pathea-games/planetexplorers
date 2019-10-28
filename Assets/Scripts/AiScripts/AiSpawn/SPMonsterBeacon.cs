using UnityEngine;
using System.Collections;

public class SPMonsterBeacon : SPAutomatic
{
    public int id;
    public float delayTime;
    public float minRadius;
    public float maxRadius;

    protected override SPPoint Spawn(AISpawnData spData)
    {
        base.Spawn(spData);

        Vector3 pos;
        Quaternion rot;
        if (GetPositionAndRotation(out pos, out rot, spData.minAngle, spData.maxAngle))
        {
            //if (!GameConfig.IsMultiMode)
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
        }

        return null;
    }

    public override void OnSpawned(GameObject obj)
    {
        base.OnSpawned(obj);

        AiObject aiObj = obj.GetComponent<AiObject>();
        if (aiObj != null)
        {
            aiObj.tdInfo = transform;
        }

        SPGroup spGroup = obj.GetComponent<SPGroup>();
        if (spGroup != null)
        {
            spGroup.tdInfo = transform;
        }
    }

    protected override void OnSpawnComplete()
    {
        base.OnSpawnComplete();

        //DragItem.Mgr.Instance.RemoveByGameObject(gameObject);

        Delete();
    }

    protected override void OnTerrainExit(IntVector4 node)
    {
        base.OnTerrainExit(node);

        if (!GameConfig.IsMultiMode)
        {
            //ItemScript item = GetComponent<ItemScript>();
            //if (item != null)
            //{
            //    DragItem.Mgr.Instance.Remove(item);
            //}
            Delete();
        }
    }

    bool GetPositionAndRotation(out Vector3 pos, out Quaternion rot, float minAngle, float maxAngle)
    {
        pos = Vector3.zero;
        rot = Quaternion.identity;

        pos = AiUtil.GetRandomPosition(transform.position, minRadius, maxRadius,
                                        Vector3.forward, minAngle, maxAngle, 10.0f,
                                        AiUtil.groundedLayer, 5);

        if (pos != Vector3.zero)
            return true;

        pos = AiUtil.GetRandomPosition(transform.position, minRadius, maxRadius, Vector3.forward, minAngle, maxAngle);

        if (pos != Vector3.zero)
            return true;

        return false;
    }

    new public void Awake()
    {
        base.Awake();

        ID = id;
        Delay = delayTime;
    }
}
