using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using Pathea;
using PETools;

public class IKCombat : MonoBehaviour
{
    class IKData
    {
        public CCDIK ik;
        public IKCombatLimit limit;
        public Transform target;
        public string curve;
        public bool isPos;
    }

    //PeEntity m_Entity;
    Animator m_Animator;
    TargetCmpt m_Scanner;

    List<IKData> m_IKDataList;

    void Start()
    {
        //m_Entity = PEUtil.GetComponent<PeEntity>(gameObject);

        m_Animator = GetComponent<Animator>();
		m_Scanner = GetComponent<TargetCmpt>();
        m_IKDataList = new List<IKData>();
    }

    void Update()
    {
        foreach (IKData ikData in m_IKDataList)
        {
            if (!ikData.isPos)
            {
                ikData.isPos = true;
                ikData.ik.solver.SetIKPosition(GetIKPosition(ikData));
            }
            ikData.ik.solver.SetIKPositionWeight(m_Animator.GetFloat(ikData.curve));
        }
    }

    Vector3 GetIKPosition(IKData ikData)
    {
        Vector3 dir = ikData.target.position - ikData.ik.transform.position;
        //Vector3 axis = ikData.limit.transform.TransformDirection(ikData.limit.axis);
        Vector3 axis = ikData.limit.pivot;
        float angle = Vector3.Angle(axis, dir);
        if (angle > ikData.limit.limit)
        {
            Vector3 tmpAxis = Vector3.Cross(axis, dir);
            Vector3 tmpDir = Quaternion.AngleAxis(ikData.limit.limit, tmpAxis) * axis;
            return ikData.limit.transform.position + tmpDir.normalized * ikData.limit.distance;
        }
        else
        {
            return ikData.target.position;
        }
    }

    Transform GetIKTransform()
    {
        if (m_Scanner != null)
        {
            Enemy enemy = m_Scanner.GetAttackEnemy();
            if (!Enemy.IsNullOrInvalid(enemy))
            {
                return enemy.CenterBone;
            }
        }

        return null;
    }

    void ActivateCombatIK(string data)
    {
        string[] args = PEUtil.ToArrayString(data, '|');

        string[] datas = PETools.PEUtil.ToArrayString(args[1], ',');
        foreach (string item in datas)
        {
            Transform tr = PETools.PEUtil.GetChild(transform, item);
            if (tr != null)
            {
                CCDIK ik = tr.GetComponent<CCDIK>();

                if (ik != null)
                {
                    IKData ikData = m_IKDataList.Find(ret => ret.ik == ik);
                    if (ikData == null)
                    {

                        IKData newIKData = new IKData();
                        newIKData.ik = ik;
                        newIKData.limit = newIKData.ik.GetComponent<IKCombatLimit>();
                        newIKData.target = GetIKTransform();
                        newIKData.curve = args[0];

                        //newIKData.ik.solver.target = GetIKTransform(GetTarget());
                        //newIKData.ik.solver.SetIKPosition(GetIKPosition(newIKData));

                        m_IKDataList.Add(newIKData);
                    }
                }
            }
        }
    }

    void DeactivateCombatIK(string data)
    {
        string[] args = PEUtil.ToArrayString(data, '|');
        string[] datas = PETools.PEUtil.ToArrayString(args[1], ',');
        foreach (string item in datas)
        {
            Transform tr = PETools.PEUtil.GetChild(transform, item);
            if (tr != null)
            {
                CCDIK ik = tr.GetComponent<CCDIK>();

                if (ik != null)
                {
                    IKData ikData = m_IKDataList.Find(ret => ret.ik == ik);
                    if (ikData != null)
                    {
                        ikData.ik.solver.target = null;
                        m_IKDataList.Remove(ikData);
                    }
                }
            }
        }
    }
}
