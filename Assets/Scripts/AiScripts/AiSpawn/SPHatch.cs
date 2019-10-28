using UnityEngine;
using System.Collections;

public class SPHatch : SPGroup
{
    public int id;
    public int minCount;
    public int maxCount;
    public float radius;
    public float delayTime;

    AIResource res;
    //Ai_hatch hatch;

    public override IEnumerator SpawnGroup()
    {
        if (res == null) yield break;

        yield return new WaitForSeconds(delayTime);

        //if (hatch != null && hatch.dead)
        //    yield break;

        //hatch.ApplyDamage(hatch.maxLife + 100.0f);

        int count = Random.Range(minCount, maxCount);

        for (int i = 0; i < count; i++)
        {
            Vector3 position = transform.position + Random.insideUnitSphere * radius;

            if (AiUtil.CheckPositionOnGround(ref position, 10.0f, AiUtil.groundedLayer))
            {
                AIResource.Instantiate(id, position, Quaternion.identity, OnSpawned);
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForSeconds(0.5f);
        yield break;
    }

    void OnSpawned(GameObject go)
    {
        if (go == null) return;

        //go.transform.parent = AiManager.Manager.transform;

        //AiObject aiObject = go.GetComponent<AiObject>();

        //if (aiObject != null)
        //{
        //    aiObject.Delete(120.0f);
        //}
    }

    void Awake()
    {
        res = AIResource.Find(id);
        //hatch = GetComponent<Ai_hatch>();
    }
}
