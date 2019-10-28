using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SPFaeces : MonoBehaviour 
{
    public float probability;
    public float interval;
    public GameObject[] faeces;

    Shader transparent;

    List<IntVector2> mExists = new List<IntVector2>();
    List<GameObject> mFaeces = new List<GameObject>();

    bool MatchFaece(IntVector4 node, GameObject obj)
    {
        if (obj == null)
            return false;

        float dx = obj.transform.position.x - node.x;
        //float dy = obj.transform.position.y - node.y;
        float dz = obj.transform.position.z - node.z;

        return dx >= PETools.PEMath.Epsilon && dx <= VoxelTerrainConstants._numVoxelsPerAxis << node.w
            /*&& dy >= PETools.PEMath.Epsilon && dy <= VoxelTerrainConstants._numVoxelsPerAxis << node.w*/
            && dz >= PETools.PEMath.Epsilon && dz <= VoxelTerrainConstants._numVoxelsPerAxis << node.w;
    }

    bool Match(AiObject aiObj)
    {
        //if (aiObj == null || !aiObj.isActive || aiObj.enemy != null || aiObj.dead)
        //    return false;

        return AiUtil.GetChild(aiObj.transform, "faece") != null;
    }

    //AiObject GetRandomAiObjectForFaece()
    //{
        //List<AiObject> aiObjs = AiManager.Manager.aiObjects.FindAll(ret => Match(ret));
        //if (aiObjs != null && aiObjs.Count > 0)
        //    return aiObjs[Random.Range(0, aiObjs.Count)];
        //else
        //    return null;
    //}

    IEnumerator DestroyFaece(GameObject faece, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (faece == null)
            yield break;

        MeshRenderer renderer = faece.GetComponentInChildren<MeshRenderer>();

        if (faece != null && transparent != null && renderer != null)
        {
            float alphaValue = 1.0f;
            while (Mathf.Abs(alphaValue) > PETools.PEMath.Epsilon && faece != null)
            {
                alphaValue = Mathf.Clamp01(alphaValue - 0.05f);

                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Color color = renderer.materials[i].color;
                    color.a = alphaValue;
                    renderer.materials[i].shader = transparent;
                    renderer.materials[i].color = color;
                }

                yield return new WaitForSeconds(0.1f);
            }
        }

        if(faece != null)
        {
            mFaeces.Remove(faece);
            GameObject.Destroy(faece);
        }
    }

    //IEnumerator SpawnFaecesForAiObject(AiMonster aiObj)
    //{
    //    Transform tr = AiUtil.GetChild(aiObj.transform, "faece");
    //    if(tr != null)
    //    {
    //        aiObj.CrossFade("faece", true);

    //        yield return new WaitForSeconds(aiObj.delayFaeceTime);

    //        GameObject faece = Instantiate(GetRandomFaeces(), tr.position, Quaternion.identity) as GameObject;
    //        faece.transform.parent = transform;

    //        mFaeces.Add(faece);

    //        StartCoroutine(DestroyFaece(faece, 400.0f));
    //    }
    //}

    //IEnumerator SpawnFaeces()
    //{
        //while (true)
        //{
        //    AiMonster aiObj = GetRandomAiObjectForFaece() as AiMonster;
        //    if (aiObj != null)
        //    {
        //        yield return SpawnFaecesForAiObject(aiObj);
        //    }
        //    yield return new WaitForSeconds(interval);
        //}
    //}

    GameObject GetRandomFaeces()
    {
        return faeces[Random.Range(0, faeces.Length)];
    }

    void RegisterEvent()
    {
        LODOctreeMan.self.AttachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
    }

    void RemoveEvent()
    {
        LODOctreeMan.self.DetachEvents(null, OnTerrainColliderDestroy, OnTerrainColliderCreated, null);
    }

    void OnTerrainColliderCreated(IntVector4 node)
    {
        if (node.w == 0)
        {
            IntVector2 mark = new IntVector2(node.x, node.z);
            if (!mExists.Contains(mark))
            {
                if (Random.value < probability)
                {
                    Vector3 pos = AiUtil.GetRandomPosition(node);

                    if (AiUtil.CheckPositionOnGround(ref pos,
                                                    0.0f,
                                                    VoxelTerrainConstants._numVoxelsPerAxis << node.w,
                                                    AiUtil.groundedLayer))
                    {
                        if(!AiUtil.CheckPositionUnderWater(pos))
                        {
                            GameObject faece = Instantiate(GetRandomFaeces(), pos, Quaternion.identity) as GameObject;
                            faece.transform.parent = transform;

                            mFaeces.Add(faece);

                            StartCoroutine(DestroyFaece(faece, 400.0f));
                        }
                    }
                }

                mExists.Add(mark);
            }

            List<GameObject> faeces = mFaeces.FindAll(ret => MatchFaece(node, ret));
            foreach (GameObject ite in faeces)
            {
                Vector3 pos = ite.transform.position;
                float distance = pos.y - node.y;
                if (distance > PETools.PEMath.Epsilon)
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(pos + Vector3.up * 0.1f, Vector3.down, out hitInfo, distance, AiUtil.groundedLayer))
                    {
                        if (ite.GetComponent<Rigidbody>() != null)
                        {
                            ite.GetComponent<Rigidbody>().useGravity = true;
                        }
                    }
                }
            }
        }
    }

    void OnTerrainColliderDestroy(IntVector4 node)
    {
        if (node.w == 0)
        {
            IntVector2 mark = new IntVector2(node.x, node.z);
            if (mExists.Contains(mark))
            {
                mExists.Remove(mark);
            }

            List<GameObject> faeces = mFaeces.FindAll(ret => MatchFaece(node, ret));
            foreach (GameObject ite in faeces)
            {
                if (ite.GetComponent<Rigidbody>() != null)
                {
                    ite.GetComponent<Rigidbody>().useGravity = false;
                }
            }
        }
    }

	// Use this for initialization
	void Start () 
    {
        transparent = Shader.Find("Transparent/Bumped Diffuse");
        //StartCoroutine(SpawnFaeces());
        RegisterEvent();
	}
	
	// Update is called once per frame
	void Destroy () 
{
        RemoveEvent();
	}
}
