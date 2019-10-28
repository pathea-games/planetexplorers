using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SPPointSimulate : SPPointMovable
{
    //float damage;
    //float minInterval;
    //float maxInterval;
    //float radius;

    //float mDamage;
    float mHp;
    float mMaxHp;

    public override void Init(IntVector4 idx, Transform parent = null, 
        int spid = 0, int pathid = 0, bool isActive = true, bool revisePos = true, 
        bool isBoss = false, bool isErode = true, bool isDelete = true, SimplexNoise noise = null, 
        AssetReq.ReqFinishDelegate onSpawned = null, CommonInterface common = null)
    {
        base.Init(idx, parent, spid, pathid, isActive, revisePos, isBoss, isErode, isDelete, noise, onSpawned, common);

        if (pathid > 0)
        {
            AiAsset.AiDataBlock aiData = AiAsset.AiDataBlock.GetAIDataBase(pathid);
            if (aiData != null)
            {
                //mDamage = aiData.damageSimulate;
                mMaxHp = aiData.maxHpSimulate;
                mHp = mMaxHp;
            }
        }

        if (spid > 0)
        {
            AISpawnPath path = AISpawnPath.GetSpawnPath(spid);
            {
                if (path != null)
                {
                    //mDamage = path.damage;
                    mMaxHp = path.maxHp;
                    mHp = mMaxHp;
                }
            }
        }
    }

    protected override void OnSpawnedChild(GameObject obj)
    {
        base.OnSpawnedChild(obj);

        AiObject aiObj = obj.GetComponent<AiObject>();
        if (aiObj != null)
        {
            aiObj.lifePercent = hpPercent;
        }
    }

    protected override void OnSpawned(GameObject obj)
    {
        base.OnSpawned(obj);

        if (aiObject != null)
        {
            aiObject.lifePercent = hpPercent;
        }
    }

    #region public interface

    public void SetData(float argDamage, float argMinInterval, float argMaxInterval, float argRadius)
    {
        //damage = argDamage;
        //minInterval = argMinInterval;
        //maxInterval = argMaxInterval;
        //radius = argRadius;
    }

    public void ApplyDamage(float value)
    {
        if (aiObject != null)
        {
            hpPercent = aiObject.lifePercent;
        }

        mHp = Mathf.Clamp(mHp - value, 0.0f, mMaxHp);

        if (aiObject != null)
        {
            aiObject.lifePercent = hpPercent;
        }

        if (hpPercent < PETools.PEMath.Epsilon)
        {
            Delete();
        }
    }

    public bool isDamage
    {
        get
        {
            return hpPercent > PETools.PEMath.Epsilon && (clone == null || !clone.activeSelf);
        }
    }

    public float hpPercent
    {
        get { return mHp / mMaxHp; }
        set { mHp = Mathf.Clamp(mMaxHp * value, 0.0f, mMaxHp); }
    }

    #endregion

//    IEnumerator DamageSimulate()
//    {
//        while (true)
//        {
//            if (isDamage)
//            {
//                if (CSMain.s_MgCreator != null)
//                {
//                    CSMain.s_MgCreator.SimulatorMgr.ApplyDamage(damage);
//                }
//            }
//            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
//        } 
//    }

    public new void Start()
    {
        base.Start();
//        StartCoroutine(DamageSimulate());
    }
}
