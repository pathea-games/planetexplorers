using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WhiteCat.UnityExtension;
using Pathea;

public class L1Manager : MonoBehaviour
{

    public static L1Manager instance;
    public Dictionary<int, bool> isTrigger;
    Dictionary<int, Vector3> triggerPoint;
    [HideInInspector]public List<PeEntity> followers;

    void Start()
    {
        instance = this;
        instance.isTrigger = new Dictionary<int, bool>();
        instance.triggerPoint = new Dictionary<int, Vector3>();
        FindPointsTriggers();
        followers = new List<PeEntity>();
    }

    private void FindPointsTriggers()
    {
        transform.TraverseHierarchy(delegate(Transform trans, int n)
        {
            string name = trans.gameObject.name;
            if (name.Length < 6 || name.Substring(0, 5) != "Point")
                return;
            int num = -1;
            if (IsNumberic(name.Substring(5, 1), out num))
            {
                if(!triggerPoint.ContainsKey(num))
                    triggerPoint.Add(num, trans.position);
            }
        });
        for (int i = 1; i < 7; i++)
        {
            instance.isTrigger.Add(i, false);
        }
    }

    private bool IsNumberic(string s, out int result)
    {
        result = -1;
        try
        {
            result = Convert.ToInt32(s);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void FindFollowers(List<PeEntity> tmp)
    {
        foreach (var item in MissionManager.Instance.m_PlayerMission.followers)
        {
            if (item != null)
                tmp.Add(item.Follwerentity);
        }
        foreach (var item in ServantLeaderCmpt.Instance.mFollowers)
        {
            if (item != null)
                tmp.Add(item.Follwerentity);
        }
        foreach (var item in ServantLeaderCmpt.Instance.mForcedFollowers)
        {
            if (item != null)
                tmp.Add(item.Follwerentity);
        }
    }

    public void SetPosition(int triggerNum)
    {
        foreach (var item in followers)
        {
            Pathea.PeEntityExtTrans.PeEntityExtTrans.ExtSetPos(item, triggerPoint[triggerNum]);
        }
    }
}
